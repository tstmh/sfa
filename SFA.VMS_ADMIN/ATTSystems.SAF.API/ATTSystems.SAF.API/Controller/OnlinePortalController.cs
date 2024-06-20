using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Web.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnlinePortalController : ControllerBase
    {
        private IOnlinePortal _online;
        private ILogger<OnlinePortalController> logger;
        public OnlinePortalController(IOnlinePortal online, ILogger<OnlinePortalController> logger)
        {
            _online = online;
            this.logger = logger;
        }

        #region Nikitha
        [HttpPost]
        [Route("GetExistingPassportNo")]
        public async Task<IActionResult> GetExistingPassportNo(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetExistingPassportNo invoke started");

                var dbResult = await _online.GetExistingPassportNoAsync(request);

                if (dbResult != null)
                {
                    if (dbResult.NId == 1)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered";
                    }
                    else if (dbResult.NId == 2)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with the same Location. Please proceed to counter";
                    }
                    else if (dbResult.NId == 0)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport is not registered with any of the location";
                    }
                    else if (dbResult.NId == 3)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with both the Locations";
                    }
                    else if (dbResult.NId == 4)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with both the Locations";
                    }
                    else if (dbResult.NId == 5)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with the same Location";

                    }
                    else if (dbResult.NId == 6)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Visitor is Blacklisted. Please proceed to counter";
                    }
                    else if (dbResult.NId == 7)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already registered as a NRIC/FIN";
                    }
                    else
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = false;
                        //result.Message = _Mobile.GetErrorMsg();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetExistingPassportNo method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("UpdatePassportVisitorDetails")]
        public async Task<IActionResult> UpdatePassportVisitorDetails(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdatePassportVisitorDetails invoke started");

                var dbResult = await _online.UpdatePassVisitorReRegistration(request);

                if (dbResult.NId > 0)
                {
                    string? passportNumber = dbResult.passportNumber;
                    // int idType = dbResult.IdType;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    // result.Message = Convert.ToInt32(idType).ToString();

                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
                    //result.Message = _kiosk.GetErrorMsg();
                    result.Message = "Details Failed";
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdatePassportVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
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
                logger.LogInformation("GetLocationList invoke started");

                //OnlinePortalRepository repository = new OnlinePortalRepository();
                List<Location> res = await _online.GetLocationListAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.listOnlinePort = new List<OnlinePortalViewList>();
                    foreach (var item in res)
                    {
                        result.listOnlinePort.Add(new OnlinePortalViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listLocation = item.LocationName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetLocationList method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("GetVisitorTypeList")]
        public async Task<IActionResult> GetVisitorTypeList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetVisitorTypeList invoke started");

                //OnlinePortalRepository repository = new OnlinePortalRepository();
                List<VisitType> res = await _online.GetVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.listOnlinePort = new List<OnlinePortalViewList>();
                    foreach (var item in res)
                    {
                        result.listOnlinePort.Add(new OnlinePortalViewList
                        {
                            listId = item.Id,
                            listvisitorType = item.VisitTypeName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetVisitorTypeList method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        //Get Unit Id list
        [HttpPost]
        [Route("GetUnitIDList")]
        public async Task<IActionResult> GetUnitIDList(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetUnitIDList invoke started");

                var resp = await _online.GetUnitIdListAsync(req);
                if (resp != null)
                {
                    result.OnlineUnitsDetailLists = resp.OnlineUnitsDetailLists;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetUnitIDList method");
                LoggerHelper.Instance.LogError(ex);
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Kiosk, Action:GetUnitIDList , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("ValidateUnitID")]
        public async Task<IActionResult> ValidateUnitID(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("ValidateUnitID invoke started");


                int resp = await _online.ValidateUnitIDAsync(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "Unit ID is not Valid";
                    result.Succeeded = false;
                }
                else
                {
                    result.Message = "Unit ID is Valid";
                    result.Code = 1;
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ValidateUnitID method");
                LoggerHelper.Instance.LogError(ex);
                //LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: Kiosk, Action:ValidateUnitID , Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            return Ok(result);
        }


        //INSERT
        [HttpPost]
        [Route("AddNewVisitorPassport")]
        public async Task<IActionResult> AddNewVisitorPassport(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("AddNewVisitorPassport invoke started");

                var res = new OnlinePortalViewModel();

                //OnlinePortalRepository onlineRepos = new OnlinePortalRepository();
                var dbResult = await _online.SaveVisitorPassportDetailsAsync(request);

                if (dbResult.NId > 0)
                {
                    string? passportNumber = dbResult.passportNumber;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message = passportNumber;
                    //res.passportNumber = dbResult.passportNumber;
                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
                    //result.Message = onlineRepos.GetErrorMsg();
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddNewVisitorPassport method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateCardNumber")]
        public async Task<IActionResult> UpdateCardNumber(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdateCardNumber invoke started");

                var cardNumber = await _online.UpdateCardNumber(request);

                if (cardNumber != "")
                {
                    result.Succeeded = true;
                    result.Message = cardNumber;

                }
                else
                {
                    // result.Code = dbResult.NId;
                    result.Succeeded = false;
                    //result.Message = _kiosk.GetErrorMsg();
                    result.Message = "Details Failed";
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdateKioskVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }


        [HttpPost]
        [Route("Nricverifyno")]
        public async Task<IActionResult> Nricverifyno(APIRequest req)
        {
            AppResponse result = new AppResponse();

            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                logger.LogInformation("Nricverifyno invoke started");

                var dbResult = await _online.GetNricverifynoAsync(req);
                if (dbResult != null)
                {
                    if (dbResult.NId == 1)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered";
                    }
                    else if (dbResult.NId == 2)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with the same Location. Please proceed to counter";
                    }
                    else if (dbResult.NId == 0)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN is not registered with any of the location";
                    }
                    else if (dbResult.NId == 3)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with both the Locations";
                    }
                    else if (dbResult.NId == 4)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with both the Locations";
                    }
                    else if (dbResult.NId == 5)
                    {
                        result.listOnlinePort = dbResult.onlinePortalList;
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with the same Location";

                    }
                    else if (dbResult.NId == 6)
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = true;
                        result.Message = "This Visitor is Blacklisted. Please proceed to counter";
                    }
                    else
                    {
                        result.Code = dbResult.NId;
                        result.Succeeded = false;
                        //result.Message = _Mobile.GetErrorMsg();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Nricverifyno method");
                LoggerHelper.Instance.LogError(ex);
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: OnlinePortal, Action: Nricverifyno, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            //  response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            // response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            return Ok(result);
        }


        [HttpPost]
        [Route("GetNricVisitorTypeList")]
        public async Task<IActionResult> GetNricVisitorTypeList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetNricVisitorTypeList invoke started");

                //OnlinePortalRepository repository = new OnlinePortalRepository();
                List<VisitType> res = await _online.GetNricVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.listOnlinePort = new List<OnlinePortalViewList>();
                    foreach (var item in res)
                    {
                        result.listOnlinePort.Add(new OnlinePortalViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listvisitorType = item.VisitTypeName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetNricVisitorTypeList method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("GetNricLocationList")]
        public async Task<IActionResult> GetNricLocationList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetNricLocationList invoke started");

                // OnlinePortalRepository repository = new OnlinePortalRepository();
                List<Location> res = await _online.GetNricLocationAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.listOnlinePort = new List<OnlinePortalViewList>();
                    foreach (var item in res)
                    {
                        result.listOnlinePort.Add(new OnlinePortalViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listLocation = item.LocationName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetNricLocationList method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("Nricdtlsaveinfo")]
        public async Task<IActionResult> Nricdtlsaveinfo(APIRequest req)
        {
            APIResponse result = new APIResponse();

            // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            try
            {
                logger.LogInformation("Nricdtlsaveinfo invoke started");

                // OnlinePortalRepository _repository = new OnlinePortalRepository();

                var nricid = await _online.NricdtlsaveinfoAsync(req);
                if (nricid == -1)
                {


                    result.Code = -1;
                    result.Message = "NRIC is not registered";
                    result.Succeeded = true;
                }
                else
                {
                    result.Message = "NRIC is registered";
                    result.Code = 1;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Nricdtlsaveinfo method");
                LoggerHelper.Instance.LogError(ex);
                // LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: OnlinePortal, Action: Nricdtlsaveinfo, Message: {0} ", ex.Message));
                result.Message = ex.Message;
            }
            //  response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            // response.Content = new StringContent(JsonConvert.SerializeObject(result), System.Text.Encoding.UTF8, "application/json");
            return Ok(result);
        }


        [HttpPost]
        [Route("UpdateNRICVisitorDetails")]
        public async Task<IActionResult> UpdateNRICVisitorDetails(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdateNRICVisitorDetails invoke started");

                var dbResult = await _online.UpdateNRICVisitorReRegistration(request);

                if (dbResult.NId > 0)
                {
                    string? nricNumber = dbResult.NRICNumber;
                    // int idType = dbResult.IdType;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    // result.Message = Convert.ToInt32(idType).ToString();

                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
                    //result.Message = _kiosk.GetErrorMsg();
                    result.Message = "Details Failed";
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdateNRICVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }
        #endregion
    }
}
