using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAPIController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("SFA VMS - ATT Systems API Ready");
        }
    }
}
