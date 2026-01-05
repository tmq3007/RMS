using Microsoft.AspNetCore.Mvc;
using RMS.Services.AI;
using RMS.Services.AI.NSFW;

namespace RMS.Controllers.AI
{
    [Route("api/[controller]")]
    [ApiController]
    public class NsfwController : ControllerBase
    {
        private readonly NsfwOfflineService _nsfwService;

        public NsfwController(NsfwOfflineService nsfwService)
        {
            _nsfwService = nsfwService;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng upload ảnh.");

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                var result = _nsfwService.AnalyzeImage(imageBytes);

                if (result.IsUnsafe)
                {
                    return StatusCode(406, new
                    {
                        status = "BLOCKED",
                        score = $"{result.Score * 100:F2}%",
                        message = "Ảnh chứa nội dung nhạy cảm."
                    });
                }

                return Ok(new
                {
                    status = "SAFE",
                    score = $"{result.Score * 100:F2}%",
                    message = "Ảnh an toàn."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}