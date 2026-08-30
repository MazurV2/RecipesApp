namespace RecipesApi.Services
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? ResultMessage { get; }

        public ServiceResult(bool isSuccess, T? data, string? resultMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ResultMessage = resultMessage;
        }

        public static ServiceResult<T> Success(T data) => new ServiceResult<T>(true, data, null);
        public static ServiceResult<T> Failure(string resultMessage) => new ServiceResult<T>(false, default, resultMessage);
    }
}
