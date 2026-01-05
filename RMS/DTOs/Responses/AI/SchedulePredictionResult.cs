namespace RMS.DTOs.Responses.AI
{
    public class SchedulePredictionResult
    {
        public string Status { get; set; } // "OnTrack", "Delayed", "Ahead"
        public int DelayedDays { get; set; }
        public string RiskAnalysis { get; set; }
        public string Recommendation { get; set; }
    }
}
