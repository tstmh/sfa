using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ATTSystems.SFA.Web
{
    public class NoDirectAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var referrer = context.HttpContext.Request.Headers["Referer"].ToString();
            var host = context.HttpContext.Request.Host.Host;

            if (string.IsNullOrEmpty(referrer) || !referrer.Contains(host))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
