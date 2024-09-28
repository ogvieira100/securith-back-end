using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Fornecedor.Api.Auth;
using Portal.Fornecedor.Api.Controllers;
using Portal.Fornecedor.Api.Models.User.Request;
using Portal.Fornecedor.Api.Services;
using Portal.Fornecedor.Api.Util;

namespace Portal.Fornecedor.Api.V1.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/user")]
    [Authorize]
    public class UserController : MainController
    {
        readonly JwtBuilder _jwtBuilder;
        readonly TempConvertClient _tempConvertClient;
        readonly IUserService _userService;
        readonly IServiceGoogleCaptcha _serviceGoogleCaptcha;
        public UserController(JwtBuilder jwtBuilder,
            TempConvertClient tempConvertClient,
            IUserService userService,
            IServiceGoogleCaptcha serviceGoogleCaptcha,
            LNotifications notifications) : base(notifications)
        {
            _tempConvertClient = tempConvertClient;
            _jwtBuilder = jwtBuilder;   
            _userService = userService;
            _serviceGoogleCaptcha = serviceGoogleCaptcha;   
        }

        [HttpGet("VerifyCaptcha")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCaptcha([FromQuery] string captchaToken)
         => await ExecControllerApiGatewayAsync(() => _serviceGoogleCaptcha.IsValidCaptchaAsync(captchaToken));

        [HttpPost("nova-conta")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRegister)
        {
            if (!ModelState.IsValid) return ReturnModelState(ModelState);
            return await ExecControllerApiGatewayAsync(() => _userService.UserRegisterRequestAsync(userRegister));

        }


        [HttpGet("Celcius/{celcius}")]
        public async Task<IActionResult> Celcius([FromRoute] string celcius)
        => await ExecControllerAsync(() => _tempConvertClient.CelsiusToFahrenheitAsync(celcius));

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] UserRequest userRequest)
        => await ExecControllerAsync(async () => {

            if (userRequest.Username == "teste" && userRequest.Password == "123")
            {
                return await Task.FromResult(  _jwtBuilder.BuildUserResponse() );
            }
            else
            {
                _notifications.Add(new LNotification { Message = "Usuário ou senha inválidos." });
                return null;
            }
        });

    }
}
