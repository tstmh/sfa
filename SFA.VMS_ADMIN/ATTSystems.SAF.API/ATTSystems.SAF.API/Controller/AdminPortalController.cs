using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL.Implementations;
using System.Net;
using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.DAL.Implementation;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/AdminPortal")]
    [ApiController]
    public class AdminPortalController : ControllerBase
    {
        private string ErrorMsg = string.Empty;
        private IAdminPortal _adminportal;
        private IBaseRepository _ibaseReposit;
        private readonly ILogger<AdminPortalController> _logger;
        public AdminPortalController(IAdminPortal adminportal, IBaseRepository baseRepository, ILogger<AdminPortalController> logger)
        {
            _adminportal = adminportal;
            _ibaseReposit = baseRepository;
            _logger = logger;
        }

        #region Santosh Wali

        #region Dashboard

        [HttpPost]
        [Route("GetDashboardList")]
        public async Task<IActionResult> GetDashboardList(APIRequest req)
        {
            _logger.LogInformation("Getting Dashboard list");
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _adminportal.GetDashboardListAsync(req);
                if (resp != null)
                {
                    if (resp.EntryViewLists.Count > 0)
                    {
                        result.EntryViewLists = resp.EntryViewLists;
                        result.EntryCount = resp.EntryCount;
                        result.ExitCount = resp.ExitCount;
                        result.LiveCount = resp.LiveCount;
                    }
                    else
                    {
                        result.EntryViewLists = new List<RegistrationViewList>();
                        result.EntryCount = 0;
                        result.ExitCount = resp.ExitCount;
                        result.LiveCount = resp.LiveCount;
                    }
                    if (resp.StayoverViewLists.Count > 0)
                    {
                        result.StayoverViewLists = resp.StayoverViewLists;
                        result.StayoverCount = resp.StayoverCount;
                        result.ExitCount = resp.ExitCount;
                        result.LiveCount = resp.LiveCount;
                    }
                    else
                    {
                        result.StayoverViewLists = new List<RegistrationViewList>();
                        result.StayoverCount = 0;
                        result.ExitCount = resp.ExitCount;
                        result.LiveCount = resp.LiveCount;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard list is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        //Visitor Reactivation

        [HttpPost]
        [Route("ReactivateVisitor")]
        public async Task<IActionResult> ReactivateVisitor(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Reactivate visitor details");
            try
            {
                int resp = await _adminportal.ReactivateVisitorAsync(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "Reactivation is not done";
                    result.Succeeded = false;
                    _logger.LogInformation("Reactivation is not done");
                }
                else
                {
                    result.Message = "Reactivation is done";
                    result.Code = 1;
                    result.Succeeded = true;
                    _logger.LogInformation("Reactivation is done");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reactivate visitor is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        //oveerstay export excel

        [HttpPost]
        [Route("ExportOverstayerVisitor")]
        public async Task<IActionResult> ExportOverstayerVisitor(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Got Registration List");
            try
            {
                var panric = await _adminportal.GetoverstayerToExcelAsync(req);
                if (panric != null)
                {
                    result.registrationViewLists = panric.RegistrationViewLists;
                    _logger.LogInformation("Got Registration List");
                }
                else
                {
                    result.Message = "Failed to get registartion list";
                    result.Code = 0;
                    result.Succeeded = false;
                    _logger.LogInformation("Failed to get registartion list");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel Export is failed");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        #endregion

        #region Registration             

        [HttpPost]
        [Route("GetRegisterList")]
        public async Task<IActionResult> GetRegisterList(APIRequest req)
        {
            _logger.LogInformation("getting register list");
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _adminportal.GetRegistrationListAsync(req);
                if (resp != null)
                {
                    result.registrationViewLists = resp.RegistrationViewLists;
                    result.UnitsDetailLists = resp.UnitsDetailLists;                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register list is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

       

        [HttpPost]
        [Route("GetNricOrPassportNotExistenceAsync")]
        public async Task<IActionResult> GetNricOrPassportNotExistenceAsync(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Get NRIC/Passport not existency");
            try
            {
                var resp = await _adminportal.GetNricOrPassportNotExistenceAsync(req);
                if (resp.RegistrationViewLists.Count > 0)
                {
                    result.registrationViewLists = resp.RegistrationViewLists;
                    result.Code = -1;
                    result.Message = "NRIC/Passport is not registered";
                    result.Succeeded = true;
                    _logger.LogInformation("NRIC/Passport is not registered");
                }
                else
                {
                    result.Message = "NRIC/Passport is registered";
                    result.Code = 1;
                    _logger.LogInformation("NRIC/Passport is registered");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NRIC/Passport not existency is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("SaveBatchExcelFiles")]
        public async Task<IActionResult> SaveBatchExcelFiles(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Save batch excel files");
            try
            {
                int resp = await _adminportal.SaveBatchExcelFiles(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "NRIC/Passport is not registered";
                    result.Succeeded = false;
                    _logger.LogInformation("NRIC/Passport is not registered");
                }
                else
                {
                    result.Message = "NRIC/Passport is registered";
                    result.Code = 1;
                    result.Succeeded = true;
                    _logger.LogInformation("NRIC/passport is registered");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save batch files is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        #endregion

        #region ManualCheckIn

        [HttpPost]
        [Route("GetManualCheckInList")]
        public async Task<IActionResult> GetManualCheckInList(APIRequest req)
        {
            _logger.LogInformation("Getting manual checkin list");
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _adminportal.GetManualCheckInListAsync(req);
                if (resp != null)
                {
                    result.registrationViewLists = resp.RegistrationViewLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual checkin is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("ManualCheckInSave")]
        public async Task<IActionResult> ManualCheckInSave(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Manual check in save");
            try
            {
                int resp = await _adminportal.ManualCheckInSaveAsync(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "Manual CheckIn Not Done";
                    result.Succeeded = false;
                    _logger.LogInformation("Manual CheckIn Not Done");
                }
                else
                {
                    result.Message = "Manual CheckIn Done";
                    result.Code = 1;
                    result.Succeeded = true;
                    _logger.LogInformation("Manual CheckIn Done");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual checkin save is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        #endregion

        #region Blacklist

        [HttpPost]
        [Route("GetBlackList")]
        public async Task<IActionResult> GetBlackList(APIRequest req)
        {
            _logger.LogInformation("Getting black list");
            AppResponse result = new AppResponse();
            try
            {
                var resp = await _adminportal.GetGetBlackListAsync(req);
                if (resp != null)
                {
                    result.registrationViewLists = resp.RegistrationViewLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Black list is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("blacklist_Trigger")]
        public async Task<IActionResult> blacklist_Trigger(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Black list trigger");
            try
            {
                int resp = await _adminportal.blacklist_Trigger(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "blacklist is not registered";
                    result.Succeeded = false;
                    _logger.LogInformation("blacklist is not registered");
                }
                else
                {
                    result.Message = "blacklist is registered";
                    result.Code = 1;
                    result.Succeeded = true;
                    _logger.LogInformation("blacklist is registered");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Black list trigger is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        //blacklist excel export
        [HttpPost]
        [Route("ExportblacklistVisitor")]
        public async Task<IActionResult> ExportblacklistVisitor(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Got Black List");
            try
            {
                var panric = await _adminportal.GetExportBlackListAsync(req);
                if (panric != null)
                {
                    result.registrationViewLists = panric.RegistrationViewLists;
                    _logger.LogInformation("Got Black List");
                }
                else
                {
                    result.Message = "Failed to get Black list";
                    result.Code = 0;
                    result.Succeeded = false;
                    _logger.LogInformation("Failed to get Black list");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel Export is failed");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        #endregion

        #endregion

        #region Rakesh

        [HttpPost]
        [Route("GetsingleregLocationList")]
        public async Task<IActionResult> GetsingleregLocationList(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("load location");
            try
            {
                List<Location> res = await _adminportal.GetsingleregLocationAsync(request);
                if (res != null && res.Count > 0)
                {
                    result.registrationViewLists = new List<RegistrationViewList>();
                    foreach (var item in res)
                    {
                        result.registrationViewLists.Add(new RegistrationViewList
                        {
                            listLocationId = Convert.ToInt32(item.Id),
                            listLocationName = item.LocationName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("load location is successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "load location is getting error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("GetSingleLoadVisitorType")]
        public async Task<IActionResult> GetSingleLoadVisitorType(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Loading visitor types");
            try
            {
                List<VisitType> res = await _adminportal.GetSingleVisitorTypeAsync(request);
                if (res != null && res.Count > 0)
                {
                    result.registrationViewLists = new List<RegistrationViewList>();
                    foreach (var item in res)
                    {
                        result.registrationViewLists.Add(new RegistrationViewList
                        {
                            listVisitorTypeId = Convert.ToInt32(item.Id),
                            listVisitorTypeName = item.VisitTypeName
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("loaded visitor types successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading visitor types is getting error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("SingleLoadVstIdentityType")]
        public async Task<IActionResult> SingleLoadVstIdentityType(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("loaded visitor identity types");
            try
            {
                List<VisitorIdentity> res = await _adminportal.GetSinglevstidentityAsync(request);
                if (res != null && res.Count > 0)
                {
                    result.registrationViewLists = new List<RegistrationViewList>();
                    foreach (var item in res)
                    {
                        result.registrationViewLists.Add(new RegistrationViewList
                        {
                            listIdTypeId = Convert.ToInt32(item.Id),
                            listIdTypeName = item.Name
                        });
                    }
                    result.Succeeded = true;
                    result.Message = "Success";
                    _logger.LogInformation("load Visitor identity types success");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load visitor identity types is getting error");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("GetSingleregsave")]
        public async Task<IActionResult> GetSingleregsave(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Single registration save");
            try
            {
                var nricid = await _adminportal.SingleregsaveAsync(req);
                if (nricid == -1)
                {
                    result.Code = -1;
                    result.Message = "Single NRIC/Passport is Already registered";
                    result.Succeeded = true;
                    _logger.LogInformation("Single NRIC/Passport is Already registered");
                }
                else if (nricid == 1)
                {
                    result.Message = "Single NRIC/Passport is registered";
                    result.Code = 1;
                    _logger.LogInformation("Single NRIC/Passport is registered");
                }
                else if (nricid == 0)
                {
                    result.Message = "Single NRIC/Passport is not registered";
                    result.Code = 0;
                    _logger.LogInformation("Single NRIC/Passport is not registered");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Single registration save is getting error");
                result.Message = ex.Message;
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("GetEditRegModel")]
        public async Task<IActionResult> GetEditRegModel(APIRequest request)
        {
            _logger.LogInformation("Getting Edit registration modal");
            AppResponse result = new AppResponse();
            RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                var viewmodel = await _adminportal.EditVstRegAsync(request.RequestString);
                if (viewmodel != null)
                {
                    result.rmodel = viewmodel;
                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                    _logger.LogInformation("System able to retrieve Visitor Registration details");
                }
                else
                {
                    result.Message = _adminportal.GetErrorMsg();
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        result.Message = "System unable to retrieve Visitor Registration details";
                    }
                    _logger.LogInformation("System unable to retrieve Visitor Registration details");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "visitor registration edit modal is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("Deletevstreg")]
        public async Task<IActionResult> Deletevstreg(APIRequest request)
        {
            APIResponse result = new APIResponse();
            _logger.LogInformation("Delete registration");
            try
            {
                result.Code = await _adminportal.DeleteVstAsync(request);
                if (result.Code > 0)
                {
                    result.Succeeded = true;
                    result.Message = "Visitor sucessfully deleted.";
                    _logger.LogInformation("Visitor successfully deleted");
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = _adminportal.GetErrorMsg();
                    _logger.LogInformation("Visitor not deleted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visitor delete is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateVisitorDetails")]
        public async Task<IActionResult> UpdateVisitorDetails(APIRequest request)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Update visitor details");
            try
            {
                int dbResult = await _adminportal.UpdateVstRegAsync(request);
                if (dbResult > 0)
                {
                    result.Code = dbResult;
                    result.Succeeded = true;
                    result.Message = "Visitor sucessfully updated";
                    _logger.LogInformation("Visitor details successfully updated");
                }
                else
                {
                    result.Code = dbResult;
                    result.Succeeded = false;
                    result.Message = _adminportal.GetErrorMsg();
                    _logger.LogInformation("Visitor details not updated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visitor update is getting error.");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        #endregion

        [HttpPost]
        [Route("GetLocationUnitIDS")]
        public async Task<IActionResult> GetLocationUnitIDS(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Get location Unit IDS");
            try
            {
                var resp = await _adminportal.GetLocationUnitIDSAsync(req);
                if (resp != null)
                {
                    result.UnitsDetailLists = resp.UnitsDetailLists;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Location unit ids is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

        //Validate UnitID

        [HttpPost]
        [Route("ValidateUnitID")]
        public async Task<IActionResult> ValidateUnitID(APIRequest req)
        {
            _logger.LogInformation("Validation unit IDS");
            AppResponse result = new AppResponse();
            try
            {
                int resp = await _adminportal.ValidateUnitIDAsync(req);
                if (resp == 0)
                {
                    result.Code = 0;
                    result.Message = "Unit ID is not Valid";
                    result.Succeeded = false;
                    _logger.LogInformation("Unit ID is not valid");
                }
                else
                {
                    result.Message = "Unit ID is Valid";
                    result.Code = 1;
                    result.Succeeded = true;
                    _logger.LogInformation("Unit ID is valid");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation unit ids is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }


        //ViewAllDetails

        [HttpPost]
        [Route("ViewAllDetails")]
        public async Task<IActionResult> ViewAllDetails(APIRequest request)
        {
            _logger.LogInformation("Getting registration details");
            AppResponse result = new AppResponse();
            RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                var viewmodel = await _adminportal.ViewAllDetailsAsync(request.RequestString);
                if (viewmodel != null)
                {
                    result.rmodel = viewmodel;
                    result.Code = 1;
                    result.Message = "Got It";
                    result.Succeeded = true;
                    // _logger.LogInformation("System able to retrieve Visitor Registration details");
                }
                else
                {
                    result.Message = _adminportal.GetErrorMsg();
                    if (string.IsNullOrEmpty(result.Message))
                    {
                        result.Message = "System unable to retrieve Visitor Registration details";
                    }
                    //_logger.LogInformation("System unable to retrieve Visitor Registration details");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "visitor registration edit modal is getting error");
                result.Message = ex.Message;
            }
            return Ok(result);
        }


        //sandesh
        [HttpPost]
        [Route("GetUpdateNRICPassport")]
        public async Task<IActionResult> GetUpdateNRICPassport(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Updated the NRIC/FIN/Passport");
            try
            {
                var panric = await _adminportal.UpdateNRICPassportAsync(req);
                if (panric > 0)
                {
                    result.Code = 1;
                    result.Message = "Passport/NRIC/FIN is Updated";
                    result.Succeeded = true;
                    _logger.LogInformation("Passport/NRIC/FIN is Updated");
                }
                else
                {
                    result.Message = "Passport/NRIC/FIN is Update Failed";
                    result.Code = 0;
                    result.Succeeded = false;
                    _logger.LogInformation("Passport/NRIC/FIN is Update Failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Updating Failed");
                result.Message = ex.Message;
            }

            return Ok(result);
        }
        [HttpPost]
        [Route("GetExcelregisterlist")]
        public async Task<IActionResult> GetExcelregisterlist(APIRequest req)
        {
            AppResponse result = new AppResponse();
            _logger.LogInformation("Got Registration List");
            try
            {
                var panric = await _adminportal.GetRegistrationsToExcelAsync(req);
                if (panric!=null)
                {
                    result.registrationViewLists = panric.RegistrationViewLists;                   
                    _logger.LogInformation("Got Registration List");
                }
                else
                {
                    result.Message = "Failed to get registartion list";
                    result.Code = 0;
                    result.Succeeded = false;
                    _logger.LogInformation("Failed to get registartion list");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel Export is failed");
                result.Message = ex.Message;
            }
            return Ok(result);
        }

    }
}
