using Portal.Fornecedor.Api.Util;

namespace Portal.Fornecedor.Api.Models
{
    public class BaseResponseHttpApi<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }

        public List<LNotification> Errors { get; set; }

        public BaseResponseHttpApi()
        {
            Errors = new List<LNotification>();
        }


    }
}
