using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL;
using ATTSystems.SFA.DAL.Implementation;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Numerics;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/Kiosk")]
    [ApiController]
    public class KioskController : ControllerBase
    {
        private IKiosk _kiosk;
        private ILogger<KioskController> logger;

       
        public KioskController(IKiosk kiosk, ILogger<KioskController> logger)
        {
            _kiosk = kiosk;
            this.logger = logger;

        }
     
        [HttpPost]
        [Route("GetExistingPassportNo")]
        public async Task<IActionResult> GetExistingPassportNo(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
               logger.LogInformation("GetExistingPassportNo invoke started");

                var res = new KioskViewModel();

                var dbResult = await _kiosk.GetExistingPassportNoAsync(request);
              
                if (dbResult.NId == 1)
                {
                    result.listKiosk = new List<KioskViewList>();
                    foreach (var item in dbResult.KioskList)
                    {
                        result.listKiosk.Add(new KioskViewList
                        {
                            listId = Convert.ToInt32(item.listId),
                            listvisitorName = item.listvisitorName,
                            listcompanyName = item.listcompanyName,
                            listmobileNumber = item.listmobileNumber,
                            listvisitorType = item.listvisitorType,
                            listvehicleNumber = item.listvehicleNumber,
                            listPassportNumber = item.listPassportNumber,
                            listEmail = item.listEmail,
                        });
                    }
                    res.KioskList = dbResult.KioskList;
                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message = "This NRIC/ Passport has been already Registered";
                }
                else if (dbResult.NId == 2)
                {
                    result.listKiosk = new List<KioskViewList>();
                    foreach (var item in dbResult.KioskList)
                    {
                        result.listKiosk.Add(new KioskViewList
                        {
                            listId = Convert.ToInt32(item.listId),
                            listvisitorName = item.listvisitorName,
                            listcompanyName = item.listcompanyName,
                            listmobileNumber = item.listmobileNumber,
                            listvisitorType = item.listvisitorType,
                            listvehicleNumber = item.listvehicleNumber,
                            listPassportNumber = item.listPassportNumber,
                            listEmail = item.listEmail,
                        });
                    }
                    res.KioskList = dbResult.KioskList;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message = "This NRIC/ Passport has been already Registered. Please proceed to the counter";
                    result.ID = dbResult.alertId;
                }
                else if (dbResult.NId == 0)
                {
                    string? passportNumber = dbResult.NRICPassport;
                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    //result.Message = "This Passport is not registered";
                    result.Message = passportNumber;
                }
                else if (dbResult.NId == 4)
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message = "This Visitor is Blacklisted. Please proceed to counter";
                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
                    result.Message = "Details Failed";
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
        [Route("GetVisitorTypeList")]
        public async Task<IActionResult> GetVisitorTypeList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("GetVisitorTypeList invoke started");
                List<VisitType> res = await _kiosk.GetVisitorTypeAsync(request);

                if (res != null && res.Count > 0)
                {
                    result.listKiosk = new List<KioskViewList>();
                    foreach (var item in res)
                    {
                        result.listKiosk.Add(new KioskViewList
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

        [HttpPost]
        [Route("ValidateUnitID")]
        public async Task<IActionResult> ValidateUnitID(APIRequest req)
        {
            AppResponse result = new AppResponse();
            try
            {
                logger.LogInformation("ValidateUnitID invoke started");
                int resp = await _kiosk.ValidateUnitIDAsync(req);
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
                result.Message = ex.Message;
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
                var resp = await _kiosk.GetUnitIdListAsync(req);
                if (resp != null)
                {
                    result.UnitsDetailLists = resp.UnitsDetailLists;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetUnitIDList method");
                LoggerHelper.Instance.LogError(ex);
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        //INSERT
        [HttpPost]
        [Route("AddKioskVisitorDetails")]
        public async Task<IActionResult> AddKioskVisitorDetails(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("AddNewVisitorPassport invoke started");

                var dbResult = await _kiosk.SaveKioskVisitorDetailsAsync(request);

                if (dbResult.NId > 0)
                {
                    string? passportNumber = dbResult.NRICPassport;
                    int idType = dbResult.IdType;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message =Convert.ToInt32(idType).ToString();     
                   
                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
                    result.Message = "Details Failed";
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
        [Route("UpdateKioskVisitorDetails")]
        public async Task<IActionResult> UpdateKioskVisitorDetails(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdateKioskVisitorDetails invoke started");

                var dbResult = await _kiosk.UpdateVisitorRegLocationMapping(request);

                if (dbResult.NId > 0)
                {
                    string? passportNumber = dbResult.NRICPassport;
                    int idType = dbResult.IdType;

                    result.Code = dbResult.NId;
                    result.Succeeded = true;
                    result.Message = Convert.ToInt32(idType).ToString();

                }
                else
                {
                    result.Code = dbResult.NId;
                    result.Succeeded = false;
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
        [Route("UpdateCardNumber")]
        public async Task<IActionResult> UpdateCardNumber(APIRequest request)
        {
            APIResponse result = new APIResponse();
            try
            {
                logger.LogInformation("UpdateCardNumber invoke started");

                var cardNumber = await _kiosk.UpdateCardNumber(request);

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
                logger.LogError(ex, "Error in UpdateKioskVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(result);
        }

    }
}
