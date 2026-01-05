using Microsoft.VisualBasic;
using Newtonsoft.Json;
using RMS.DTOs.Responses.AI;
using Constant = RMS.Shared.Constants.Constant;

namespace RMS.Services.AI.Service
{
    public class SchedulePredictor
    {
        private readonly GeminiApiClient _client;

        public SchedulePredictor(GeminiApiClient client)
        {
            _client = client;
        }

        public async Task<SchedulePredictionResult> PredictAsync(object scheduleData)
        {
            string dataJson = JsonConvert.SerializeObject(scheduleData);

            // Prompt Engineering: Yêu cầu AI đóng vai PM và trả về JSON
            string prompt = $@"
                Đóng vai Quản lý dự án xây dựng (PM). 
                Dựa trên dữ liệu tiến độ thực tế (Actual) so với kế hoạch (Baseline) dưới đây:
                {dataJson}

                Hãy phân tích và trả về kết quả định dạng JSON (không giải thích thêm) theo mẫu tất cả theo tiếng Việt :
                {{
                    ""Status"": ""OnTrack"" hoặc ""Delayed"" hoặc ""Ahead"",
                    ""DelayedDays"": số_ngày_dự_kiến_trễ (int),
                    ""RiskAnalysis"": ""Đánh giá rủi ro ngắn gọn (tiếng Việt và không có tiếng Anh)"",
                    ""Recommendation"": ""Đề xuất giải pháp (tiếng Việt)""
                }}
            ";

            string resultJson = await _client.GenerateContentAsync(prompt, Constant.GEMINI_MODEL_NAME_FLASH);
            return JsonConvert.DeserializeObject<SchedulePredictionResult>(resultJson);
        }
    }
}
