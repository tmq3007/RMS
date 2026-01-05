using Microsoft.ML.Data;

namespace RMS.Services.AI.NSFW.Models
{
    public class NsfwOutput
    {
        [ColumnName("logits")]
        public float[] Prediction { get; set; }
    }
}