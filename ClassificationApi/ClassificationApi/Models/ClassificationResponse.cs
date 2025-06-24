namespace ClassificationApi.Models
{
    public class ClassificationResponse
    {
        public string? PredictedType { get; set; }
        public double Confidence { get; set; }
        public string? Error { get; set; }
    }
} 