namespace StockRay.Other
{
    public class ServiceResult<T> : ServiceResult
    {

        public T? Value { get; init; }


        public ServiceResult(bool hasPassed, T value) : base(hasPassed)
        {
            Value = value;
        }


        public ServiceResult(bool hasPassed) : base(hasPassed)
        {

        }
    }

    public class ServiceResult
    {
        public bool HasPassed { get; init; }

        public string? Message { get; init; }


        public ServiceResult(bool hasPassed)
        {
            HasPassed = hasPassed;
        }


        public ServiceResult(bool hasPassed, string message) : this(hasPassed)
        {
            Message = message;
        }

       
    }








}
