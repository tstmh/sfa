using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL;
using ATTSystems.SFA.DAL.Implementation;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private IReport _report;
        private readonly ILogger<ReportController> _logger;
        public ReportController(IReport report, IBaseRepository baseRepository, ILogger<ReportController> logger)
        {
            _report = report;
            _logger = logger;
        }
        [HttpPost]
        [Route("GetVisitorAccessList")]
        public async Task<IActionResult> GetVisitorAccessList(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Getting Visitor access list");

                var resp = await _report.GetVisitorAccessListtAsync(req);
                if (resp != null)
                {
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Getting Visitor access list is error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetVisitorAccessList , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetBlockVisitorAccessList")]
        public async Task<IActionResult> GetBlockVisitorAccessList(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Get black visitor details");
                var resp = await _report.GetBlockVisitorAccessListtAsync(req);
                if (resp != null)
                {
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Black visitor details is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetBlockVisitorAccessList , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetAudittrailList")]
        public async Task<IActionResult> GetAudittrailList(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Get visitor audit trail list ");
                var resp = await _report.GetAudittrailAsync(req);
                if (resp != null)
                {
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Visitor audit trail is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetAudittrailList , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetSearchVisitorAccess")]
        public async Task<IActionResult> GetSearchVisitorAccess(APIRequest request)
        {
            AppResponse result = new AppResponse();

            try
            {
                _logger.LogInformation("Get search visitor access details");
                //ReportRepository repository = new ReportRepository();
                var searchvisitoraccess = await _report.GetSearchVisitoraccessAsync(request);

                if (searchvisitoraccess != null)
                {
                    result.VisitorAccessLists = searchvisitoraccess.VisitorAccessLists;
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Got the result";
                    _logger.LogInformation("Got the result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search visitor access is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetSearchVisitorAccess, Message: {0} ", ex.Message));

            }

            return Ok(result);
        }
        [HttpPost]
        [Route("GetSearchblkVisitor")]
        public async Task<IActionResult> GetSearchblkVisitor(APIRequest request)
        {
            AppResponse result = new AppResponse();

            try
            {
                _logger.LogInformation("Getting search black visitor details");
                //ReportRepository repository = new ReportRepository();
                var searchvisitoraccess = await _report.GetSearchblkVisitorAsync(request);

                if (searchvisitoraccess != null)
                {
                    result.VisitorAccessLists = searchvisitoraccess.VisitorAccessLists;
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Got the result";
                    _logger.LogInformation("Got the result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search black visitor is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetSearchblkVisitor, Message: {0} ", ex.Message));

            }

            return Ok(result);
        }
        [HttpPost]
        [Route("GetSearchAudittrail")]
        public async Task<IActionResult> GetSearchAudittrail(APIRequest request)
        {
            AppResponse result = new AppResponse();

            try
            {
                _logger.LogInformation("Getting search audit trail details");
                //ReportRepository repository = new ReportRepository();
                var searchvisitoraccess = await _report.GetSearchAudittrailAsync(request);

                if (searchvisitoraccess != null)
                {
                    result.VisitorAccessLists = searchvisitoraccess.VisitorAccessLists;
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Got the result";
                    _logger.LogInformation("Got the result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search audit trail is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetSearchAudittrail, Message: {0} ", ex.Message));

            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetLoadVisitorType")]
        public async Task<IActionResult> GetLoadVisitorType(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Load the visitor types");
                List<VisitType> res = await _report.GetVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.VsttypList = new List<VsttypList>();
                    foreach (var item in res)
                    {
                        result.VsttypList.Add(new VsttypList
                        {
                            lVsttypeId = Convert.ToInt32(item.Id),
                            lVstTypeNmae = item.VisitTypeName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("Load the visitor types successfully");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "load visitor types is getting error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetLocationList")]
        public async Task<IActionResult> GetLocationList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Getting location");
                //AdminPortalRepository repository = new AdminPortalRepository();
                List<Location> res = await _report.GetLocationAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.VisitorAccessLists = new List<VisitorAccessList>();
                    foreach (var item in res)
                    {
                        result.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            LocationIdlst = Convert.ToInt32(item.Id),
                            LocationNamelst = item.LocationName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("Getting location is successfully");
                }
            }

            catch (Exception ex)

            {
                _logger.LogError(ex, "Load location is getting error.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }
        #region graph

        [HttpPost]
        [Route("GetSearchlinechart")]
        public async Task<IActionResult> GetSearchlinechart(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Getting search line chart");
            try
            {
                ReportViewModel _transarrival = await _report.GetSearchlinechartAsync(request);
                if (_transarrival != null)
                {
                    result.VisitorAccessLists = _transarrival.VisitorAccessLists;
                    result.transLists1 = _transarrival.transLists1;
                   
                }
                result.Code = 1;
                result.Succeeded = true;
                result.Message = "Visitor access Details";
                _logger.LogInformation("Getting search line chart data successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Search line chart is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level:Warning, Type:API, Controller:Report, Action:GetSearchlinechart, Message:{0}", ex.Message));
            }
            // response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            return Ok(result);
        }
        [HttpPost]
        [Route("GetDefaultlinechart")]
        public async Task<IActionResult> GetDefaultlinechart(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Getting default line chart");
            try
            {
                ReportViewModel _transarrival = await _report.GetDefaultlinechartAsync(request);
                if (_transarrival != null)
                {
                    result.VisitorAccessLists = _transarrival.VisitorAccessLists;
                    result.transLists1 = _transarrival.transLists1;
                    
                }
                result.Code = 1;
                result.Succeeded = true;
                result.Message = "Visitor access Details";
                _logger.LogInformation("Getting default line chart data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Default line chart is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level:Warning, Type:API, Controller:Report, Action:GetDefaultlinechart, Message:{0}", ex.Message));
            }
            // response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            return Ok(result);
        }

        #endregion
        #region User Audit Trial
        [HttpPost]
        [Route("GetUserList")]
        public async Task<IActionResult> GetUserList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Getting User");
                List<User> res = await _report.GetUserAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.VisitorAccessLists = new List<VisitorAccessList>();
                    foreach (var item in res)
                    {
                        result.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            UserIdlst = Convert.ToInt32(item.Id),
                            UserNamelst = item.Email
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("Getting User is successfully");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Load user is getting error.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetVisitorUserAccessList")]
        public async Task<IActionResult> GetVisitorUserAccessList(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Get user audit trail list ");
                var resp = await _report.GetUserVisitorListtAsync(req);
                if (resp != null)
                {
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("User audit trail is getting error");
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetVisitorUserAccessList , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);

        }

        [HttpPost]
        [Route("GetSearchVisitorUserAccess")]
        public async Task<IActionResult> GetSearchVisitorUserAccess(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                _logger.LogInformation("Getting search user audit trail details");
                var searchvisitoraccess = await _report.GetSearchVisitorUseraccessAsync(request);

                if (searchvisitoraccess != null)
                {
                    result.VisitorAccessLists = searchvisitoraccess.VisitorAccessLists;
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Got the result";
                    _logger.LogInformation("Got the result");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search user audit trail is getting error");
               // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Report, Action:GetSearchVisitorUserAccess, Message: {0} ", ex.Message));

            }
            return Ok(result);
        }
        #endregion

        #region Login Audit Trial


        [HttpPost]
        [Route("GetLoginUser")]
        public async Task<IActionResult> GetLoginUser(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                List<User> res = await _report.LoginUserAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.VisitorAccessLists = new List<VisitorAccessList>();
                    foreach (var item in res)
                    {
                        result.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            UserIdlst = Convert.ToInt32(item.Id),
                            UserNamelst = item.Email
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            return Ok(result);
        }


        [HttpPost]

        [Route("GetLoginusertracking")]
        public async Task<IActionResult> GetLoginusertracking(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _report.LoginUserTrackingAsync(request);
                if (resp != null)
                {
                    result._usersessiontrackinglist = resp._usersessiontrackinglist;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
                result.Message = ex.Message;
            }
            return Ok(result);
        }



        [HttpPost]
        [Route("GetSearchLoginUserTracking")]
        public async Task<IActionResult> GetSearchLoginUserTracking(APIRequest request)
        {
            AppResponse result = new AppResponse();

            try
            {
                var SearchUser = await _report.SearchLoginUserTrackingAsync(request);

                if (SearchUser != null)
                {
                    result._usersessiontrackinglist = SearchUser._usersessiontrackinglist;
                    result.Code = 1;
                    result.Succeeded = true;
                    result.Message = "Got the result";
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }

            return Ok(result);
        }




        #endregion

    }
}
