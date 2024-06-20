using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Web.Models.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Implementation
{
    public class OnlinePortalRepository : IOnlinePortal
    {
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        Message msg = null;
        ILogger<OnlinePortalRepository> logger;
        public OnlinePortalRepository(ILogger<OnlinePortalRepository> logger)
        {
            entity = new DataContext();
            this.logger = logger;
        }
        public string GetErrorMsg()
        {
            return ErrorMsg;
        }
        #region Nikitha
        public async Task<OnlinePortalViewModel> GetExistingPassportNoAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            
                OnlinePortalViewModel? model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString()?? string.Empty);


                using (var entity = new DataContext())
                {
                    try
                    {
                        logger.LogInformation("GetExistingPassportNoAsync invoke started");

                        string dataToEncrypt = model.passportNumber.ToUpper();

                        string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                        VisitorRegistration visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);

                        if (visitorRegistration != null)
                        {
                            if (visitorRegistration.IdType == 2)
                            {
                                model.NId = 7;
                                ErrorMsg = "This Passport has been already registered as a NRIC/FIN";
                            }
                            else
                            {
                                //If user deletes the visitor 

                                if (visitorRegistration.IsDeleted == true)
                                {
                                    visitorRegistration.IsDeleted = false;
                                    visitorRegistration.VistStartDateTime = DateTime.Now;
                                    visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                                    entity.SaveChanges();


                                    // Retrieve all VisitorRegistrations for the given passport number
                                    var matchingVisitors = entity.VisitorRegistrations
                                        .Include(y => y.Locations)
                                        .Where(x => x.NricOrPassport == encData && x.IsDeleted == false).ToList();

                                    foreach (var visLoc in matchingVisitors)
                                    {
                                        if (visLoc.IsBlockListed == true)
                                        {
                                            model.NId = 6;
                                            ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                                        }
                                        else if (visLoc.Locations.Count == 2)
                                        {
                                            if (visLoc.VistEndDateTime < DateTime.Now)
                                            {
                                                foreach (var item in matchingVisitors)
                                                {
                                                    var visitTypeName = string.Empty;

                                                    if (item.VisitTypeId == 1)
                                                    {
                                                        visitTypeName = " SFA Staff";
                                                    }

                                                    else if (item.VisitTypeId == 2)
                                                    {
                                                        visitTypeName = "Tenants";
                                                    }
                                                    else if (item.VisitTypeId == 3)
                                                    {
                                                        visitTypeName = "Workers";
                                                    }
                                                    else if (item.VisitTypeId == 4)
                                                    {
                                                        visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                    }
                                                    else if (item.VisitTypeId == 5)
                                                    {
                                                        visitTypeName = "Public";
                                                    }
                                                    else if (item.VisitTypeId == 6)
                                                    {
                                                        visitTypeName = "Other Government Agency Staff";
                                                    }
                                                    else if (item.VisitTypeId == 7)
                                                    {
                                                        visitTypeName = "Managing Agent and Staff";
                                                    }

                                                    //Decryption
                                                    string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                    var length = decryptedData.Length;
                                                    var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                    model.onlinePortalList.Add(new OnlinePortalViewList
                                                    {
                                                        listId = Convert.ToInt32(item.Id),
                                                        listvisitorName = item.VisitorName,
                                                        listcompanyName = item.CompanyName,
                                                        listmobileNumber = item.VisitorContanctNo,
                                                        listvisitorType = visitTypeName,
                                                        listvehicleNumber = item.VehicleNo,
                                                        listPassportNumber = mask_data,
                                                        listEmail = item.Email,
                                                    });
                                                }
                                                model.NId = 4;
                                            }
                                            else
                                            {
                                                model.NId = 3;
                                                ErrorMsg = "This Passport has been already Registered with both the Locations";
                                            }

                                        }

                                        else if (visLoc.Locations.Count > 0)
                                        {
                                            var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                            //Check whether the Visitor already registered with same Location

                                            if (!locationIds.Contains(model.locationid))
                                            {
                                                //Update only if end date reached 1 year(expired), if expired then allow to re-register
                                                //Check if enddate is less than startdate and update accordingly

                                                model.NId = 1;
                                                ErrorMsg = "This Passport has been already Registered";
                                                foreach (var item in matchingVisitors)
                                                {
                                                    var visitTypeName = string.Empty;

                                                    if (item.VisitTypeId == 1)
                                                    {
                                                        visitTypeName = "SFA Staff";
                                                    }
                                                    else if (item.VisitTypeId == 2)
                                                    {
                                                        visitTypeName = "Tenants";
                                                    }
                                                    else if (item.VisitTypeId == 3)
                                                    {
                                                        visitTypeName = "Workers";
                                                    }
                                                    else if (item.VisitTypeId == 4)
                                                    {
                                                        visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                    }
                                                    else if (item.VisitTypeId == 5)
                                                    {
                                                        visitTypeName = "Public";
                                                    }
                                                    else if (item.VisitTypeId == 6)
                                                    {
                                                        visitTypeName = "Other Government Agency Staff";
                                                    }
                                                    else if (item.VisitTypeId == 7)
                                                    {
                                                        visitTypeName = "Managing Agent and Staff";
                                                    }

                                                    string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                                    var length = decryptedData.Length;
                                                    var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                    model.onlinePortalList.Add(new OnlinePortalViewList
                                                    {
                                                        listId = Convert.ToInt32(item.Id),
                                                        listvisitorName = item.VisitorName,
                                                        listcompanyName = item.CompanyName,
                                                        listmobileNumber = item.VisitorContanctNo,
                                                        listvisitorType = visitTypeName,
                                                        listvehicleNumber = item.VehicleNo,
                                                        listPassportNumber = mask_data,
                                                        listEmail = item.Email,
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                // Visitor is already registered for the selected location
                                                // Update visit details and set NId to 2

                                                if (visLoc.VistEndDateTime < DateTime.Now)
                                                {
                                                    foreach (var item in matchingVisitors)
                                                    {
                                                        var visitTypeName = string.Empty;

                                                        if (item.VisitTypeId == 1)
                                                        {
                                                            visitTypeName = "SFA Staff";
                                                        }
                                                        else if (item.VisitTypeId == 2)
                                                        {
                                                            visitTypeName = "Tenants";
                                                        }
                                                        else if (item.VisitTypeId == 3)
                                                        {
                                                            visitTypeName = "Workers";
                                                        }
                                                        else if (item.VisitTypeId == 4)
                                                        {
                                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                        }
                                                        else if (item.VisitTypeId == 5)
                                                        {
                                                            visitTypeName = "Public";
                                                        }
                                                        else if (item.VisitTypeId == 6)
                                                        {
                                                            visitTypeName = "Other Government Agency Staff";
                                                        }
                                                        else if (item.VisitTypeId == 7)
                                                        {
                                                            visitTypeName = "Managing Agent and Staff";
                                                        }

                                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                                        var length = decryptedData.Length;
                                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                                        {
                                                            listId = Convert.ToInt32(item.Id),
                                                            listvisitorName = item.VisitorName,
                                                            listcompanyName = item.CompanyName,
                                                            listmobileNumber = item.VisitorContanctNo,
                                                            listvisitorType = visitTypeName,
                                                            listvehicleNumber = item.VehicleNo,
                                                            listPassportNumber = mask_data,
                                                            listEmail = item.Email,
                                                        });
                                                    }
                                                    model.NId = 5;
                                                }
                                                else
                                                {
                                                    model.NId = 2;
                                                    ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";
                                                }

                                            }
                                        }
                                    }
                                }

                                else
                                {

                                    // Retrieve all VisitorRegistrations for the given passport number
                                    var matchingVisitors = entity.VisitorRegistrations
                                        .Include(y => y.Locations)
                                        .Where(x => x.NricOrPassport == encData && x.IsDeleted == false).ToList();

                                    foreach (var visLoc in matchingVisitors)
                                    {
                                        if (visLoc.IsBlockListed == true)
                                        {
                                            model.NId = 6;
                                            ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                                        }
                                        else if (visLoc.Locations.Count == 2)
                                        {
                                            if (visLoc.VistEndDateTime < DateTime.Now)
                                            {
                                                foreach (var item in matchingVisitors)
                                                {
                                                    var visitTypeName = string.Empty;

                                                    if (item.VisitTypeId == 1)
                                                    {
                                                        visitTypeName = " SFA Staff";
                                                    }

                                                    else if (item.VisitTypeId == 2)
                                                    {
                                                        visitTypeName = "Tenants";
                                                    }
                                                    else if (item.VisitTypeId == 3)
                                                    {
                                                        visitTypeName = "Workers";
                                                    }
                                                    else if (item.VisitTypeId == 4)
                                                    {
                                                        visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                    }
                                                    else if (item.VisitTypeId == 5)
                                                    {
                                                        visitTypeName = "Public";
                                                    }
                                                    else if (item.VisitTypeId == 6)
                                                    {
                                                        visitTypeName = "Other Government Agency Staff";
                                                    }
                                                    else if (item.VisitTypeId == 7)
                                                    {
                                                        visitTypeName = "Managing Agent and Staff";
                                                    }

                                                    //Decryption
                                                    string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                    var length = decryptedData.Length;
                                                    var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                    model.onlinePortalList.Add(new OnlinePortalViewList
                                                    {
                                                        listId = Convert.ToInt32(item.Id),
                                                        listvisitorName = item.VisitorName,
                                                        listcompanyName = item.CompanyName,
                                                        listmobileNumber = item.VisitorContanctNo,
                                                        listvisitorType = visitTypeName,
                                                        listvehicleNumber = item.VehicleNo,
                                                        listPassportNumber = mask_data,
                                                        listEmail = item.Email,
                                                    });
                                                }
                                                model.NId = 4;
                                            }
                                            else
                                            {
                                                model.NId = 3;
                                                ErrorMsg = "This Passport has been already Registered with both the Locations";
                                            }

                                        }

                                        else if (visLoc.Locations.Count > 0)
                                        {
                                            var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                            //Check whether the Visitor already registered with same Location

                                            if (!locationIds.Contains(model.locationid))
                                            {
                                                //Update only if end date reached 1 year(expired), if expired then allow to re-register
                                                //Check if enddate is less than startdate and update accordingly

                                                model.NId = 1;
                                                ErrorMsg = "This Passport has been already Registered";
                                                foreach (var item in matchingVisitors)
                                                {
                                                    var visitTypeName = string.Empty;

                                                    if (item.VisitTypeId == 1)
                                                    {
                                                        visitTypeName = "SFA Staff";
                                                    }
                                                    else if (item.VisitTypeId == 2)
                                                    {
                                                        visitTypeName = "Tenants";
                                                    }
                                                    else if (item.VisitTypeId == 3)
                                                    {
                                                        visitTypeName = "Workers";
                                                    }
                                                    else if (item.VisitTypeId == 4)
                                                    {
                                                        visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                    }
                                                    else if (item.VisitTypeId == 5)
                                                    {
                                                        visitTypeName = "Public";
                                                    }
                                                    else if (item.VisitTypeId == 6)
                                                    {
                                                        visitTypeName = "Other Government Agency Staff";
                                                    }
                                                    else if (item.VisitTypeId == 7)
                                                    {
                                                        visitTypeName = "Managing Agent and Staff";
                                                    }

                                                    string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                                    var length = decryptedData.Length;
                                                    var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                    model.onlinePortalList.Add(new OnlinePortalViewList
                                                    {
                                                        listId = Convert.ToInt32(item.Id),
                                                        listvisitorName = item.VisitorName,
                                                        listcompanyName = item.CompanyName,
                                                        listmobileNumber = item.VisitorContanctNo,
                                                        listvisitorType = visitTypeName,
                                                        listvehicleNumber = item.VehicleNo,
                                                        listPassportNumber = mask_data,
                                                        listEmail = item.Email,
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                // Visitor is already registered for the selected location
                                                // Update visit details and set NId to 2

                                                if (visLoc.VistEndDateTime < DateTime.Now)
                                                {
                                                    foreach (var item in matchingVisitors)
                                                    {
                                                        var visitTypeName = string.Empty;

                                                        if (item.VisitTypeId == 1)
                                                        {
                                                            visitTypeName = "SFA Staff";
                                                        }
                                                        else if (item.VisitTypeId == 2)
                                                        {
                                                            visitTypeName = "Tenants";
                                                        }
                                                        else if (item.VisitTypeId == 3)
                                                        {
                                                            visitTypeName = "Workers";
                                                        }
                                                        else if (item.VisitTypeId == 4)
                                                        {
                                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                                        }
                                                        else if (item.VisitTypeId == 5)
                                                        {
                                                            visitTypeName = "Public";
                                                        }
                                                        else if (item.VisitTypeId == 6)
                                                        {
                                                            visitTypeName = "Other Government Agency Staff";
                                                        }
                                                        else if (item.VisitTypeId == 7)
                                                        {
                                                            visitTypeName = "Managing Agent and Staff";
                                                        }

                                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                                        var length = decryptedData.Length;
                                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                                        {
                                                            listId = Convert.ToInt32(item.Id),
                                                            listvisitorName = item.VisitorName,
                                                            listcompanyName = item.CompanyName,
                                                            listmobileNumber = item.VisitorContanctNo,
                                                            listvisitorType = visitTypeName,
                                                            listvehicleNumber = item.VehicleNo,
                                                            listPassportNumber = mask_data,
                                                            listEmail = item.Email,
                                                        });
                                                    }
                                                    model.NId = 5;
                                                }
                                                else
                                                {
                                                    model.NId = 2;
                                                    ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";
                                                }

                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            ErrorMsg = "This Passport has not been Registered";
                        }


                    }
                    catch (Exception ex)
                    {
                        model.NId = -1;
                        logger.LogError(ex, "Error in GetExistingPassportNoAsync method");
                        LoggerHelper.Instance.LogError(ex);
                    }
                }
                return await Task.FromResult<OnlinePortalViewModel>(model);
            
        }
        public async Task<List<Location>> GetLocationListAsync(APIRequest request)
        {
            List<Location>? result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("GetLocationListAsync invoke started");

                    var locList = entity.Locations.Where(x => x.IsDeleted == false);
                    if (locList != null)
                    {
                        result = locList.ToList();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in GetLocationListAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }
            }
            return await Task.FromResult<List<Location>>(result);
        }
        public async Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request)
        {
            List<VisitType>? result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("GetVisitorTypeAsync invoke started");

                    var visitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                    if (visitTypeList != null)
                    {
                        result = visitTypeList.ToList();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in GetVisitorTypeAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }
            }
            return await Task.FromResult<List<VisitType>>(result);
        }

        public async Task<OnlinePortalViewModel> GetUnitIdListAsync(APIRequest req)
        {
            OnlinePortalViewModel model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(req.Model.ToString());

            try
            {
                logger.LogInformation("GetUnitIdListAsync invoke started");

                var Unitslist = entity.UnitDetails.Where(x => x.LocationId == model.locationid).ToList();

                foreach (var item in Unitslist)
                {
                    model.OnlineUnitsDetailLists.Add(new OnlineUnitsDetailList
                    {
                        Id = item.Id,
                        LocationId = Convert.ToInt32(item.LocationId),
                        BlockNo = item.BlockNo,
                        UnitNo = item.UnitNo,
                        UnitId = item.UnitId
                    });
                }
                model.OnlineUnitsDetailLists = model.OnlineUnitsDetailLists.OrderBy(x => x.UnitId).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetUnitIdListAsync method");
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult(model);
        }

        public async Task<int> ValidateUnitIDAsync(APIRequest req)
        {
            int res = 0;
            OnlinePortalViewModel result = JsonConvert.DeserializeObject<OnlinePortalViewModel>(req.Model.ToString());
            try
            {
                logger.LogInformation("ValidateUnitIDAsync invoke started");

                UnitDetail site = entity.UnitDetails.FirstOrDefault(x => x.UnitId == result.unitNo);
                if (site != null)
                {
                    res = 1;
                }
                else
                {
                    res = 0;
                }
            }
            catch (Exception ex)
            {
                res = 0;
                logger.LogError(ex, "Error in ValidateUnitIDAsync method");
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult<int>(res);
        }
        public async Task<OnlinePortalViewModel> SaveVisitorPassportDetailsAsync(APIRequest request)
        {
            int _checkId = 0;
            OnlinePortalViewModel model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("SaveVisitorPassportDetailsAsync invoke started");

                    string dataToEncrypt = model.passportNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    VisitorRegistration visitDtl = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);
                    if (visitDtl == null)
                    {
                        int lid = model.locationid;
                        //24 hrs from current date time
                        DateTime currentDateTime = DateTime.Now;
                        DateTime endDateTime = currentDateTime.AddHours(24);

                        VisitorRegistration register = new VisitorRegistration();
                        register.VisitorName = model.visitorName;
                        register.VisitorStatus = 0;
                        register.VisitorContanctNo = model.mobileNumber;
                        register.VehicleNo = model.vehicleNumber;
                        register.IdType = 1;
                        register.CreateDateTime = DateTime.Now;
                        register.NricOrPassport = encData;
                        register.CompanyName = model.companyName;
                        register.Email = model.emailId;
                        register.VisitTypeId = Convert.ToInt32(model.visitorType);
                        register.BlockNo = model.blockNo;
                        register.UnitNo = model.unitNo;
                        register.CreateDateTime = DateTime.Now;
                        register.VisitorStatus = 1;
                        register.VistStartDateTime = DateTime.Now;
                        register.VistEndDateTime = endDateTime;
                        register.IsDeleted = false;
                        register.UploadtoController = 1;
                        register.ManualCheckIn = 0;
                        register.RegistrationBy = "Online Portal";
                        register.IsEnabled = false;
                        register.EnabledOverStayer = false;
                        register.PushVisitors = false;
                        register.IsDisabled = false;
                        register.CreateBy = "Online Portal";
                   

                        entity.VisitorRegistrations.Add(register);
                        entity.SaveChanges();
                        model.NId = 1;

                        model.passportNumber = register.NricOrPassport;

                        VisitorRegistration visLoc = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);

                        if (visLoc != null)
                        {

                            List<ModuleViewModel> modules = new List<ModuleViewModel>();

                            UserViewModel roleView = new UserViewModel();

                            var lList = new List<string>();
                            lList.Add(Convert.ToInt32(model.locationid).ToString());

                            roleView.RoleList = new List<RoleViewModel>();
                            foreach (string rId in lList)
                            {
                                int roleId = 0;
                                if (int.TryParse(rId, out roleId))
                                {
                                    roleView.RoleList.Add(new RoleViewModel { Id = roleId, });
                                }
                            }

                            var roleIdList = roleView.RoleList.Select(x => x.Id);
                            var roleList = entity.Locations.Where(x => roleIdList.Contains(x.Id));
                            if (roleList != null)
                            {
                                foreach (Location role in roleList)
                                {
                                    visLoc.Locations.Add(role);

                                }
                            }
                            entity.SaveChanges();

                        }
                    }

                    else
                    {
                        ErrorMsg = "This Passport has not been Registered";
                        model.NId = -1;
                    }

                }

                catch (Exception ex)
                {
                    model.NId = -1;
                    logger.LogError(ex, "Error in SaveVisitorPassportDetailsAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }

            }
            return await Task.FromResult<OnlinePortalViewModel>(model);
        }

        public async Task<string> UpdateCardNumber(APIRequest request)
        {
            OnlinePortalViewModel? model = null;
            string result = "";
            try
            {
                logger.LogInformation("UpdateCardNumber invoke started");
                model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());

                string dataToEncrypt = model.passportNumber.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                if (model != null)
                {
                    using (var dbContext = new DataContext())
                    {

                        var visitorReg = dbContext.VisitorRegistrations.Include(y => y.Locations).Where(x => x.NricOrPassport == encData && x.IsDeleted == false).FirstOrDefault();
                        string cardNumber = model.passportNumber.ToUpper();
                        if (visitorReg != null)
                        {

                            Setting seetingValue = dbContext.Settings.Where(s => s.Type == "31").FirstOrDefault();
                            int dbCardNum = Convert.ToInt32(seetingValue.Value);

                            cardNumber = "A" + "31" + dbCardNum.ToString("00000") + seetingValue.Field;

                            if (dbCardNum >= 99999)
                            {
                                seetingValue.Field = RotateCharacters(seetingValue.Field);
                                cardNumber = "00001";
                                dbCardNum = int.Parse(cardNumber);
                                dbCardNum++;
                                string newValue = dbCardNum.ToString("00000");
                                seetingValue.Value = newValue;
                                cardNumber = "A" + "31" + cardNumber + seetingValue.Field;
                            }
                            else
                            {
                                dbCardNum++;
                                string newValue = dbCardNum.ToString("00000");
                                seetingValue.Value = newValue;

                            }

                            string encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);

                            CardIssueDetail? issueDetail = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == visitorReg.NricOrPassport && x.IsActive == true);

                            if (issueDetail == null)
                            {
                                CardIssueDetail cardDetails = new CardIssueDetail
                                {
                                    CardNumber = encDataPass,
                                    IsActive = true,
                                    CreateDateTime = DateTime.Now,
                                    IssueDate = DateTime.Now,
                                    NricOrPassport = encData
                                };
                                dbContext.CardIssueDetails.Add(cardDetails);
                                dbContext.SaveChanges();
                                result = cardNumber;

                                msg = new Message(logger);
                                string messageTemplate = msg.GetMessageString(entity);
                                msg.InsertMessage(cardNumber, visitorReg.Email, entity);
                            }
                            else
                            {
                                result = EncryptionDecryptionSHA256.Decrypt(issueDetail.CardNumber);
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                model.NId = -1;
                logger.LogError(ex, "Error in UpdateCardNumber method");
                LoggerHelper.Instance.LogError(ex);
                result = "";
            }
            return await Task.FromResult<string>(result);
        }

        public async Task<OnlinePortalViewModel> UpdatePassVisitorReRegistration(APIRequest request)
        {
            OnlinePortalViewModel? model = null;

            try
            {
                logger.LogInformation("UpdatePassVisitorReRegistration invoke started");
                model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());

                string dataToEncrypt = model.passportNumber.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                if (model != null)
                {
                    using (var dbContext = new DataContext())
                    {
                        var visitorReg = dbContext.VisitorRegistrations.Include(y => y.Locations).Where(x => x.NricOrPassport == encData && x.IsDeleted == false).FirstOrDefault();

                        if (visitorReg != null)
                        {
                            visitorReg.VistStartDateTime = DateTime.Now;
                            visitorReg.VistEndDateTime = DateTime.Now.AddHours(24);
                            visitorReg.UploadtoController = 1;
                            dbContext.SaveChanges();


                            var locationIds = visitorReg.Locations.Select(x => x.Id).ToList();

                            //Check whether the Visitor already registered with same model Location

                            if (!locationIds.Contains(model.locationid))
                            {
                                VisitorRegistration? visitorLocationMapping = dbContext.VisitorRegistrations.Include(y => y.Locations)
                                                                                    .FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);

                                if (visitorLocationMapping != null)
                                {
                                    List<ModuleViewModel> modules = new List<ModuleViewModel>();
                                    UserViewModel roleView = new UserViewModel();
                                    var lList = new List<string>();

                                    lList.Add(Convert.ToInt32(model.locationid).ToString());

                                    roleView.RoleList = new List<RoleViewModel>();
                                    foreach (string rId in lList)
                                    {
                                        int roleId = 0;
                                        if (int.TryParse(rId, out roleId))
                                        {
                                            roleView.RoleList.Add(new RoleViewModel { Id = roleId, });
                                        }
                                    }

                                    var roleIdList = roleView.RoleList.Select(x => x.Id);
                                    var roleList = entity.Locations.Where(x => roleIdList.Contains(x.Id));
                                    if (roleList != null)
                                    {
                                        foreach (Location role in roleList)
                                        {
                                            visitorLocationMapping.Locations.Add(role);

                                        }
                                    }
                                    dbContext.SaveChanges();
                                    model.NId = 1;
                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                model.NId = -1;
                logger.LogError(ex, "Error in UpdatePassVisitorReRegistration method");
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult<OnlinePortalViewModel>(model);

        }

        //newly added one 
        public static string RotateCharacters(string input)
        {
            char[] charArray = input.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                if (char.IsLetter(charArray[i]))
                {
                    if (char.IsLower(charArray[i]))
                    {
                        charArray[i] = (char)(((charArray[i] - 'a' + 1) % 26) + 'a');
                    }
                    else
                    {
                        charArray[i] = (char)(((charArray[i] - 'A' + 1) % 26) + 'A');
                    }
                }
                else if (char.IsDigit(charArray[i]))
                {
                    charArray[i] = (char)(((charArray[i] - '0' + 1) % 10) + '0');
                }
                // For other characters, you can define your own behavior.
            }

            return new string(charArray);
        }

        //*************   NRIC   *************
        #region NRIC
        public async Task<OnlinePortalViewModel> GetNricverifynoAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            string msg;
            //int roleId = 0;


            OnlinePortalViewModel model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());

            try
            {
                logger.LogInformation("GetNricverifynoAsync invoke started");

                string dataToEncrypt = model.NRICNumber.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                //If user deletes the visitor 
                VisitorRegistration visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);

                if (visitorRegistration != null)
                {
                    if (visitorRegistration.IsDeleted == true)
                    {
                        visitorRegistration.IsDeleted = false;
                        visitorRegistration.VistStartDateTime = DateTime.Now;
                        visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                        entity.SaveChanges();


                        // Retrieve all VisitorRegistrations for the given passport number
                        var matchingVisitors = entity.VisitorRegistrations
                            .Include(y => y.Locations)
                            .Where(x => x.NricOrPassport == encData && x.IsDeleted == false).ToList();

                        foreach (var visLoc in matchingVisitors)
                        {
                            if (visLoc.IsBlockListed == true)
                            {
                                model.NId = 6;
                                ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                            }
                            else if (visLoc.Locations.Count == 2)
                            {
                                if (visLoc.VistEndDateTime < DateTime.Now)
                                {
                                    foreach (var item in matchingVisitors)
                                    {
                                        var visitTypeName = string.Empty;

                                        if (item.VisitTypeId == 1)
                                        {
                                            visitTypeName = " SFA Staff";
                                        }

                                        else if (item.VisitTypeId == 2)
                                        {
                                            visitTypeName = "Tenants";
                                        }
                                        else if (item.VisitTypeId == 3)
                                        {
                                            visitTypeName = "Workers";
                                        }
                                        else if (item.VisitTypeId == 4)
                                        {
                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                        }
                                        else if (item.VisitTypeId == 5)
                                        {
                                            visitTypeName = "Public";
                                        }
                                        else if (item.VisitTypeId == 6)
                                        {
                                            visitTypeName = "Other Government Agency Staff";
                                        }
                                        else if (item.VisitTypeId == 7)
                                        {
                                            visitTypeName = "Managing Agent and Staff";
                                        }

                                        //Decryption
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                        {
                                            listId = Convert.ToInt32(item.Id),
                                            listvisitorName = item.VisitorName,
                                            listcompanyName = item.CompanyName,
                                            listmobileNumber = item.VisitorContanctNo,
                                            listvisitorType = visitTypeName,
                                            listvehicleNumber = item.VehicleNo,
                                            listPassportNumber = mask_data,
                                            listEmail = item.Email,
                                        });
                                    }
                                    model.NId = 4;
                                }
                                else
                                {
                                    model.NId = 3;
                                    ErrorMsg = "This NRIC/FIN has been already Registered with both the Locations";
                                }

                            }

                            else if (visLoc.Locations.Count > 0)
                            {
                                var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                //Check whether the Visitor already registered with same Location

                                if (!locationIds.Contains(model.locationid))
                                {
                                    //Update only if end date reached 1 year(expired), if expired then allow to re-register
                                    //Check if enddate is less than startdate and update accordingly

                                    model.NId = 1;
                                    ErrorMsg = "This NRIC/FIN has been already Registered";
                                    foreach (var item in matchingVisitors)
                                    {
                                        var visitTypeName = string.Empty;

                                        if (item.VisitTypeId == 1)
                                        {
                                            visitTypeName = "SFA Staff";
                                        }
                                        else if (item.VisitTypeId == 2)
                                        {
                                            visitTypeName = "Tenants";
                                        }
                                        else if (item.VisitTypeId == 3)
                                        {
                                            visitTypeName = "Workers";
                                        }
                                        else if (item.VisitTypeId == 4)
                                        {
                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                        }
                                        else if (item.VisitTypeId == 5)
                                        {
                                            visitTypeName = "Public";
                                        }
                                        else if (item.VisitTypeId == 6)
                                        {
                                            visitTypeName = "Other Government Agency Staff";
                                        }
                                        else if (item.VisitTypeId == 7)
                                        {
                                            visitTypeName = "Managing Agent and Staff";
                                        }

                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                        {
                                            listId = Convert.ToInt32(item.Id),
                                            listvisitorName = item.VisitorName,
                                            listcompanyName = item.CompanyName,
                                            listmobileNumber = item.VisitorContanctNo,
                                            listvisitorType = visitTypeName,
                                            listvehicleNumber = item.VehicleNo,
                                            listPassportNumber = mask_data,
                                            listEmail = item.Email,
                                        });
                                    }
                                }
                                else
                                {
                                    // Visitor is already registered for the selected location
                                    // Update visit details and set NId to 2

                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        foreach (var item in matchingVisitors)
                                        {
                                            var visitTypeName = string.Empty;

                                            if (item.VisitTypeId == 1)
                                            {
                                                visitTypeName = "SFA Staff";
                                            }
                                            else if (item.VisitTypeId == 2)
                                            {
                                                visitTypeName = "Tenants";
                                            }
                                            else if (item.VisitTypeId == 3)
                                            {
                                                visitTypeName = "Workers";
                                            }
                                            else if (item.VisitTypeId == 4)
                                            {
                                                visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                            }
                                            else if (item.VisitTypeId == 5)
                                            {
                                                visitTypeName = "Public";
                                            }
                                            else if (item.VisitTypeId == 6)
                                            {
                                                visitTypeName = "Other Government Agency Staff";
                                            }
                                            else if (item.VisitTypeId == 7)
                                            {
                                                visitTypeName = "Managing Agent and Staff";
                                            }

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.onlinePortalList.Add(new OnlinePortalViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listPassportNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
                                        model.NId = 5;
                                    }
                                    else
                                    {
                                        model.NId = 2;
                                        ErrorMsg = "This NRIC/FIN has been already Registered with the same Location. Please proceed to the counter";
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        // Retrieve all VisitorRegistrations for the given passport number
                        var matchingVisitors = entity.VisitorRegistrations
                            .Include(y => y.Locations)
                            .Where(x => x.NricOrPassport == encData && x.IsDeleted == false).ToList();

                        foreach (var visLoc in matchingVisitors)
                        {
                            if (visLoc.IsBlockListed == true)
                            {
                                model.NId = 6;
                                ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                            }
                            else if (visLoc.Locations.Count == 2)
                            {
                                if (visLoc.VistEndDateTime < DateTime.Now)
                                {
                                    foreach (var item in matchingVisitors)
                                    {
                                        var visitTypeName = string.Empty;

                                        if (item.VisitTypeId == 1)
                                        {
                                            visitTypeName = " SFA Staff";
                                        }

                                        else if (item.VisitTypeId == 2)
                                        {
                                            visitTypeName = "Tenants";
                                        }
                                        else if (item.VisitTypeId == 3)
                                        {
                                            visitTypeName = "Workers";
                                        }
                                        else if (item.VisitTypeId == 4)
                                        {
                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                        }
                                        else if (item.VisitTypeId == 5)
                                        {
                                            visitTypeName = "Public";
                                        }
                                        else if (item.VisitTypeId == 6)
                                        {
                                            visitTypeName = "Other Government Agency Staff";
                                        }
                                        else if (item.VisitTypeId == 7)
                                        {
                                            visitTypeName = "Managing Agent and Staff";
                                        }

                                        //Decryption
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                        {
                                            listId = Convert.ToInt32(item.Id),
                                            listvisitorName = item.VisitorName,
                                            listcompanyName = item.CompanyName,
                                            listmobileNumber = item.VisitorContanctNo,
                                            listvisitorType = visitTypeName,
                                            listvehicleNumber = item.VehicleNo,
                                            listPassportNumber = mask_data,
                                            listEmail = item.Email,
                                        });
                                    }
                                    model.NId = 4;
                                }
                                else
                                {
                                    model.NId = 3;
                                    ErrorMsg = "This NRIC/FIN has been already Registered with both the Locations";
                                }

                            }

                            else if (visLoc.Locations.Count > 0)
                            {
                                var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                //Check whether the Visitor already registered with same Location

                                if (!locationIds.Contains(model.locationid))
                                {
                                    //Update only if end date reached 1 year(expired), if expired then allow to re-register
                                    //Check if enddate is less than startdate and update accordingly

                                    model.NId = 1;
                                    ErrorMsg = "This NRIC/FIN has been already Registered";
                                    foreach (var item in matchingVisitors)
                                    {
                                        var visitTypeName = string.Empty;

                                        if (item.VisitTypeId == 1)
                                        {
                                            visitTypeName = "SFA Staff";
                                        }
                                        else if (item.VisitTypeId == 2)
                                        {
                                            visitTypeName = "Tenants";
                                        }
                                        else if (item.VisitTypeId == 3)
                                        {
                                            visitTypeName = "Workers";
                                        }
                                        else if (item.VisitTypeId == 4)
                                        {
                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                        }
                                        else if (item.VisitTypeId == 5)
                                        {
                                            visitTypeName = "Public";
                                        }
                                        else if (item.VisitTypeId == 6)
                                        {
                                            visitTypeName = "Other Government Agency Staff";
                                        }
                                        else if (item.VisitTypeId == 7)
                                        {
                                            visitTypeName = "Managing Agent and Staff";
                                        }

                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        model.onlinePortalList.Add(new OnlinePortalViewList
                                        {
                                            listId = Convert.ToInt32(item.Id),
                                            listvisitorName = item.VisitorName,
                                            listcompanyName = item.CompanyName,
                                            listmobileNumber = item.VisitorContanctNo,
                                            listvisitorType = visitTypeName,
                                            listvehicleNumber = item.VehicleNo,
                                            listPassportNumber = mask_data,
                                            listEmail = item.Email,
                                        });
                                    }
                                }
                                else
                                {
                                    // Visitor is already registered for the selected location
                                    // Update visit details and set NId to 2

                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        foreach (var item in matchingVisitors)
                                        {
                                            var visitTypeName = string.Empty;

                                            if (item.VisitTypeId == 1)
                                            {
                                                visitTypeName = "SFA Staff";
                                            }
                                            else if (item.VisitTypeId == 2)
                                            {
                                                visitTypeName = "Tenants";
                                            }
                                            else if (item.VisitTypeId == 3)
                                            {
                                                visitTypeName = "Workers";
                                            }
                                            else if (item.VisitTypeId == 4)
                                            {
                                                visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                            }
                                            else if (item.VisitTypeId == 5)
                                            {
                                                visitTypeName = "Public";
                                            }
                                            else if (item.VisitTypeId == 6)
                                            {
                                                visitTypeName = "Other Government Agency Staff";
                                            }
                                            else if (item.VisitTypeId == 7)
                                            {
                                                visitTypeName = "Managing Agent and Staff";
                                            }

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.onlinePortalList.Add(new OnlinePortalViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listPassportNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
                                        model.NId = 5;
                                    }
                                    else
                                    {
                                        model.NId = 2;
                                        ErrorMsg = "This NRIC/FIN has been already Registered with the same Location. Please proceed to the counter";
                                    }

                                }
                            }
                        }
                    }


                }
                else
                {
                    ErrorMsg = "This Passport has not been Registered";
                }


            }
            catch (Exception ex)
            {
                model.NId = -1;
                logger.LogError(ex, "Error in GetNricverifynoAsync method");
                LoggerHelper.Instance.LogError(ex);
            }

            return await Task.FromResult<OnlinePortalViewModel>(model);
        }
        public async Task<List<VisitType>> GetNricVisitorTypeAsync(APIRequest request)
        {
            List<VisitType> result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("GetNricVisitorTypeAsync invoke started");

                    var nricvisitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                    if (nricvisitTypeList != null)
                    {
                        result = nricvisitTypeList.ToList();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in GetNricVisitorTypeAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }
            }
            return await Task.FromResult<List<VisitType>>(result);
        }
        public async Task<List<Location>> GetNricLocationAsync(APIRequest request)
        {
            List<Location> result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("GetNricLocationAsync invoke started");

                    var nricloclist = entity.Locations.Where(x => x.IsDeleted == false);
                    if (nricloclist != null)
                    {
                        result = nricloclist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in GetNricLocationAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }
            }
            return await Task.FromResult<List<Location>>(result);
        }
        public async Task<int> NricdtlsaveinfoAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            string msg;
            int _checkId = 0;

            OnlinePortalViewModel? model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());

            try
            {
                logger.LogInformation("NricdtlsaveinfoAsync invoke started");

                string? dataToEncrypt = model.NRICNumber.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                VisitorRegistration? checkPass = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);

                if (checkPass == null)
                {
                    string lid = model.location;
                    DateTime currentDateTime = DateTime.Now;
                    DateTime endDateTime = currentDateTime.AddYears(+1);

                    VisitorRegistration register = new VisitorRegistration();
                    register.VisitorName = model.visitorName;
                    register.VisitorStatus = 0;
                    register.VisitorContanctNo = model.mobileNumber;
                    register.VehicleNo = model.vehicleNumber;
                    register.IdType = 2;
                    register.CreateDateTime = DateTime.Now;
                    register.NricOrPassport = encData;
                    register.CompanyName = model.companyName;
                    register.Email = model.emailId;
                    register.VisitTypeId = Convert.ToInt32(model.visitorType);
                    register.BlockNo = model.blockNo;
                    register.UnitNo = model.unitNo;
                    register.CreateDateTime = DateTime.Now;
                    register.VisitorStatus = 1;
                    register.VistStartDateTime = DateTime.Now;
                    register.VistEndDateTime = endDateTime;
                    register.IsDeleted = false;
                    register.UploadtoController = 1;
                    register.ManualCheckIn = 0;
                    register.RegistrationBy = "Online Portal";
                    register.IsEnabled = false;
                    register.EnabledOverStayer = false;
                    register.PushVisitors = false;
                    register.IsDisabled = false;
                    register.CreateBy = "Online Portal";

                    entity.VisitorRegistrations.Add(register);
                    entity.SaveChanges();
                    _checkId = 1;

                    VisitorRegistration visLoc = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);

                    if (visLoc != null)
                    {

                        List<ModuleViewModel> modules = new List<ModuleViewModel>();

                        UserViewModel roleView = new UserViewModel();

                        var lList = new List<string>();
                        lList.Add(Convert.ToInt32(model.locationid).ToString());

                        roleView.RoleList = new List<RoleViewModel>();
                        foreach (string rId in lList)
                        {
                            int roleId = 0;
                            if (int.TryParse(rId, out roleId))
                            {
                                roleView.RoleList.Add(new RoleViewModel { Id = roleId, });
                            }
                        }

                        var roleIdList = roleView.RoleList.Select(x => x.Id);
                        var roleList = entity.Locations.Where(x => roleIdList.Contains(x.Id));
                        if (roleList != null)
                        {
                            foreach (Location role in roleList)
                            {
                                visLoc.Locations.Add(role);

                            }
                        }
                        entity.SaveChanges();
                    }


                    CardIssueDetail cardIssueDetail = new CardIssueDetail
                    {
                        CardNumber = encData,
                        IsActive = true,
                        CreateDateTime = DateTime.Now,
                        IssueDate = DateTime.Now,
                        NricOrPassport = encData,
                    };
                    entity.CardIssueDetails.Add(cardIssueDetail);
                    entity.SaveChanges();
                }

                else
                {
                    ErrorMsg = "This NRIC has not been Registered";
                    _checkId = -1;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in NricdtlsaveinfoAsync method");
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult<int>(_checkId);
        }


        public async Task<OnlinePortalViewModel> UpdateNRICVisitorReRegistration(APIRequest request)
        {
            using (var entity = new DataContext())
            {
                OnlinePortalViewModel model = JsonConvert.DeserializeObject<OnlinePortalViewModel>(request.Model.ToString());
                try
                {
                    //Encription
                    string result = "";
                    string dataToEncrypt = model.NRICNumber.ToUpper();
                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData && X.IsDeleted == false).ToList();
                    if (visitor.Count > 0)
                    {
                        foreach (var visLoc in visitor)
                        {
                            if (visLoc.Locations.Count == 2)
                            {
                                if (visLoc.VistEndDateTime < DateTime.Now)
                                {
                                    visLoc.VistStartDateTime = DateTime.Now;
                                    visLoc.VistEndDateTime = DateTime.Now.AddYears(1);
                                    entity.SaveChanges();

                                    //check cardetails and update for nric

                                    CardIssueDetail card_dtls = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
                                    if (card_dtls == null)
                                    {
                                        CardIssueDetail cardIssueDtls = new CardIssueDetail
                                        {
                                            CardNumber = encData,
                                            IsActive = true,
                                            CreateDateTime = DateTime.Now,
                                            IssueDate = DateTime.Now,
                                            NricOrPassport = encData,
                                        };
                                        entity.CardIssueDetails.Add(cardIssueDtls);
                                        entity.SaveChanges();
                                    }
                                    else
                                    {
                                        card_dtls.IssueDate = DateTime.Now;
                                        card_dtls.IsActive = true;
                                        entity.SaveChanges();
                                    }

                                }
                            }
                            else if (visLoc.Locations.Count > 0)
                            {
                                var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                // If Visitor is not registered for the selected location
                                if (!locationIds.Contains(model.locationid))
                                {
                                    model.NId = 1;
                                    ErrorMsg = "This NRIC/FIN has been already Registered";

                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        visLoc.VistStartDateTime = DateTime.Now;
                                        visLoc.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();

                                        //check cardetails and update for nric

                                        CardIssueDetail card_dtls = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
                                        if (card_dtls == null)
                                        {
                                            CardIssueDetail cardIssueDtls = new CardIssueDetail
                                            {
                                                CardNumber = encData,
                                                IsActive = true,
                                                CreateDateTime = DateTime.Now,
                                                IssueDate = DateTime.Now,
                                                NricOrPassport = encData,
                                            };
                                            entity.CardIssueDetails.Add(cardIssueDtls);
                                            entity.SaveChanges();
                                        }
                                        else
                                        {
                                            card_dtls.IssueDate = DateTime.Now;
                                            card_dtls.IsActive = true;
                                            entity.SaveChanges();
                                        }
                                    }
                                    else
                                    {

                                        UserViewModel roleView = new UserViewModel();

                                        var lList = new List<string>();
                                        lList.Add(Convert.ToInt32(model.locationid).ToString());

                                        roleView.RoleList = new List<RoleViewModel>();
                                        foreach (string rId in lList)
                                        {
                                            int roleId = 0;
                                            if (int.TryParse(rId, out roleId))
                                            {
                                                roleView.RoleList.Add(new RoleViewModel { Id = roleId, });
                                            }
                                        }

                                        var roleIdList = roleView.RoleList.Select(x => x.Id);
                                        var roleList = entity.Locations.Where(x => roleIdList.Contains(x.Id));
                                        if (roleList != null)
                                        {
                                            foreach (Location role in roleList)
                                            {
                                                visLoc.Locations.Add(role);

                                            }
                                        }
                                        entity.SaveChanges();


                                        CardIssueDetail card_dtls = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
                                        if (card_dtls == null)
                                        {
                                            CardIssueDetail cardIssueDtls = new CardIssueDetail
                                            {
                                                CardNumber = encData,
                                                IsActive = true,
                                                CreateDateTime = DateTime.Now,
                                                IssueDate = DateTime.Now,
                                                NricOrPassport = encData,
                                            };
                                            entity.CardIssueDetails.Add(cardIssueDtls);
                                            entity.SaveChanges();
                                        }
                                        else
                                        {
                                            card_dtls.IssueDate = DateTime.Now;
                                            card_dtls.IsActive = true;
                                            entity.SaveChanges();
                                        }
                                    }
                                }

                                else
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        visLoc.VistStartDateTime = DateTime.Now;
                                        visLoc.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();

                                        //check cardetails and update for nric

                                        CardIssueDetail card_dtls = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
                                        if (card_dtls == null)
                                        {
                                            CardIssueDetail cardIssueDtls = new CardIssueDetail
                                            {
                                                CardNumber = encData,
                                                IsActive = true,
                                                CreateDateTime = DateTime.Now,
                                                IssueDate = DateTime.Now,
                                                NricOrPassport = encData,
                                            };
                                            entity.CardIssueDetails.Add(cardIssueDtls);
                                            entity.SaveChanges();
                                        }
                                        else
                                        {
                                            card_dtls.IssueDate = DateTime.Now;
                                            card_dtls.IsActive = true;
                                            entity.SaveChanges();
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        ErrorMsg = "This NRIC/FIN has not been already Registered";
                    }

                }
                catch (Exception ex)
                {
                    model.NId = -1;
                    LoggerHelper.Instance.LogError(ex);
                }

                return await Task.FromResult<OnlinePortalViewModel>(model);
            }

        }


        #endregion
        #endregion
    }
}
