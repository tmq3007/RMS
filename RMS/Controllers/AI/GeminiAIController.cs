using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMS.DTOs.Requests.AI;
using RMS.Services.AI.Service;

namespace RMS.Controllers.AI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiAIController : ControllerBase
    {
        private readonly GeminiApiClient _geminiApiClient;
        private readonly SchedulePredictor _schedulePredictor;
        public GeminiAIController(SchedulePredictor schedulePredictor, GeminiApiClient geminiApiClient)
        {
            _geminiApiClient = geminiApiClient;
            _schedulePredictor = schedulePredictor;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateContent([FromBody] PromptRequest request)
        {
            try
            {
                string response = await _geminiApiClient.GenerateContentAsync(request.Prompt);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("analyze-schedule")]
        public async Task<IActionResult> AnalyzeSchedule([FromBody] object scheduleData)
        {
            var result = await _schedulePredictor.PredictAsync(scheduleData);
            return Ok(result);
        }



        /// <summary>
        /// API Upload ảnh để kiểm tra 18+ (NSFW)
        /// </summary>
        [HttpPost("check-image-safety")]
        public async Task<IActionResult> CheckImageSafety(IFormFile file)
        {
            // 1. Validate đầu vào
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file ảnh." });
            }

            try
            {
                // 2. Chuyển đổi file sang byte array
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // 3. Gọi Gemini để check
                // file.ContentType sẽ lấy loại ảnh (vd: image/png, image/jpeg)
                bool isSafe = await _geminiApiClient.CheckImageSafetyAsync(imageBytes, file.ContentType);

                // 4. Trả về kết quả
                if (isSafe)
                {
                    return Ok(new
                    {
                        status = "Safe",
                        message = "Ảnh an toàn, được phép sử dụng."
                    });
                }
                else
                {
                    // Trả về lỗi 400 hoặc 406 (Not Acceptable) tùy quy ước của bạn
                    return BadRequest(new
                    {
                        status = "Unsafe",
                        message = "Ảnh chứa nội dung nhạy cảm (18+) và bị từ chối."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống AI: " + ex.Message });
            }
        }
    }
}
