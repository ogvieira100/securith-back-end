using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Portal.Fornecedor.Api.Util;
using System.Text.Json;
using Portal.Fornecedor.Api.Models;

namespace Portal.Fornecedor.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        protected readonly LNotifications _notifications;

        public MainController(LNotifications notifications)
        {
            _notifications = notifications;

        }
        [NonAction]
        public bool IsValid()
        {
            return !_notifications.Any();
        }
        [NonAction]
        protected void ClearErrors()
        {
            _notifications.Clear();
        }

        [NonAction]
        protected async Task<IActionResult> ExecControllerApiGatewayAsync<T>
             (Func<Task<BaseResponseHttpApi<T>>> func)
        {
            try
            {
                return Response(await func());
            }
            catch (Exception ex)
            {

                AddError(ex);
                return Response(null);
            }
        }

        [NonAction]
        protected IActionResult ReturnModelState(ModelStateDictionary modelState)
        {

            NotifyModelStateErrors();

            return Response(null);

        }
        [NonAction]
        protected async Task<IActionResult> ExecControllerAsync<T>
                            (Func<Task<T>> func)
        {
            try
            {
                return Response(await func());
            }
            catch (Exception ex)
            {

                AddError(ex);
                return Response(null);
            }
        }

  

        [NonAction]
        protected async Task<IActionResult> ExecControllerAsync(Func<Task> func)
        {
            try
            {
                await func.Invoke();
                return Response(null);
            }
            catch (Exception ex)
            {

                AddError(ex);
                return Response(null);
            }
        }




        //
        [NonAction]
        protected IActionResult ExecController(object result = null)
        {
            try
            {
                return Response(result);
            }
            catch (Exception ex)
            {

                AddError(ex);
                return Response(null);
            }
        }


        [NonAction]
        protected new IActionResult Response(object result = null)
        {
            if (IsValid())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }

            if (_notifications.Any(x => x.TypeNotificationNoty == TypeNotificationNoty.BreakSystem))
            {
                return new ContentResult()
                {
                    StatusCode = 500,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(new
                    {
                        success = false,
                        data = result,
                        errors = _notifications
                    })
                };
            }
            return BadRequest(new
            {
                success = false,
                data = result,
                errors = _notifications
            });
        }


        [NonAction]
        protected void NotifyModelStateErrors()
        {
            var erros = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var erro in erros)
            {
                var erroMsg = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                AddError(new LNotification { Message = erroMsg });

            }
        }
        [NonAction]
        protected void AddError(IdentityResult result)
        {
            foreach (var error in result.Errors)
                AddError(new LNotification { Message = error.Description });
        }

        [NonAction]
        protected void AddError(Exception except)
        {
            _notifications.Add(new LNotification { TypeNotificationNoty = TypeNotificationNoty.BreakSystem, Message = except.Message });
        }

        [NonAction]
        protected void AddError(LNotification erro)
        {
            _notifications.Add(erro);
        }

    }
}
