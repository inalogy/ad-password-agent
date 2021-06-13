namespace MidPointUpdatingService.Models
{
    public class MidPointError
    {
        public MidPointErrorEnum ErrorCode { get; set; }
        public bool Recoverable { get; set; }
        public string ErrorMessage { get; set; }
    }
}
