namespace MidPointUpdatingService.Models
{
    public class MidPointError
    {
        public int ErrorCode { get; set; }
        public bool Recoverable { get; set; }
        public string ErrorMessage { get; set; }
    }
}
