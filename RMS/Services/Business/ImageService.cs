using RMS.Services.AI.Service;

namespace RMS.Services.Business
{
    public class ImageService
    {
        private readonly GeminiApiClient _geminiClient;

        public ImageService(GeminiApiClient geminiClient)
        {
            _geminiClient = geminiClient;
        }

        public async Task UploadImage(IFormFile file)
        {
            if (file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var imageBytes = memoryStream.ToArray();

                    // === BƯỚC KIỂM TRA 18+ ===
                    bool isSafe = await _geminiClient.CheckImageSafetyAsync(imageBytes, file.ContentType);

                    if (!isSafe)
                    {
                        throw new Exception("Ảnh của bạn vi phạm tiêu chuẩn cộng đồng (Nội dung nhạy cảm/18+).");
                    }

                    // Nếu an toàn thì mới lưu xuống DB/Disk
                    // SaveToDisk(imageBytes)...
                }
            }
        }
    }
}
