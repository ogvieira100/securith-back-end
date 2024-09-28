using Portal.Fornecedor.Api.Util;
using System.Text.Json;
using System.Text;
using Portal.Fornecedor.Api.Models;

namespace Portal.Fornecedor.Api.Services
{
    public abstract class BaseHttpService
    {
        protected readonly LNotifications _notification;

        protected BaseHttpService(LNotifications notification)
        {
            _notification = notification ?? new LNotifications();

        }

        protected StringContent GetContentJsonUTF8(object dado)
        {
            return new StringContent(
                JsonSerializer.Serialize(dado),
                Encoding.UTF8,
                "application/json");
        }
        protected async Task TreatErrorsResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseApi = await DeserializeObjResponse<BaseResponseHttpApi<T>>(response);
                _notification.AddRange(responseApi.Errors);
            }
        }


        protected async Task<T> DeserializeObjResponse<T>(HttpResponseMessage responseMessage)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var cont = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(cont, options);
        }

    }
}
