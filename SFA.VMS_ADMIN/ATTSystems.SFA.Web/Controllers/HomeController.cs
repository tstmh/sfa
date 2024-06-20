using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ATTSystems.SFA.Web.Controllers
{
    [NoDirectAccess]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<ModuleViewModel> model = new List<ModuleViewModel>();
            string? moduleList = HttpContext.Session.GetString("MODULE");
            if (moduleList != null)
            {
                string[] tok = moduleList.Split(',');
                foreach (var item in tok)
                {
                    model.Add(new ModuleViewModel
                    {
                        ModuleId = item,
                        ModuleName = item,
                        Id = 0
                    });
                }

                if (model[0].ModuleId == "AdminModule")
                {
                    ViewBag.ModuleId = "Administrator";
                }
                if (model[0].ModuleId == "ManagerModule")
                {
                    ViewBag.ModuleId = "Manager";
                }
                if (model[0].ModuleId == "OperatorModule")
                {
                    ViewBag.ModuleId = "Operator";
                }
                ViewBag.Title = "SFA Home Page";
                string moduleName = ViewBag.ModuleId;
                HttpContext.Session.SetString("ModuleName", moduleName);
                ViewBag.CurrentUserName = HttpContext.Session.GetString("USERKEY");
                return View(model);
            }
            else
            {
                return RedirectToAction("Login", "Auth");
            }
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
