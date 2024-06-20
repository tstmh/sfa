using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace ATTSystems.SFA.DAL.Implementation
{
    public class KioskRepository : IKiosk
    {
        #region Standard Practice to create a Data Repo
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        Message msg = null;
        private IConfiguration config;

      

        ILogger<KioskRepository> logger;
        public KioskRepository(IConfiguration configuration, ILogger<KioskRepository> logger)
        {
            entity = new DataContext();
            config = configuration;
            this.logger = logger;
        }

      

        public string GetErrorMsg()
        {
            return ErrorMsg;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && entity != null)
            {
                entity.Dispose();
            }
        }

    
        #endregion

        public async Task<KioskViewModel> GetExistingPassportNoAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int NId = 0;

            KioskViewModel model = JsonConvert.DeserializeObject<KioskViewModel>(request.Model.ToString());

            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("GetExistingPassportNoAsync invoke started");

                    string dataToEncrypt = model.NRICPassport.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    VisitorRegistration visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);

                    if (visitorRegistration != null)
                    {
                        if (visitorRegistration.IsBlockListed == true)
                        {
                            model.NId = 4;
                            ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                        }
                        //If user deletes the visitor 

                        else if (visitorRegistration.IsDeleted == true)
                        {
                            visitorRegistration.IsDeleted = false;
                            visitorRegistration.VistStartDateTime = DateTime.Now;
                            visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                            entity.SaveChanges();
                        }

                        else
                        {
                            //Check Visitor already registered with same NRIC/Passport with the location

                            var matchingVisitors = entity.VisitorRegistrations.Include(y => y.Locations).Where(x => x.NricOrPassport == encData && x.IsDeleted == false).ToList();

                            foreach (var visLoc in matchingVisitors)
                            {
                                if (visLoc.Locations.Count > 0)
                                {
                                    //Check whether the Visitor already registered with same Location

                                    var locationIds = visLoc.Locations.Select(x => x.Id).ToList();
                                    DateTime visitStartDate = DateTime.Now;

                                    var checkExpiry = entity.VisitorRegistrations.Where(x => x.VistEndDateTime < visitStartDate && x.NricOrPassport == encData).ToList();

                                    if (!locationIds.Contains(model.locationid) && checkExpiry.Count == 0)
                                    {
                                        //Update only if end date reached 1 year(expired), if expired then allow to re-register
                                        //Check if enddate is less than startdate and update accordingly

                                        model.NId = 1;
                                        ErrorMsg = "This NRIC/ Passport has been already Registered";
                                        var visitTypeName = string.Empty;

                                        if (visLoc.VisitTypeId == 1)
                                        {
                                            visitTypeName = "SFA Staff";
                                        }
                                        else if (visLoc.VisitTypeId == 2)
                                        {
                                            visitTypeName = "Tenants";
                                        }
                                        else if (visLoc.VisitTypeId == 3)
                                        {
                                            visitTypeName = "Workers";
                                        }
                                        else if (visLoc.VisitTypeId == 4)
                                        {
                                            visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                        }
                                        else if (visLoc.VisitTypeId == 5)
                                        {
                                            visitTypeName = "Public";
                                        }
                                        else if (visLoc.VisitTypeId == 6)
                                        {
                                            visitTypeName = "Other Government Agency Staff";
                                        }
                                        else if (visLoc.VisitTypeId == 7)
                                        {
                                            visitTypeName = "Managing Agent and Staff";
                                        }

                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(visLoc.NricOrPassport);

                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        model.KioskList.Add(new KioskViewList
                                        {
                                            listId = Convert.ToInt32(visLoc.Id),
                                            listvisitorName = visLoc.VisitorName,
                                            listcompanyName = visLoc.CompanyName,
                                            listmobileNumber = visLoc.VisitorContanctNo,
                                            listvisitorType = visitTypeName,
                                            listvehicleNumber = visLoc.VehicleNo,
                                            listPassportNumber = mask_data,
                                            listEmail = visLoc.Email,
                                        });

                                    }

                                    //If Visitor already exists with same loc or both the locations and the register pass has expired
                                    //show existing details if not just show alert

                                    else
                                    {
                                        if (checkExpiry.Count == 1)
                                        {
                                            var visitTypeName = string.Empty;

                                            if (visLoc.VisitTypeId == 1)
                                            {
                                                visitTypeName = "SFA Staff";
                                            }
                                            else if (visLoc.VisitTypeId == 2)
                                            {
                                                visitTypeName = "Tenants";
                                            }
                                            else if (visLoc.VisitTypeId == 3)
                                            {
                                                visitTypeName = "Workers";
                                            }
                                            else if (visLoc.VisitTypeId == 4)
                                            {
                                                visitTypeName = "Trade Visitors (contractors, commercial buyers, logistics companies)";
                                            }
                                            else if (visLoc.VisitTypeId == 5)
                                            {
                                                visitTypeName = "Public";
                                            }
                                            else if (visLoc.VisitTypeId == 6)
                                            {
                                                visitTypeName = "Other Government Agency Staff";
                                            }
                                            else if (visLoc.VisitTypeId == 7)
                                            {
                                                visitTypeName = "Managing Agent and Staff";
                                            }

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(visLoc.NricOrPassport);

                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.KioskList.Add(new KioskViewList
                                            {
                                                listId = Convert.ToInt32(visLoc.Id),
                                                listvisitorName = visLoc.VisitorName,
                                                listcompanyName = visLoc.CompanyName,
                                                listmobileNumber = visLoc.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = visLoc.VehicleNo,
                                                listPassportNumber = mask_data,
                                                listEmail = visLoc.Email,
                                            });
                                        }
                                        else
                                        {
                                            //randomly passing value 8 to get only alert msg if visitor is already registered
                                            model.alertId = 8;
                                            ErrorMsg = "This NRIC/ Passport has been already Registered. Please proceed to the counter";
                                        }
                                        model.NId = 2;

                                    }
                                }
                                else
                                {
                                    model.NId = 0;
                                }
                            }
                        }

                    }
                    else
                    {
                        ErrorMsg = "This Passport/NRIC/FIN has not been Registered";
                    }


                }
                catch (Exception ex)
                {
                    model.NId = -1;
                    logger.LogError(ex, "Error in GetExistingPassportNoAsync method");
                    LoggerHelper.Instance.LogError(ex);
                }

            }
            return await Task.FromResult<KioskViewModel>(model);
        }
        public async Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request)
        {
            List<VisitType> result = null;

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

        public async Task<KioskViewModel> GetUnitIdListAsync(APIRequest req)
        {
            KioskViewModel model = new KioskViewModel();
            try
            {
                logger.LogInformation("GetUnitIdListAsync invoke started");

                int locid = Convert.ToInt32(config.GetSection("AppSettings:LocationId").Value);

                var Unitslist = entity.UnitDetails.Where(x => x.LocationId == locid).ToList();

                foreach (var item in Unitslist)
                {
                    model.UnitsDetailLists.Add(new UnitsDetailList
                    {
                        Id = item.Id,
                        LocationId = Convert.ToInt32(item.LocationId),
                        BlockNo = item.BlockNo,
                        UnitNo = item.UnitNo,
                        UnitId = item.UnitId
                    });
                }
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
            KioskViewModel result = JsonConvert.DeserializeObject<KioskViewModel>(req.Model.ToString());
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

        public async Task<KioskViewModel> SaveKioskVisitorDetailsAsync(APIRequest request)
        {
            KioskViewModel model = JsonConvert.DeserializeObject<KioskViewModel>(request.Model.ToString());
            string icStr = "";
            using (var entity = new DataContext())
            {
                try
                {
                    logger.LogInformation("SaveVisitorPassportDetailsAsync invoke started");

                    int locid = Convert.ToInt32(config.GetSection("AppSettings:LocationId").Value);

                    string dataToEncrypt = model.NRICPassport.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                    VisitorRegistration visitDtl = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);
                    if (visitDtl == null)
                    {
                        DateTime dt = DateTime.Now;
                        int _idType = 0;

                        if (model.IdType == 1)  
                        {
                            dt = DateTime.Now.AddHours(24);
                            _idType = 1;
                        }
                        if (model.IdType == 2) 
                        {
                            dt = DateTime.Now.AddYears(1);
                            _idType = 2;

                        }

                        VisitorRegistration register = new VisitorRegistration();
                        register.VisitorName = model.visitorName;
                        register.VisitorStatus = 0;
                        register.VisitorContanctNo = model.mobileNumber;
                        register.VehicleNo = model.vehicleNumber;
                        register.IdType = _idType;
                        register.CreateDateTime = DateTime.Now;
                        register.NricOrPassport = encData;
                        register.CompanyName = model.companyName;
                        register.VisitTypeId = Convert.ToInt32(model.visitorType);
                        register.BlockNo = model.blockNo;
                        register.UnitNo = model.unitNo;
                        register.CreateDateTime = DateTime.Now;
                        register.VisitorStatus = 1;
                        register.VistStartDateTime = DateTime.Now;
                        register.VistEndDateTime = dt;
                        register.IsDeleted = false;
                        register.UploadtoController = 1;
                        register.ManualCheckIn = 0;
                        register.RegistrationBy = "Kiosk";
                        register.IsEnabled = false;
                        register.EnabledOverStayer = false;
                        register.PushVisitors = false;
                        register.Email = model.emailId;
                        register.IsDisabled = false;
                        register.CreateBy = "Kiosk";


                        entity.VisitorRegistrations.Add(register);
                        entity.SaveChanges();
                      
                        model.NId = 1;

                        model.NRICPassport = register.NricOrPassport;

                        VisitorRegistration visLoc = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);

                        if (visLoc != null)
                        {

                            List<ModuleViewModel> modules = new List<ModuleViewModel>();

                            UserViewModel roleView = new UserViewModel();

                            var lList = new List<string>();
                            lList.Add(Convert.ToInt32(locid).ToString());

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

                        // ====== Update Card Issue Details for IC
                        if (model.IdType == 2)
                        {

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
                    }

                    else
                    {
                        ErrorMsg = "This NRIC/ Passport has been already Registered";
                   
                        model.NId = -1;
                    }

                }

                catch (Exception ex)
                {
                  
                    model.NId = -1;
                    logger.LogError(ex, "Error in SaveVisitorPassportDetailsAsync method");
                    LoggerHelper.Instance.LogError(ex);
                    ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                }

            }
            return await Task.FromResult<KioskViewModel>(model);
        }

        public async Task<KioskViewModel> UpdateVisitorRegLocationMapping(APIRequest request)
        {
            KioskViewModel? model = null;

            try
            {
                logger.LogInformation("UpdateVisitorRegLocationMapping invoke started");
                model = JsonConvert.DeserializeObject<KioskViewModel>(request.Model.ToString());

                string dataToEncrypt = model.NRICPassport.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                if (model != null)
                {
                    using (var dbContext = new DataContext())
                    {

                        var visitorReg = dbContext.VisitorRegistrations.Include(y => y.Locations).Where(x => x.NricOrPassport == encData && x.IsDeleted == false).FirstOrDefault();

                        if (visitorReg != null)
                        {
                            if (model.IdType == 1) //for Passport
                            {
                                visitorReg.VistStartDateTime = DateTime.Now;
                                visitorReg.VistEndDateTime = DateTime.Now.AddHours(24);
                                visitorReg.UploadtoController = 1;
                                // ====== Update Card Issue Details for Passport, While generating QR, Not here
                                dbContext.SaveChanges();
                            }
                            else if (model.IdType == 2) //for NRIC
                            {
                                visitorReg.VistStartDateTime = DateTime.Now;
                                visitorReg.VistEndDateTime = DateTime.Now.AddYears(+1);
                                visitorReg.UploadtoController = 1;


                                dbContext.SaveChanges();
                            }

                            var locationIds = visitorReg.Locations.Select(x => x.Id).ToList();

                            //Check whether the Visitor already registered with same model Location

                            if (!locationIds.Contains(model.locationid))
                            {
                                VisitorRegistration? visitorLocationMapping = dbContext.VisitorRegistrations.Include(y => y.Locations)
                                                                                    .FirstOrDefault(x => x.Id == visitorReg.Id && x.IsDeleted == false);

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

                                // ====== Update Card Issue Details for IC
                                if (model.IdType == 2) //for NRIC
                                {
                                    var cardIssudetails = dbContext.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
                                    if (cardIssudetails != null)
                                    {
                                        cardIssudetails.IsActive = true;
                                        cardIssudetails.IssueDate = DateTime.Now;
                                    }
                                    else
                                    {
                                        CardIssueDetail cardIssueDetail = new CardIssueDetail
                                        {
                                            CardNumber = encData,
                                            IsActive = true,
                                            CreateDateTime = DateTime.Now,
                                            IssueDate = DateTime.Now,
                                            NricOrPassport = encData
                                        };
                                        dbContext.CardIssueDetails.Add(cardIssueDetail);
                                    }

                                    dbContext.SaveChanges();
                                }

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                model.NId = -1;
                logger.LogError(ex, "Error in UpdateVisitorRegLocationMapping method");
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult<KioskViewModel>(model);

        }

        public async Task<string> UpdateCardNumber(APIRequest request)
        {
            KioskViewModel? model = null;
            string result = "";
            try
            {
                logger.LogInformation("UpdateCardNumber invoke started");
                model = JsonConvert.DeserializeObject<KioskViewModel>(request.Model.ToString());

                string dataToEncrypt = model.NRICPassport.ToUpper();

                string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                if (model != null)
                {
                    using (var dbContext = new DataContext())
                    {

                        var visitorReg = dbContext.VisitorRegistrations.Include(y => y.Locations).Where(x => x.NricOrPassport == encData && x.IsDeleted == false).FirstOrDefault();
                        string cardNumber = model.NRICPassport.ToUpper();
                        if (visitorReg != null)
                        {

                            Setting seetingValue = dbContext.Settings.Where(s => s.Type == request.RequestString).FirstOrDefault();
                            int dbCardNum = Convert.ToInt32(seetingValue.Value);
                            
                            cardNumber = "A" + request.RequestString + dbCardNum.ToString("00000") + seetingValue.Field;

                            if (dbCardNum >= 99999)
                            {
                                seetingValue.Field = RotateCharacters(seetingValue.Field);
                                cardNumber = "00001";
                                dbCardNum = int.Parse(cardNumber);
                                dbCardNum++;
                                string newValue = dbCardNum.ToString("00000");
                                seetingValue.Value = newValue;
                                cardNumber = "A" + request.RequestString + cardNumber + seetingValue.Field;
                            }
                            else
                            {
                                dbCardNum++;
                                string newValue = dbCardNum.ToString("00000");
                                seetingValue.Value = newValue;

                            }
                            string dataToEncryptPass = cardNumber;

                            string encDataPass = EncryptionDecryptionSHA256.Encrypt(dataToEncryptPass);

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
            }

            return new string(charArray);
        }

    }
}
