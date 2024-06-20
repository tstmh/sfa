using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.DAL.Implementation;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/MobileDevice")]
    [ApiController]

    public class MobileDeviceController : ControllerBase
    {
        private IMobileDevice _Mobile;
        //  private ILogger _Logger;
        private IConfiguration config;
        ILogger<MobileDeviceController> logger;

        public MobileDeviceController(IMobileDevice mobile, IConfiguration config, ILogger<MobileDeviceController> logger)
        {
            _Mobile = mobile;

            this.config = config;
            this.logger = logger;
        }


        [HttpPost]
        [Route("Registernric")]
        public async Task<IActionResult> Registernric(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _Mobile.AsyncRegisternric(request);
                if (res != null)
                {
                    if (res.NId == 1)
                    {
                        result.ListMobileDevice = res.ListMobileDevice;
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered";
                    }
                    else if (res.NId == 2)
                    {
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with the same Location. Please proceed to counter";
                    }
                    else if (res.NId == 0)
                    {
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN is not registered with any of the location";
                    }
                    else if (res.NId == 3)
                    {
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with both the Locations";
                    }
                    else if (res.NId == 4)
                    {
                        result.ListMobileDevice = res.ListMobileDevice;
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with both the Locations";
                    }
                    else if (res.NId == 5)
                    {
                        result.ListMobileDevice = res.ListMobileDevice;
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered with the same Location";

                    }
                    else if (res.NId == 6)
                    {
                        result.Code = res.NId;
                        result.Succeeded = true;
                        result.Message = "This Visitor is Blacklisted. Please proceed to counter";

                    }
                    else
                    {
                        result.Code = res.NId;
                        result.Succeeded = false;
                    }
                }
            }

            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in Registernric method");
                logger.LogError(ex.ToString());
                result.Message = ex.Message;
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("SavenricRegisterDtls")]
        public async Task<ActionResult> SavenricRegisterDtls(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var nricid = await _Mobile.AsyncSavenricRegisterdtls(request);
                if (nricid == 1)
                {
                    result.Code = 1;
                    result.Message = "NRIC/FIN is registered";
                    result.Succeeded = true;
                }

            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in SavenricRegisterDtls method");
                logger.LogError(ex.ToString());
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("GetExitsSaveNric")]
        public async Task<ActionResult> GetExitsSaveNric(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var resP = await _Mobile.AsyncExitsSaveNric(request);
                if (resP != null)
                {
                    if (resP.NId == 1)
                    {
                        result.ListMobileDevice = resP.ListMobileDevice;
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This NRIC/FIN has been already Registered";
                    }

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetExitsSaveNric method");
                logger.LogError(ex.ToString());
            }

            return Ok(result);
        }



     

        [HttpPost]
        [Route("GetRegisterPassport")]
        public async Task<IActionResult> GetRegisterPassport(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var resP = await _Mobile.AsyncRegisterPassport(request);
                if (resP != null)
                {
                    if (resP.NId == 1)
                    {
                        result.ListMobileDevice = resP.ListMobileDevice;
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered";
                    }
                    else if (resP.NId == 2)
                    {
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with the same Location. Please proceed to counter";
                    }
                    else if (resP.NId == 0)
                    {
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport is not registered with any of the location";
                    }
                    else if (resP.NId == 3)
                    {
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with both the Locations";
                    }
                    else if (resP.NId == 4)
                    {
                        result.ListMobileDevice = resP.ListMobileDevice;
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with both the Locations";
                    }
                    else if (resP.NId == 5)
                    {
                        result.ListMobileDevice = resP.ListMobileDevice;
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered with the same Location";

                    }
                    else if (resP.NId == 6)
                    {
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Exits as a NIRC/FIN Number";

                    }
                    else if (resP.NId == 7)
                    {
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Visitor is Blacklisted. Please proceed to counter";

                    }

                    else
                    {
                        result.Code = resP.NId;
                        result.Succeeded = false;

                    }
                }
            }

            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetRegisterPassport method");
                logger.LogError(ex.ToString());
                result.Message = ex.Message;
            }

            return Ok(result);
        }


        [HttpPost]
        [Route("Savepassportdtls")]
        public async Task<ActionResult> Savepassportdtls(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var nricid = await _Mobile.AsyncSavePassportRegisterdtls(request);
                if (nricid == 1)
                {
                    result.Code = 1;
                    result.Message = "Passport is registered";
                    result.Succeeded = true;
                }

            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in Savepassportdtls method");
                logger.LogError(ex.ToString());
            }
            return Ok(result);
        }


        [HttpPost]
        [Route("GetExitsSavepassport")]
        public async Task<ActionResult> GetExitsSavepassport(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var resP = await _Mobile.AsyncExitsSavePassport(request);
                if (resP != null)
                {
                    if (resP.NId == 1)
                    {
                        result.ListMobileDevice = resP.ListMobileDevice;
                        result.Code = resP.NId;
                        result.Succeeded = true;
                        result.Message = "This Passport has been already Registered";
                    }

                }

            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetExitsSaveNric method");
                logger.LogError(ex.ToString());
            }

            return Ok(result);
        }






        //  Load NRIC


        [HttpPost]
        [Route("GetNricVisitorTypeList")]
        public async Task<IActionResult> GetNricVisitorTypeList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                List<VisitType> res = await _Mobile.GetNricVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.ListMobileDevice = new List<MobileDeviceViewList>();
                    foreach (var item in res)
                    {
                        result.ListMobileDevice.Add(new MobileDeviceViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listvisitorType = item.VisitTypeName,
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetNricVisitorTypeList method");
                logger.LogError(ex.ToString());

            }
            return Ok(result);
        }


        [HttpPost]
        [Route("GetVisitorTypePassport")]
        public async Task<IActionResult> GetVisitorTypePassport(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                List<VisitType> res = await _Mobile.GetNricVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.ListMobileDevice = new List<MobileDeviceViewList>();
                    foreach (var item in res)
                    {
                        result.ListMobileDevice.Add(new MobileDeviceViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listvisitorType = item.VisitTypeName,
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                }
            }

            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetVisitorTypePassport method");
                logger.LogError(ex.ToString());

            }
            return Ok(result);
        }



        [HttpPost]
        [Route("GetLocationUnitIDS")]
        public async Task<IActionResult> GetLocationUnitIDS(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _Mobile.GetLocationUnitIDSAsync(req);
                if (resp != null)
                {
                    result.mobileunitsDetailLists = resp.mobileunitsDetailLists;
                    result.locationid = resp.locationid;
                }
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in GetLocationUnitIDS method");
                logger.LogError(ex.ToString());
                result.Message = ex.Message;
            }
            return Ok(result);
        }


        [HttpPost]
        [Route("GetMobileValidateUnitID")]
        public async Task<IActionResult> GetMobileValidateUnitID(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                int resp = await _Mobile.MobileValidateUnitIDAsync(req);
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
                //logger.LogError(ex, "Error in ValidateUnitID method");
                logger.LogError(ex.ToString());
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("MobileUpdateCardNumber")]
        public async Task<IActionResult> MobileUpdateCardNumber(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdateCardNumber invoke started");

                var cardNumber = await _Mobile.MobileUpdateCardNumber(request);

                if (cardNumber != "")
                {
                    result.Succeeded = true;
                    result.Message = cardNumber;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = "Details Failed";
                }

            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error in UpdateKioskVisitorDetails method");
                //LoggerHelper.Instance.LogError(ex);
                logger.LogError(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }





    }
}



