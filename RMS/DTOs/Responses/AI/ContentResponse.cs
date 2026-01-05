namespace RMS.DTOs.Responses.AI
{
    public sealed class ContentResponse
    {
        public Candidate[] candidates { get; set; }
        public PromptFeedback promptFeedback { get; set; }
    }

    public sealed class PromptFeedback
    {
        public SafetyRating[] safetyRatings { get; set; }
    }

    public sealed class Candidate
    {
        public ResponseContent content { get; set; }
        public string finishReason { get; set; }
    }

    public sealed class ResponseContent
    {
        public ResponsePart[] parts { get; set; }
    }

    public sealed class ResponsePart
    {
        public string text { get; set; }
    }

    public sealed class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }
}
