using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;

namespace RMS.Services.AI.NSFW.Models
{
    public class NsfwInput
    {
        // Thay Bitmap bằng MLImage
        [ImageType(224, 224)]
        public MLImage Image { get; set; }
    }
}