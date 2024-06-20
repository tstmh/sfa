using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ATTSystems.SFA.DAL.Implementation
{
    public class MobileDeviceRepository : IMobileDevice
    {
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        Message msg = null;
        private IConfiguration config;
        ILogger<MobileDeviceRepository> logger;
        public MobileDeviceRepository(IConfiguration configuration, ILogger<MobileDeviceRepository> logger)
        {
            config = configuration;
            entity = new DataContext();
            this.logger = logger;
        }
        public string GetErrorMsg()
        {
            return ErrorMsg;
        }


        public async Task<MobileDeviceViewModel> AsyncRegisternric(APIRequest request)
        {
            using (var entity = new DataContext())
            {
                MobileDeviceViewModel model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
                try
                {
                    string result = "";
                    string dataToEncrypt = model.NRICNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);

                    VisitorRegistration visitor1 = entity.VisitorRegistrations.FirstOrDefault(X => X.NricOrPassport == encData);

                    if (visitor1 != null)
                    {
                        if (visitor1.IsDeleted == true)
                        {
                            visitor1.IsDeleted = false;
                            visitor1.VistStartDateTime = DateTime.Now;
                            visitor1.VistEndDateTime = DateTime.Now.AddYears(1);
                            entity.SaveChanges();
                            var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData && X.IsDeleted == false).ToList();
                            foreach (var visLoc in visitor)
                            {
                                if (visLoc.Locations.Count == 2)
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        model.NId = 4;
                                        ErrorMsg = "This NRIC/FIN has been already Registered with both the Locations";
                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);


                                            model.ListMobileDevice.Add(new MobileDeviceViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listNRICNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
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
                                    if (!locationIds.Contains(locid))
                                    {
                                        model.NId = 1;
                                        ErrorMsg = "This NRIC/FIN has been already Registered";

                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);


                                            model.ListMobileDevice.Add(new MobileDeviceViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listNRICNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        if (visLoc.VistEndDateTime < DateTime.Now)
                                        {
                                            model.NId = 5;
                                            ErrorMsg = "This NRIC/FIN has been already Registered with the same Location. Please proceed to the counter";

                                            foreach (var item in visitor)
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

                                                string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                var length = decryptedData.Length;
                                                var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                model.ListMobileDevice.Add(new MobileDeviceViewList
                                                {
                                                    listId = Convert.ToInt32(item.Id),
                                                    listvisitorName = item.VisitorName,
                                                    listcompanyName = item.CompanyName,
                                                    listmobileNumber = item.VisitorContanctNo,
                                                    listvisitorType = visitTypeName,
                                                    listvehicleNumber = item.VehicleNo,
                                                    listNRICNumber = mask_data,
                                                    listEmail = item.Email,
                                                });
                                            }
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
                        else if (visitor1.IsBlockListed == true)
                        {
                            model.NId = 6;
                            ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                        }
                        else
                        {
                            var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData && X.IsDeleted == false).ToList();

                            foreach (var visLoc in visitor)
                            {
                                if (visLoc.Locations.Count == 2)
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        model.NId = 4;
                                        ErrorMsg = "This NRIC/FIN has been already Registered with both the Locations";
                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);


                                            model.ListMobileDevice.Add(new MobileDeviceViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listNRICNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
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
                                    if (!locationIds.Contains(locid))
                                    {
                                        model.NId = 1;
                                        ErrorMsg = "This NRIC/FIN has been already Registered";

                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);


                                            model.ListMobileDevice.Add(new MobileDeviceViewList
                                            {
                                                listId = Convert.ToInt32(item.Id),
                                                listvisitorName = item.VisitorName,
                                                listcompanyName = item.CompanyName,
                                                listmobileNumber = item.VisitorContanctNo,
                                                listvisitorType = visitTypeName,
                                                listvehicleNumber = item.VehicleNo,
                                                listNRICNumber = mask_data,
                                                listEmail = item.Email,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        if (visLoc.VistEndDateTime < DateTime.Now)
                                        {
                                            model.NId = 5;
                                            ErrorMsg = "This NRIC/FIN has been already Registered with the same Location. Please proceed to the counter";

                                            foreach (var item in visitor)
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

                                                string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                var length = decryptedData.Length;
                                                var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                model.ListMobileDevice.Add(new MobileDeviceViewList
                                                {
                                                    listId = Convert.ToInt32(item.Id),
                                                    listvisitorName = item.VisitorName,
                                                    listcompanyName = item.CompanyName,
                                                    listmobileNumber = item.VisitorContanctNo,
                                                    listvisitorType = visitTypeName,
                                                    listvehicleNumber = item.VehicleNo,
                                                    listNRICNumber = mask_data,
                                                    listEmail = item.Email,
                                                });
                                            }
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
                        ErrorMsg = "This NRIC/FIN has not been Registered";

                    }
                }
                catch (Exception ex)
                {
                    model.NId = -1;
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }

                return await Task.FromResult<MobileDeviceViewModel>(model);
            }

        }


        public async Task<int> AsyncSavenricRegisterdtls(APIRequest request)
        {
            int result = 0;
            MobileDeviceViewModel model = new MobileDeviceViewModel();
            model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                try
                {

                    string dataToEncrypt = model.NRICNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    VisitorRegistration nricDtls = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);
                    if (nricDtls == null)
                    {
                        DateTime currentDateTime = DateTime.Now;
                        DateTime endDateTime = currentDateTime.AddYears(+1);

                        VisitorRegistration reg = new VisitorRegistration();
                        reg.VisitorName = model.visitorName;
                        reg.VisitorStatus = 0;
                        reg.VisitorContanctNo = model.mobileNumber;
                        reg.VehicleNo = model.vehicleNumber;
                        reg.IdType = 2;
                        reg.CreateDateTime = DateTime.Now;
                        reg.NricOrPassport = encData;
                        reg.CompanyName = model.companyName;
                        reg.Email = model.emailId;
                        reg.VisitTypeId = Convert.ToInt32(model.VisitTypeId);
                        reg.BlockNo = model.blockNo;
                        reg.UnitNo = model.unitNo;
                        reg.CreateDateTime = DateTime.Now;
                        reg.VisitorStatus = 1;
                        reg.ManualCheckIn = 1;
                        reg.RegistrationBy = "MobileApp";
                        reg.UploadtoController = 1;
                        reg.IsEnabled = false;
                        reg.EnabledOverStayer = false;
                        reg.PushVisitors = false;
                        reg.IsDisabled = false;
                        reg.CreateBy = "MobileApp";


                        reg.VistStartDateTime = DateTime.Now;
                        reg.VistEndDateTime = endDateTime;

                        entity.VisitorRegistrations.Add(reg);
                        entity.SaveChanges();
                        result = 1;

                        int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);
                        VisitorRegistration vLoc = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.Id == reg.Id);

                        if (vLoc != null)
                        {
                            UserViewModel roleView = new UserViewModel();

                            var lList = new List<string>();
                            lList.Add(locid.ToString());

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
                                    vLoc.Locations.Add(role);
                                }
                            }
                            entity.SaveChanges();

                        }


                        var cardIssudetails = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData);
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
                            entity.CardIssueDetails.Add(cardIssueDetail);
                        }

                        entity.SaveChanges();

                    }
                    else
                    {
                        ErrorMsg = "This NRIC/FIN has not been Registered";
                        result = 2;
                    }

                }
                catch (Exception ex)
                {
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }

            }
            return await Task.FromResult<int>(result);
        }

        public async Task<MobileDeviceViewModel> AsyncExitsSaveNric(APIRequest request)
        {
            using (var entity = new DataContext())
            {
                MobileDeviceViewModel model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
                try
                {
                    //Encription
                    string result = "";
                    string dataToEncrypt = model.NRICNumber.ToUpper();
                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);
                    var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData).ToList();
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

                                    //cardetails check and update for nric

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
                                        card_dtls.IsActive = true;
                                        card_dtls.IssueDate = DateTime.Now;
                                        entity.SaveChanges();
                                    }

                                }

                            }
                            else if (visLoc.Locations.Count > 0)
                            {
                                var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                // If Visitor is not registered for the selected location
                                if (!locationIds.Contains(locid))
                                {
                                    model.NId = 1;
                                    ErrorMsg = "This NRIC/FIN has been already Registered";
                                    //NId = 1;

                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        visLoc.VistStartDateTime = DateTime.Now;
                                        visLoc.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();

                                        //cardetails check and update for nric

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
                                            card_dtls.IsActive = true;
                                            card_dtls.IssueDate = DateTime.Now;
                                            entity.SaveChanges();
                                        }
                                    }
                                    else
                                    {

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
                                            card_dtls.IsActive = true;
                                            card_dtls.IssueDate = DateTime.Now;
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

                                        //cardetails check and update for nric

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
                                            card_dtls.IsActive = true;
                                            card_dtls.IssueDate = DateTime.Now;
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
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }

                return await Task.FromResult<MobileDeviceViewModel>(model);
            }

        }

   

        public async Task<MobileDeviceViewModel> AsyncRegisterPassport(APIRequest request)
        {
            MobileDeviceViewModel model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                try
                {
                    string dataToEncrypt = model.passportNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);

                    VisitorRegistration visitor1 = entity.VisitorRegistrations.FirstOrDefault(X => X.NricOrPassport == encData);

                    if (visitor1 != null)
                    {
                        if (visitor1.IsDeleted == true)
                        {
                            visitor1.IsDeleted = false;
                            visitor1.VistStartDateTime = DateTime.Now;
                            visitor1.VistEndDateTime = DateTime.Now.AddHours(24);
                            entity.SaveChanges();
                            var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData && X.IsDeleted == false).ToList();

                            foreach (var visLoc in visitor)
                            {
                                if (visLoc.Locations.Count == 2)
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        model.NId = 4;
                                        ErrorMsg = "This Passport has been already Registered with both the Locations";
                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                        model.NId = 3;
                                        ErrorMsg = "This Passport has been already Registered with both the Locations";
                                    }
                                }
                                else if (visLoc.Locations.Count > 0)
                                {
                                    var locationIds = visLoc.Locations.Select(x => x.Id).ToList();
                                    if (!locationIds.Contains(locid))
                                    {
                                        model.NId = 1;
                                        ErrorMsg = "This Passport has been already Registered";

                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                        if (visLoc.VistEndDateTime < DateTime.Now)
                                        {
                                            model.NId = 5;
                                            ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";

                                            foreach (var item in visitor)
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

                                                string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                var length = decryptedData.Length;
                                                var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                            model.NId = 2;
                                            ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";
                                        }
                                    }
                                }
                            }
                        }
                        else if (visitor1.IdType == 2)
                        {
                            model.NId = 6;
                            ErrorMsg = "This Passport has been already Exits as a NIRC/FIN Number";

                        }
                        else if (visitor1.IsBlockListed == true)
                        {
                            model.NId = 7;
                            ErrorMsg = "This Visitor is Blacklisted. Please proceed to counter";
                        }


                        else
                        {
                            var visitor = entity.VisitorRegistrations.Include(x => x.Locations).Where(X => X.NricOrPassport == encData && X.IsDeleted == false).ToList();

                            foreach (var visLoc in visitor)
                            {
                                if (visLoc.Locations.Count == 2)
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        model.NId = 4;
                                        ErrorMsg = "This Passport has been already Registered with both the Locations";
                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                        model.NId = 3;
                                        ErrorMsg = "This Passport has been already Registered with both the Locations";
                                    }
                                }
                                else if (visLoc.Locations.Count > 0)
                                {
                                    var locationIds = visLoc.Locations.Select(x => x.Id).ToList();
                                    if (!locationIds.Contains(locid))
                                    {
                                        model.NId = 1;
                                        ErrorMsg = "This Passport has been already Registered";

                                        foreach (var item in visitor)
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

                                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                            var length = decryptedData.Length;
                                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                            model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                        if (visLoc.VistEndDateTime < DateTime.Now)
                                        {
                                            model.NId = 5;
                                            ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";

                                            foreach (var item in visitor)
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

                                                string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                                                var length = decryptedData.Length;
                                                var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                                model.ListMobileDevice.Add(new MobileDeviceViewList
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
                                            model.NId = 2;
                                            ErrorMsg = "This Passport has been already Registered with the same Location. Please proceed to the counter";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ErrorMsg = "This Passport has not been already Registered";

                    }
                }
                catch (Exception ex)
                {
                    model.NId = -1;
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());

                }

                return await Task.FromResult<MobileDeviceViewModel>(model);
            }

        }

        public async Task<int> AsyncSavePassportRegisterdtls(APIRequest request)
        {
            int result = 0;
            MobileDeviceViewModel model = new MobileDeviceViewModel();
            model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                try
                {
                    //Encription
                    string dataToEncrypt = model.passportNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);


                    VisitorRegistration nricDtls = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);
                    if (nricDtls == null)
                    {
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
                        register.VisitTypeId = Convert.ToInt32(model.VisitTypeId);
                        register.BlockNo = model.blockNo;
                        register.UnitNo = model.unitNo;
                        register.CreateDateTime = DateTime.Now;
                        register.VisitorStatus = 1;
                        register.VistStartDateTime = DateTime.Now;
                        register.VistEndDateTime = endDateTime;
                        register.ManualCheckIn = 1;
                        register.RegistrationBy = "MobileApp";
                        register.UploadtoController = 1;
                        register.IsEnabled = false;
                        register.EnabledOverStayer = false;
                        register.PushVisitors = false;
                        register.IsDisabled = false;
                        register.CreateBy = "MobileApp";

                        entity.VisitorRegistrations.Add(register);
                        entity.SaveChanges();
                        result = 1;

                        // Location ID save

                        int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);
                        VisitorRegistration vLoc = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.Id == register.Id);

                        if (vLoc != null)
                        {
                            UserViewModel roleView = new UserViewModel();

                            var lList = new List<string>();
                            lList.Add(locid.ToString());

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
                                    vLoc.Locations.Add(role);
                                }
                            }
                            entity.SaveChanges();
                        }
                        else
                        {
                            ErrorMsg = "This PassPort has not been Registered";
                            result = 2;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());

                }

            }
            return await Task.FromResult<int>(result);
        }

        public async Task<MobileDeviceViewModel> AsyncExitsSavePassport(APIRequest request)
        {
            MobileDeviceViewModel model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                try
                {
                    //Encryption
                    string dataToEncrypt = model.passportNumber.ToUpper();

                    string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                    int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);

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
                                    visLoc.VistEndDateTime = DateTime.Now.AddHours(24);
                                    entity.SaveChanges();


                                }
                            }
                            else if (visLoc.Locations.Count > 0)
                            {
                                var locationIds = visLoc.Locations.Select(x => x.Id).ToList();

                                // If Visitor is not registered for the selected location
                                if (!locationIds.Contains(locid))
                                {
                                    model.NId = 1;
                                    ErrorMsg = "This NRIC/FIN has been already Registered";
                                    //NId = 1;

                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        visLoc.VistStartDateTime = DateTime.Now;
                                        visLoc.VistEndDateTime = DateTime.Now.AddHours(24);
                                        entity.SaveChanges();
                                    }

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
                                else
                                {
                                    if (visLoc.VistEndDateTime < DateTime.Now)
                                    {
                                        visLoc.VistStartDateTime = DateTime.Now;
                                        visLoc.VistEndDateTime = DateTime.Now.AddHours(24);
                                        entity.SaveChanges();


                                    }
                                }

                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    model.NId = -1;
                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }
            }

            return await Task.FromResult<MobileDeviceViewModel>(model);//retun 0 if not registered
        }


        public async Task<List<VisitType>> GetNricVisitorTypeAsync(APIRequest request)
        {
            List<VisitType> result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    var nricvisitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                    if (nricvisitTypeList != null)
                    {
                        result = nricvisitTypeList.ToList();
                    }
                }
                catch (Exception ex)
                {

                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }
            }
            return await Task.FromResult<List<VisitType>>(result);
        }


        public async Task<List<VisitType>> AsyncVisitorTypePassport(APIRequest request)
        {
            List<VisitType> result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    var nricvisitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                    if (nricvisitTypeList != null)
                    {
                        result = nricvisitTypeList.ToList();
                    }
                }
                catch (Exception ex)
                {

                    //LoggerHelper.Instance.LogError(ex);
                    logger.LogError(ex.ToString());
                }
            }
            return await Task.FromResult<List<VisitType>>(result);
        }


        public async Task<MobileDeviceViewModel> GetLocationUnitIDSAsync(APIRequest req)
        {
            MobileDeviceViewModel model = new MobileDeviceViewModel();
            MobileDeviceViewModel result = JsonConvert.DeserializeObject<MobileDeviceViewModel>(req.Model.ToString());
            try
            {
                int locid = Convert.ToInt32(config.GetSection("AppSettings:Locationid").Value);
                var Unitslist = entity.UnitDetails.Where(x => x.LocationId == locid).ToList();
                foreach (var item in Unitslist)
                {
                    model.mobileunitsDetailLists.Add(new MobileUnitsDetailList
                    {
                        Id = item.Id,
                        LocationId = Convert.ToInt32(item.LocationId),
                        BlockNo = item.BlockNo,
                        UnitNo = item.UnitNo,
                        UnitId = item.UnitId
                    });
                    model.locationid = locid;
                }
                model.mobileunitsDetailLists = model.mobileunitsDetailLists.OrderBy(x => x.UnitId).ToList();// added 
            }
            catch (Exception ex)
            {
                //LoggerHelper.Instance.LogError(ex);
                logger.LogError(ex.ToString());
            }
            return await Task.FromResult(model);
        }

      
        public async Task<int> MobileValidateUnitIDAsync(APIRequest req)
        {
            int res = 0;
            MobileDeviceViewModel result = JsonConvert.DeserializeObject<MobileDeviceViewModel>(req.Model.ToString());
            try
            {

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
                //LoggerHelper.Instance.LogError(ex);
                logger.LogError(ex.ToString());

            }
            return await Task.FromResult<int>(res);
        }


        public async Task<string> MobileUpdateCardNumber(APIRequest request)
        {
            MobileDeviceViewModel? model = null;
            string result = "";
            try
            {
                logger.LogInformation("UpdateVisitorRegLocationMapping invoke started");
                model = JsonConvert.DeserializeObject<MobileDeviceViewModel>(request.Model.ToString());

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
                logger.LogError(ex.ToString());
                //logger.LogError(ex, "Error in UpdateVisitorRegLocationMapping method");
                //LoggerHelper.Instance.LogError(ex);
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
                // For other characters, you can define your own behavior.
            }

            return new string(charArray);
        }

    }
}

