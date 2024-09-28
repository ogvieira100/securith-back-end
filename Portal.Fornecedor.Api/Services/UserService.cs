using Portal.Fornecedor.Api.Models;
using Portal.Fornecedor.Api.Models.User.Dto;
using Portal.Fornecedor.Api.Models.User.Request;
using Portal.Fornecedor.Api.Models.User.Response;
using Portal.Fornecedor.Api.Util;

namespace Portal.Fornecedor.Api.Services
{
    public interface IUserService
    {
        Task<BaseResponseHttpApi<UserRegisterResponse>> UserRegisterRequestAsync(UserRegisterRequest userRegisterRequest);

        Task<BaseResponseHttpApi<UserLoginDto>> LoginAsync(UserLoginRequest userLoginRequest);

        Task<BaseResponseHttpApi<object>> DeleteUserAsync(Guid id);
    }
    public class UserService : BaseHttpService, IUserService
    {
        readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient, LNotifications notification) : base(notification)
        {
            _httpClient = httpClient;
        }

        public async Task<BaseResponseHttpApi<object>> DeleteUserAsync(Guid id)
        {
            var responseLogin = await _httpClient.DeleteAsync($"api/v1/user/deletar-conta/{id}");
            await TreatErrorsResponse<object>(responseLogin);

            return new BaseResponseHttpApi<object>();
        }

        public async Task<BaseResponseHttpApi<UserLoginDto>> LoginAsync(UserLoginRequest userLoginRequest)
        {
            var responseLogin = await _httpClient.GetAsync($"api/v1/user/login?{userLoginRequest.GetQueryString()}");
            await TreatErrorsResponse<UserLoginDto>(responseLogin);
            if (_notification.Any())
                return null;
            return (await DeserializeObjResponse<BaseResponseHttpApi<UserLoginDto>>(responseLogin));
        }



        public async Task<BaseResponseHttpApi<UserRegisterResponse>> UserRegisterRequestAsync(UserRegisterRequest userRegisterRequest)
        {
            var httpContent = GetContentJsonUTF8(userRegisterRequest);
            var responseLogin = await _httpClient.PostAsync($"api/v1/user/nova-conta", httpContent);
            await TreatErrorsResponse<UserRegisterDto>(responseLogin);
            if (_notification.Any())
                return null;
            return (await DeserializeObjResponse<BaseResponseHttpApi<UserRegisterResponse>>(responseLogin));
        }
    }
}
