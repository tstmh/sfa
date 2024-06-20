namespace ATTSystems.SFA.Web.Helper
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;

    public class StateManager : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            byte[]? stateValue;
            bool result = filterContext.HttpContext.Session.TryGetValue("USERKEY", out stateValue);
            if (result)
            {
                string state = System.Text.Encoding.Default.GetString(stateValue ?? Array.Empty<byte>());
                result = string.IsNullOrEmpty(state) ? false : true;
            }

            if (!result)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "action", "Logoff" },
                    { "controller", "Auth" }
                });
            }
        }

    }
}
