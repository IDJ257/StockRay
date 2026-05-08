namespace StockRay.Other
{
    public class ServiceResult
    {

        public bool HasPassed { get; init; }

        public string? Message { get; init; }

        public ServiceResult(bool hasPassed)
        {
            HasPassed = hasPassed;
        }

        public ServiceResult(bool hasPassed, string message)
        {
            HasPassed = hasPassed;
            Message = message;
        }
    }








}
