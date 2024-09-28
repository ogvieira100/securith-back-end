using Microsoft.Extensions.Configuration;
using Portal.Fornecedor.Api.Models;
using Portal.Fornecedor.Api.Models.User.Dto;
using Portal.Fornecedor.Api.Models.User.Response;
using Portal.Fornecedor.Api.Util;

namespace Portal.Fornecedor.Api.Services
{
    public interface IServiceGoogleCaptcha {
        Task<BaseResponseHttpApi<CaptchaResponse>> IsValidCaptchaAsync(string captchaToken);

    }
    public class ServiceGoogleCaptcha : BaseHttpService, IServiceGoogleCaptcha
    {
        readonly HttpClient _httpClient;
        readonly IConfiguration _configuration; 
        public ServiceGoogleCaptcha(LNotifications notification, IConfiguration configuration,  HttpClient httpClient) 
            : base(notification)
        {
            _httpClient = httpClient;
            _configuration = configuration; 

        }

        public async Task<BaseResponseHttpApi<CaptchaResponse>> IsValidCaptchaAsync(string captchaToken)
        {
            var secretKey =  _configuration.GetSection("Google:CaptchaSecretKey").Value;

            var ret = new BaseResponseHttpApi<CaptchaResponse>();



            var content = new FormUrlEncodedContent(new[]
               {
                    new KeyValuePair<string, string>("secret", secretKey),
                    new KeyValuePair<string, string>("response", captchaToken),
                   // new KeyValuePair<string, string>("remoteip", "localhost")
                });
            var result = await _httpClient.PostAsync("", content);
            if (_notification.Any())
                return null;
            var serial = await result.Content.ReadAsStringAsync();

            var resp = (await DeserializeObjResponse<CaptchaResponse>(result));

            ret.Data = resp;    

            return ret;
        }
    }
}
