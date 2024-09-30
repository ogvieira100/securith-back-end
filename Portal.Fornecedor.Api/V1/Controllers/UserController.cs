using Asp.Versioning;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        readonly IHttpContextAccessor _accessor;
        readonly IDataProtectionProvider _dataProtectionProvider;
        public UserController(JwtBuilder jwtBuilder,
            TempConvertClient tempConvertClient,
            IUserService userService,
            IHttpContextAccessor accessor,
            IDataProtectionProvider dataProtectionProvider, 
            IServiceGoogleCaptcha serviceGoogleCaptcha,
            LNotifications notifications) : base(notifications)
        {
            _tempConvertClient = tempConvertClient;
            _jwtBuilder = jwtBuilder;   
            _userService = userService;
            _dataProtectionProvider = dataProtectionProvider;   
            _accessor  = accessor;  
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

        [HttpGet("GetXSRFToken")]
        public async Task<IActionResult> GetXSRFToken([FromServices] IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
            return Ok(new { token = tokens.RequestToken });
        }

        [HttpPost("CelciusPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CelciusPost([FromBody] CelciusPostRequest celciusPostRequest )
        => await ExecControllerAsync(() => _tempConvertClient.CelsiusToFahrenheitAsync(celciusPostRequest.Celcius));

        [HttpGet("Celcius/{celcius}")]
        public async Task<IActionResult> Celcius([FromRoute] string celcius)
        => await ExecControllerAsync(() => _tempConvertClient.CelsiusToFahrenheitAsync(celcius));

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromQuery] UserRequest userRequest)
        => await ExecControllerAsync(async () => {

            if (userRequest.Username == "teste" && userRequest.Password == "123")
            {
                var userResponse = _jwtBuilder.BuildUserResponse();

                // Criar um protetor para criptografar o token
                var protector = _dataProtectionProvider.CreateProtector("X-Access-Token");

                // Criptografar o token
                var encryptedToken = protector.Protect(userResponse.AccessToken);

                // Configuração do cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // O cookie não é acessível via JavaScript
                    Secure = true,   // O cookie é enviado apenas via HTTPS
                    SameSite = SameSiteMode.Strict, // Previne o envio do cookie em solicitações cross-site
                    Expires = DateTime.UtcNow.AddHours(1) // Define a expiração do cookie
                };
                /*se estiver num mesmo contexto adiciona */
                _accessor.HttpContext?.Response.Cookies.Append("X-Access-Token", encryptedToken, cookieOptions);

                userResponse.Secure = true;
                userResponse.HttpOnly = true;
                userResponse.SameSite = SameSiteMode.Strict;
                userResponse.AccessToken = encryptedToken;

                /*classe ou token para*/
                return userResponse;
            }
            else
            {
                _notifications.Add(new LNotification { Message = "Usuário ou senha inválidos." });
                return null;
            }
        });

    }
}
