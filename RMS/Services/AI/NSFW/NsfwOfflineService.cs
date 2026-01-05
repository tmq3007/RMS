using Microsoft.ML;
using Microsoft.ML.Data;
using RMS.Services.AI.NSFW.Models;

namespace RMS.Services.AI.NSFW
{
    public class NsfwOfflineService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<NsfwInput, NsfwOutput> _predictionEngine;
        private readonly string _modelPath;

        private const string OnnxInputNode = "pixel_values";
        private const string OnnxOutputNode = "logits";

        public NsfwOfflineService()
        {
            _mlContext = new MLContext();
            _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "model.onnx");

            if (!File.Exists(_modelPath))
                throw new FileNotFoundException($"Không tìm thấy file model tại: {_modelPath}");

            // --- QUAN TRỌNG: Định nghĩa Shape cố định để sửa lỗi "Unknown dimension" ---
            // Model ViT input chuẩn: [BatchSize=1, Channels=3, Height=224, Width=224]
            var shapeDictionary = new Dictionary<string, int[]>
            {
                { OnnxInputNode, new[] { 1, 3, 224, 224 } }
            };

            var pipeline = _mlContext.Transforms.ResizeImages(
                                outputColumnName: "resized_image",
                                imageWidth: 224,
                                imageHeight: 224,
                                inputColumnName: nameof(NsfwInput.Image))
                           .Append(_mlContext.Transforms.ExtractPixels(
                                outputColumnName: OnnxInputNode,
                                inputColumnName: "resized_image",
                                // -------------------------------------------------------
                                // FIX 1: Model ONNX/PyTorch cần Planar (False), không phải Interleaved
                                interleavePixelColors: false,
                                // -------------------------------------------------------
                                offsetImage: 0,
                                scaleImage: 1f / 255f))
                           .Append(_mlContext.Transforms.ApplyOnnxModel(
                                modelFile: _modelPath,
                                outputColumnName: OnnxOutputNode,
                                inputColumnName: OnnxInputNode,
                                // -------------------------------------------------------
                                // FIX 2: Truyền ShapeDictionary để ép BatchSize = 1
                                shapeDictionary: shapeDictionary
                                // -------------------------------------------------------
                                ));

            // Load model
            var emptyData = _mlContext.Data.LoadFromEnumerable(new List<NsfwInput>());
            var model = pipeline.Fit(emptyData);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<NsfwInput, NsfwOutput>(model);
        }

        public (bool IsUnsafe, double Score) AnalyzeImage(byte[] imageBytes)
        {
            using (var stream = new MemoryStream(imageBytes))
            using (var mlImage = MLImage.CreateFromStream(stream))
            {
                var input = new NsfwInput { Image = mlImage };

                NsfwOutput result;
                lock (_predictionEngine)
                {
                    result = _predictionEngine.Predict(input);
                }

                var probabilities = Softmax(result.Prediction);

                // Index 1 là NSFW
                double nsfwScore = probabilities[1];

                // Ngưỡng chặn 0.75 (75%)
                bool isUnsafe = nsfwScore > 0.25;

                return (isUnsafe, nsfwScore);
            }
        }

        private double[] Softmax(float[] logits)
        {
            var max = logits.Max();
            var exp = logits.Select(x => Math.Exp(x - max)).ToArray();
            var sum = exp.Sum();
            return exp.Select(x => x / sum).ToArray();
        }
    }
}
