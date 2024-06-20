using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ATTSystems.SFA.DAL.Implementation
{
    public class AdminPortalRepository : IBaseRepository, IDisposable, IAdminPortal
    {
        #region Standard Practice to create a Data Repo
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        Message msg = null;
        private readonly ILogger<AdminPortalRepository> _logger;
        public AdminPortalRepository(ILogger<AdminPortalRepository> logger)
        {
            entity = new DataContext();
            _logger = logger;
        }

        ~AdminPortalRepository()
        {
            Dispose(false);
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

        #region Santosh Wali

        #region Dashboard
        public string GetTerminalNAme(int? terminalid)
        {
            return entity.Terminals.FirstOrDefault(x => x.TerminalId == terminalid).TerminalName;

        }
        public async Task<RegistrationViewModel> GetDashboardListAsync(APIRequest req)
        {
            var modal = new RegistrationViewModel();
            //RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                _logger.LogInformation("Get dashboard list");
                DateTime dt = DateTime.Now;
                DateTime _ydt = dt.AddDays(-1);
                var entry_details = entity.VisitorTransactions.Where(x => x.EntryDateTime >= _ydt && x.EntryDateTime <= dt).OrderByDescending(x => x.EntryDateTime).ToList();

                if (entry_details.Count > 0)
                {
                    int ec = entry_details.Where(x => x.ExitDateTime == null).DistinctBy(x => x.NricOrPassport).Count();
                    int ec1 = entry_details.Where(x => x.ExitDateTime != null).Count();
                    // modal.EntryCount = ec1 + ec;
                    modal.EntryCount = entry_details.Where(x => !x.Flag).Count();
                    foreach (var item in entry_details.Where(x => !x.Flag))
                    {
                        string EntryTerminal = string.Empty;

                        if (item.EntryTerminalId != null)
                        {
                            EntryTerminal = GetTerminalNAme(item.EntryTerminalId);

                        }
                        string ExitTerminal = string.Empty;
                        if (item.ExitTerminalId != null)
                        {
                            ExitTerminal = GetTerminalNAme(item.ExitTerminalId);
                        }
                        string mentry = string.Empty;
                        if (item.EntryTerminalId == null && item.ManualCheckIn == 2)
                        {
                            EntryTerminal = "Manual entry";
                        }
                        string locationname = entity.Locations.FirstOrDefault(x => x.Id == item.LocationId).LocationName;
                        var visitordetails = entity.VisitorRegistrations.Where(x => x.NricOrPassport == item.NricOrPassport).ToList();
                        string visitorname = visitordetails[0].VisitorName;
                        string visitortype = entity.VisitTypes.FirstOrDefault(x => x.Id == visitordetails[0].VisitTypeId).VisitTypeName;
                        string entrydate = Convert.ToDateTime(item.EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");

                        string exitdate = string.Empty;
                        if (item.ExitDateTime != null)
                        {
                            exitdate = Convert.ToDateTime(item.ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        }
                        else
                        {
                            exitdate = "";
                        }
                        modal.EntryViewLists.Add(new RegistrationViewList
                        {
                            listVisitorName = visitorname,
                            listLocationName = locationname,
                            listVisitorTypeName = visitortype,
                            listentrydate = entrydate,
                            entrygate = EntryTerminal,
                            listexitdate = exitdate,
                            exitgate = ExitTerminal
                        });
                    }
                    //modal.LiveCount = entry_details.Where(x => x.ExitDateTime == null).Count();
                }
                else
                {
                    //modal.LiveCount = 0;
                    modal.EntryCount = 0;
                    modal.EntryViewLists = new List<RegistrationViewList>();
                }

                var live_details = entity.VisitorTransactions.Where(x => x.EntryDateTime >= _ydt && x.EntryDateTime <= dt && x.ExitDateTime == null).ToList();
                if (live_details.Count > 0)
                {
                    modal.LiveCount = live_details.Where(x => x.ExitDateTime == null && !x.Flag).DistinctBy(x => x.NricOrPassport).Count();
                }
                else
                {
                    modal.LiveCount = 0;
                }

                var stayover_details = entity.VisitorRegistrations.Include(x => x.Locations).Include(x => x.VisitType).Include(x => x.VisitorTransactions).ThenInclude(l => l.Location).Where(x => x.IsDeleted == false && x.OverStayer == true).ToList();

                if (stayover_details.Count > 0)
                {
                    foreach (var item in stayover_details)
                    {
                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        string locationname = string.Empty;
                        string exitdate = string.Empty;
                        string visitortype = item.VisitType.VisitTypeName;
                        var vt = item.VisitorTransactions.Where(x => x.ExitDateTime == null).ToList();
                        if (vt.Count > 0)
                        {
                            foreach (var transaction in vt)
                            {
                                var loc = transaction.Location;
                                locationname = loc.LocationName;
                            }
                            string entrydate = Convert.ToDateTime(item.VisitorTransactions.First(x => x.NricOrPassport == item.NricOrPassport).EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                            modal.StayoverViewLists.Add(new RegistrationViewList
                            {
                                listId = Convert.ToInt32(item.Id),
                                listVisitorName = item.VisitorName,
                                listLocationName = locationname,
                                listVisitorTypeName = visitortype,
                                listentrydate = entrydate,
                                listexitdate = exitdate,
                                listNricOrPassport = item.NricOrPassport,
                                listNricOrPassport2 = mask_data,
                                listContactNum = item.VisitorContanctNo,
                                listIdTypeId = item.IdType
                            });
                        }
                    }
                    modal.StayoverCount = modal.StayoverViewLists.DistinctBy(x => x.listNricOrPassport).Count();
                }
                else
                {
                    modal.StayoverCount = 0;
                    modal.StayoverViewLists = new List<RegistrationViewList>();
                }
                var exist_details = entity.VisitorTransactions.Where(x => x.ExitDateTime >= _ydt && x.EntryDateTime != null && x.ExitDateTime <= dt).OrderByDescending(x => x.ExitDateTime).ToList();
                if (exist_details.Count > 0)
                {
                    modal.ExitCount = exist_details.Where(x => !x.Flag).Count();
                    //foreach (var item in exist_details.Where(x => !x.Flag))
                    //{
                    //    string EntryTerminal = string.Empty;

                    //    if (item.EntryTerminalId != null)
                    //    {
                    //        EntryTerminal = GetTerminalNAme(item.EntryTerminalId);

                    //    }
                    //    string ExitTerminal = string.Empty;
                    //    if (item.ExitTerminalId != null)
                    //    {
                    //        ExitTerminal = GetTerminalNAme(item.ExitTerminalId);
                    //    }
                    //    string mentry = string.Empty;

                    //    string locationname = entity.Locations.FirstOrDefault(x => x.Id == item.LocationId).LocationName;
                    //    var visitordetails = entity.VisitorRegistrations.Where(x => x.NricOrPassport == item.NricOrPassport).ToList();
                    //    string visitorname = visitordetails[0].VisitorName;
                    //    string visitortype = entity.VisitTypes.FirstOrDefault(x => x.Id == visitordetails[0].VisitTypeId).VisitTypeName;
                    //    // string entrydate = Convert.ToDateTime(item.EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");

                    //    string exitdate = string.Empty;
                    //    if (item.ExitDateTime != null)
                    //    {
                    //        exitdate = Convert.ToDateTime(item.ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                    //    }
                    //    else
                    //    {
                    //        exitdate = "";
                    //    }
                    //    modal.EntryViewLists.Add(new RegistrationViewList
                    //    {
                    //        listVisitorName = visitorname,
                    //        listLocationName = locationname,
                    //        listVisitorTypeName = visitortype,
                    //        listexitdate = exitdate,
                    //        exitgate = ExitTerminal
                    //    });
                    //}
                }
                else
                {
                    modal.ExitCount = 0;
                }
            }
            catch (Exception ex)
            {
                //LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Get dashboard list is getting error");
            }
            return await Task.FromResult(modal);
        }
        //overstayer Export 
        public async Task<RegistrationViewModel> GetoverstayerToExcelAsync(APIRequest req)
        {
            RegistrationViewModel model = new RegistrationViewModel();
            int locationId = Convert.ToInt32(req.RequestString);
            var over = entity.VisitorRegistrations.Include(x => x.VisitType).Include(v => v.VisitorTransactions).ThenInclude(l => l.Location).Where(x => x.IsDeleted == false && x.OverStayer == true).ToList();
            var loid = locationId;
            if (over != null)
            {
                foreach (var item in over)
                {
                    var vt = item.VisitorTransactions.Where(x => x.ExitDateTime == null).ToList();
                    if (vt.Count > 0)
                    {
                        Location lc = new Location();
                        foreach (var transaction in vt)
                        {
                            lc = transaction.Location;
                        }
                        if (lc.Id == locationId)
                        {
                            string entrydate = Convert.ToDateTime(item.VisitorTransactions.First(x => x.NricOrPassport == item.NricOrPassport).EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                            string exitdate = Convert.ToDateTime(item.VisitorTransactions.First(x => x.NricOrPassport == item.NricOrPassport).ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");

                            model.RegistrationViewLists.Add(new RegistrationViewList
                            {
                                listLocationId = locationId,
                                listVisitorName = item.VisitorName,
                                listNricOrPassport = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport),
                                listContactNum = item.VisitorContanctNo,
                                listentrydate = entrydate,
                                listexitdate = exitdate,
                                listVisitorTypeName = item.VisitType.VisitTypeName,
                                listCompanyName = item.CompanyName,
                                listUnitNo = item.UnitNo,
                                listVehicleNo = item.VehicleNo,
                                listVisitorEmail = item.Email,
                                listLocationName = lc.LocationName
                            });
                        }

                    }
                }

            }
            return await Task.FromResult(model);
        }

        //Visitor Reaction
        public async Task<int> ReactivateVisitorAsync(APIRequest req)
        {
            int res = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString() ?? string.Empty);
            try
            {
                if (result != null)
                {
                    _logger.LogInformation("Updating Overstayer status in Vigitor Registration table");

                    VisitorRegistration? vreg = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == result.NricOrPassport && x.IsDeleted == false);
                    if (vreg != null)
                    {
                        vreg.EnabledOverStayer = true;
                        vreg.OverStayer = false;
                        vreg.IsEnabled = true; //modified as per sanju suggestion on 22-02-2024
                        entity.SaveChanges();
                    }

                    _logger.LogInformation("Reactivate visitors in Visitor transanction");
                    VisitorTransaction? visit_trans = entity.VisitorTransactions.FirstOrDefault(x => x.NricOrPassport == result.NricOrPassport && x.ExitDateTime == null);
                    if (visit_trans != null)
                    {
                        visit_trans.ExitDateTime = DateTime.Now;
                        visit_trans.ReactivatedBy = req.RequestString;
                        visit_trans.ReactivatedDateTime = DateTime.Now;
                        visit_trans.ReasonToReactivate = result.ReasonToReactivate;
                        entity.SaveChanges();
                        res = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                res = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Reactivate visitors is getting error");
            }
            return await Task.FromResult<int>(res);
        }
        #endregion

        #region Registration

        // get visitor registration list
        public async Task<RegistrationViewModel> GetRegistrationListAsync(APIRequest req)
        {
            //var RegList = new RegistrationViewModel();
            _logger.LogInformation("Getting registration list");
            int page = Convert.ToInt32(req.RequestString);
            int skip = Convert.ToInt32(req.RequestString);
            RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                var vreg = entity.Set<VisitorRegistration>().AsQueryable();
                List<VisitorRegistration> result = vreg.AsNoTracking().Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == false).OrderByDescending(x => x.CreateDateTime).ToList();//OrderByDescending(x => x.CreateDateTime).Take(100).

                //List<VisitorRegistration> result =await entity.VisitorRegistrations.AsNoTracking().Include(x=>x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == false).ToListAsync();//OrderByDescending(x => x.CreateDateTime).Take(100).
                if (result != null)
                {
                    model.RegistrationViewLists = new List<RegistrationViewList>();
                    foreach (var item in result)
                    {
                        var result_loc = item.Locations;
                        // var result_loc = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.Id == item.Id);
                        var existloc_ids = result_loc.Select(y => y.Id).ToList();
                        StringBuilder sb = new StringBuilder();
                        foreach (var locname in existloc_ids)
                        {
                            sb.Append(entity.Locations.FirstOrDefault(x => x.Id == locname).LocationName);
                            sb.Append(',');
                        }
                        var vtypename = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;

                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        model.RegistrationViewLists.Add(new RegistrationViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listVisitorName = item.VisitorName,
                            listVisitorEmail = item.Email,
                            listLocationName = sb.ToString().Trim(','), //entity.Locations.FirstOrDefault(x => x.Id == existloc_ids[0]).LocationName,
                            listIdTypeName = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name,
                            listNricOrPassport = mask_data,
                            listNricOrPassport2 = decryptedData,
                            listOriginalNricOrPassport = item.NricOrPassport,
                            listVehicleNo = item.VehicleNo,
                            listVisitorTypeName = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName,
                            listentrydate = Convert.ToDateTime(item.VistStartDateTime).ToString("dd/MM/yyyy HH:mm:ss tt")
                        });
                    }
                }

                var Unitslist = await entity.UnitDetails.AsNoTracking().ToListAsync();
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
                model.UnitsDetailLists = model.UnitsDetailLists.OrderBy(x => x.UnitId).ToList();

            }
            catch (Exception ex)
            {
                //LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Registration list is getting error");
            }
            return await Task.FromResult(model);
        }

        //Check duplicate registration NricOrPassport
        public async Task<RegistrationViewModel> GetNricOrPassportNotExistenceAsync(APIRequest req)
        {
            RegistrationViewModel viewModel = new RegistrationViewModel();
            _logger.LogInformation("Get NRIC/Passport not existencey");
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                if (result.RegistrationViewLists != null)
                {
                    foreach (var item in result.RegistrationViewLists)
                    {
                        string dataToEncrypt = item.listNricOrPassport.ToUpper();

                        string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                        VisitorRegistration? result1 = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);

                        if (result1 == null)
                        {
                            viewModel.RegistrationViewLists.Add(new RegistrationViewList
                            {
                                listVisitorTypeId = item.listVisitorTypeId,
                                listLocationName = item.listLocationName,
                                listIdTypeName = item.listIdTypeName,
                                listNricOrPassport = item.listNricOrPassport.ToUpper(),
                                listVisitorName = item.listVisitorName,
                                listVisitorEmail = item.listVisitorEmail,
                                listVehicleNo = item.listVehicleNo,
                                listContactNum = item.listContactNum,
                                listUnitNo = item.listUnitNo,
                                listCompanyName = item.listCompanyName,
                                duplicateNricOrPassport = false
                            });
                        }
                        else if (result1 != null || result1.Locations.Count > 0)
                        {
                            if (result1.Locations.Count == 1)
                            {
                                //check the existing location for visitor and match with current visitor
                                int get_locid = entity.Locations.FirstOrDefault(x => x.LocationName == item.listLocationName).Id;
                                var existloc_ids = result1.Locations.Select(y => y.Id).ToList();
                                if (!existloc_ids.Contains(get_locid))
                                {
                                    viewModel.RegistrationViewLists.Add(new RegistrationViewList
                                    {
                                        listVisitorTypeId = item.listVisitorTypeId,
                                        listLocationName = item.listLocationName,
                                        listIdTypeName = item.listIdTypeName,
                                        listNricOrPassport = item.listNricOrPassport.ToUpper(),
                                        listVisitorName = item.listVisitorName,
                                        listVisitorEmail = item.listVisitorEmail,
                                        listVehicleNo = item.listVehicleNo,
                                        listContactNum = item.listContactNum,
                                        listUnitNo = item.listUnitNo,
                                        listCompanyName = item.listCompanyName,
                                        duplicateNricOrPassport = false
                                    });
                                }
                                else
                                {
                                    viewModel.RegistrationViewLists.Add(new RegistrationViewList
                                    {
                                        listVisitorTypeId = item.listVisitorTypeId,
                                        listLocationName = item.listLocationName,
                                        listIdTypeName = item.listIdTypeName,
                                        listNricOrPassport = item.listNricOrPassport.ToUpper(),
                                        listVisitorName = item.listVisitorName,
                                        listVisitorEmail = item.listVisitorEmail,
                                        listVehicleNo = item.listVehicleNo,
                                        listContactNum = item.listContactNum,
                                        listUnitNo = item.listUnitNo,
                                        listCompanyName = item.listCompanyName,
                                        duplicateNricOrPassport = true
                                    });
                                }
                            }
                            else if (result1.Locations.Count == 2)
                            {
                                viewModel.RegistrationViewLists.Add(new RegistrationViewList
                                {
                                    listVisitorTypeId = item.listVisitorTypeId,
                                    listLocationName = item.listLocationName,
                                    listIdTypeName = item.listIdTypeName,
                                    listNricOrPassport = item.listNricOrPassport.ToUpper(),
                                    listVisitorName = item.listVisitorName,
                                    listVisitorEmail = item.listVisitorEmail,
                                    listVehicleNo = item.listVehicleNo,
                                    listContactNum = item.listContactNum,
                                    listUnitNo = item.listUnitNo,
                                    listCompanyName = item.listCompanyName,
                                    duplicateNricOrPassport = true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Get NRIC/Passport not existencey");
            }
            return await Task.FromResult(viewModel);
        }

        //Save Visitor Details Excel Data
        public async Task<int> SaveBatchExcelFiles(APIRequest req)
        {
            int res1 = 0;
            RegistrationViewModel viewModel = new RegistrationViewModel();
            _logger.LogInformation("Save excel files");
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                if (result.RegistrationViewLists != null)
                {
                    foreach (var item in result.RegistrationViewLists)
                    {
                        try
                        {
                            string dataToEncrypt = item.listNricOrPassport.ToUpper();
                            string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                            VisitorRegistration? visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);
                            if (visitorRegistration != null)
                            {
                                if (visitorRegistration.IsDeleted == true)
                                {
                                    if (visitorRegistration.IdType == 1)
                                    {
                                        visitorRegistration.IsDeleted = false;
                                        visitorRegistration.VistStartDateTime = DateTime.Now;
                                        visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                                        entity.SaveChanges();
                                    }
                                    else if (visitorRegistration.IdType == 2)
                                    {
                                        visitorRegistration.IsDeleted = false;
                                        visitorRegistration.VistStartDateTime = DateTime.Now;
                                        visitorRegistration.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();
                                    }
                                }
                            }

                            VisitorRegistration? result1 = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);
                            if (result1 == null || result1.Locations.Count == 0)
                            {
                                VisitorRegistration res = new VisitorRegistration();
                                var loc = entity.Locations.FirstOrDefault(x => x.LocationName == item.listLocationName).Id;
                                var idtyp = entity.VisitorIdentities.FirstOrDefault(x => x.Name == item.listIdTypeName).Id;
                                DateTime dt = DateTime.Now;
                                if (idtyp == 1)
                                {
                                    dt = DateTime.Now.AddHours(24);
                                }
                                if (idtyp == 2)
                                {
                                    dt = DateTime.Now.AddYears(1);
                                }

                                res.IdType = idtyp;
                                res.VisitorName = item.listVisitorName;
                                res.NricOrPassport = encData;
                                res.VehicleNo = item.listVehicleNo;
                                res.CompanyName = item.listCompanyName;
                                res.VisitTypeId = item.listVisitorTypeId;
                                res.CreateDateTime = DateTime.Now;
                                res.CreateBy = req.RequestString;
                                res.VisitorStatus = 1;
                                res.Email = item.listVisitorEmail;
                                res.UnitNo = item.listUnitNo;
                                res.VisitorContanctNo = item.listContactNum;
                                res.IsBlockListed = false;
                                res.VistStartDateTime = DateTime.Now;
                                res.VistEndDateTime = dt;
                                res.UploadtoController = 1;
                                res.ManualCheckIn = 0;
                                res.RegistrationBy = "AdminPortal";
                                res.IsEnabled = false;
                                res.EnabledOverStayer = false;
                                res.PushVisitors = false;
                                res.IsDisabled = false;

                                entity.VisitorRegistrations.Add(res);
                                entity.SaveChanges();

                                VisitorRegistration? vregister = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);

                                if (vregister != null)
                                {
                                    UserViewModel locationView = new UserViewModel();

                                    var lList = new List<string>();
                                    lList.Add(loc.ToString());

                                    locationView.RoleList = new List<RoleViewModel>();
                                    foreach (string lId in lList)
                                    {
                                        int locId = 0;
                                        if (int.TryParse(lId, out locId))
                                        {
                                            locationView.RoleList.Add(new RoleViewModel { Id = locId, });
                                        }
                                    }

                                    var locationIdList = locationView.RoleList.Select(x => x.Id);

                                    var locationList = entity.Locations.Where(x => locationIdList.Contains(x.Id));

                                    if (locationList != null)
                                    {
                                        foreach (Location _location in locationList)
                                        {
                                            vregister.Locations.Add(_location);
                                        }
                                    }
                                    entity.SaveChanges();
                                }

                                // ====== Update Card Issue Details for IC
                                if (idtyp == 2)
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
                                // ====== Update Card Issue Details for Passport
                                if (idtyp == 1)
                                {
                                    Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                    int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                    string cardNumber = string.Empty;
                                    cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                    if (dbCardNum >= 99999)
                                    {
                                        seetingValue.Field = RotateCharacters(seetingValue.Field);
                                        //== Reset settings count value
                                        cardNumber = "00001";
                                        dbCardNum = int.Parse(cardNumber);
                                        dbCardNum++; // Increment the integer value
                                        string newValue = dbCardNum.ToString("00000");
                                        seetingValue.Value = newValue;
                                        cardNumber = "A" + req.RequestString + cardNumber + seetingValue.Field;
                                    }
                                    else
                                    {
                                        dbCardNum++; // Increment the integer value
                                        string newValue = dbCardNum.ToString("00000");
                                        seetingValue.Value = newValue;

                                    }
                                    string encDataPass = string.Empty;
                                    encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);
                                    CardIssueDetail cardDetails = new CardIssueDetail
                                    {
                                        CardNumber = encDataPass,
                                        IsActive = true,
                                        CreateDateTime = DateTime.Now,
                                        IssueDate = DateTime.Now,
                                        NricOrPassport = encData
                                    };
                                    entity.CardIssueDetails.Add(cardDetails);
                                    entity.SaveChanges();

                                    msg = new Message(_logger);
                                    string messageTemplate = msg.GetMessageString(entity);
                                    msg.InsertMessage(cardNumber, item.listVisitorEmail, entity);
                                }

                                res1 = 1;
                            }

                            else
                            {
                                var idtyp = entity.VisitorIdentities.FirstOrDefault(x => x.Name == item.listIdTypeName).Id;
                                int get_locid = entity.Locations.FirstOrDefault(x => x.LocationName == item.listLocationName).Id;
                                var existloc_ids = result1.Locations.Select(y => y.Id).ToList();
                                if (!existloc_ids.Contains(get_locid))
                                {
                                    DateTime dt = DateTime.Now;
                                    if (idtyp == 1)
                                    {
                                        if (result1.VistEndDateTime < dt)
                                        {
                                            dt = DateTime.Now.AddHours(24);
                                            result1.VistStartDateTime = DateTime.Now;
                                            result1.VistEndDateTime = dt;
                                            entity.SaveChanges();
                                        }
                                    }
                                    if (idtyp == 2)
                                    {
                                        if (result1.VistEndDateTime < dt)
                                        {
                                            dt = DateTime.Now.AddYears(1);
                                            result1.VistStartDateTime = DateTime.Now;
                                            result1.VistEndDateTime = dt;
                                            entity.SaveChanges();
                                        }
                                    }
                                    UserViewModel locationView = new UserViewModel();
                                    var lList = new List<string>();
                                    lList.Add(get_locid.ToString());
                                    locationView.RoleList = new List<RoleViewModel>();
                                    foreach (string lId in lList)
                                    {
                                        int locId = 0;
                                        if (int.TryParse(lId, out locId))
                                        {
                                            locationView.RoleList.Add(new RoleViewModel { Id = locId, });
                                        }
                                    }
                                    var locationIdList = locationView.RoleList.Select(x => x.Id);
                                    var locationList = entity.Locations.Where(x => locationIdList.Contains(x.Id));
                                    if (locationList != null)
                                    {
                                        foreach (Location _location in locationList)
                                        {
                                            result1.Locations.Add(_location);
                                        }
                                    }
                                    entity.SaveChanges();

                                    // ====== Update Card Issue Details for IC
                                    if (idtyp == 2)
                                    {
                                        if (result1.VistEndDateTime < DateTime.Now)
                                        {
                                            CardIssueDetail? checkexistsornot = entity.CardIssueDetails.FirstOrDefault(c => c.NricOrPassport == encData);
                                            if (checkexistsornot != null)
                                            {
                                                checkexistsornot.IssueDate = DateTime.Now;
                                                checkexistsornot.IsActive = true;
                                                entity.SaveChanges();
                                            }
                                        }
                                    }
                                    // ====== Update Card Issue Details for Passport
                                    if (idtyp == 1)
                                    {
                                        Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                        int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                        string cardNumber = string.Empty;
                                        cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                        if (dbCardNum >= 99999)
                                        {
                                            seetingValue.Field = RotateCharacters(seetingValue.Field);
                                            //== Reset settings count value
                                            cardNumber = "00001";
                                            dbCardNum = int.Parse(cardNumber);
                                            dbCardNum++; // Increment the integer value
                                            string newValue = dbCardNum.ToString("00000");
                                            seetingValue.Value = newValue;
                                            cardNumber = "A" + req.RequestString + cardNumber + seetingValue.Field;
                                        }
                                        else
                                        {
                                            dbCardNum++; // Increment the integer value
                                            string newValue = dbCardNum.ToString("00000");
                                            seetingValue.Value = newValue;

                                        }
                                        string encDataPass = string.Empty;
                                        encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);

                                        CardIssueDetail? checkexpiry = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData && x.IsActive == true);
                                        if (checkexpiry == null)
                                        {
                                            CardIssueDetail cardDetails = new CardIssueDetail
                                            {
                                                CardNumber = encDataPass,
                                                IsActive = true,
                                                CreateDateTime = DateTime.Now,
                                                IssueDate = DateTime.Now,
                                                NricOrPassport = encData
                                            };
                                            entity.CardIssueDetails.Add(cardDetails);
                                            entity.SaveChanges();

                                            msg = new Message(_logger);
                                            string messageTemplate = msg.GetMessageString(entity);
                                            msg.InsertMessage(cardNumber, item.listVisitorEmail, entity);
                                        }
                                    }
                                    res1 = 1;
                                }
                                else
                                {
                                    DateTime dt = DateTime.Now;
                                    if (idtyp == 1)
                                    {
                                        if (result1.VistEndDateTime < dt)
                                        {
                                            dt = DateTime.Now.AddHours(24);
                                            result1.VistStartDateTime = DateTime.Now;
                                            result1.VistEndDateTime = dt;
                                            entity.SaveChanges();

                                            Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                            int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                            string cardNumber = string.Empty;
                                            cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                            if (dbCardNum >= 99999)
                                            {
                                                seetingValue.Field = RotateCharacters(seetingValue.Field);
                                                //== Reset settings count value
                                                cardNumber = "00001";
                                                dbCardNum = int.Parse(cardNumber);
                                                dbCardNum++; // Increment the integer value
                                                string newValue = dbCardNum.ToString("00000");
                                                seetingValue.Value = newValue;
                                                cardNumber = "A" + req.RequestString + cardNumber + seetingValue.Field;
                                            }
                                            else
                                            {
                                                dbCardNum++; // Increment the integer value
                                                string newValue = dbCardNum.ToString("00000");
                                                seetingValue.Value = newValue;
                                            }

                                            string encDataPass = string.Empty;
                                            encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);

                                            CardIssueDetail? checkexpiry = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData && x.IsActive == true);
                                            if (checkexpiry == null)
                                            {
                                                CardIssueDetail cardDetails = new CardIssueDetail
                                                {
                                                    CardNumber = encDataPass,
                                                    IsActive = true,
                                                    CreateDateTime = DateTime.Now,
                                                    IssueDate = DateTime.Now,
                                                    NricOrPassport = encData
                                                };
                                                entity.CardIssueDetails.Add(cardDetails);
                                                entity.SaveChanges();

                                                msg = new Message(_logger);
                                                string messageTemplate = msg.GetMessageString(entity);
                                                msg.InsertMessage(cardNumber, item.listVisitorEmail, entity);
                                            }
                                        }
                                    }
                                    if (idtyp == 2)
                                    {
                                        if (result1.VistEndDateTime < dt)
                                        {
                                            dt = DateTime.Now.AddYears(1);
                                            result1.VistStartDateTime = DateTime.Now;
                                            result1.VistEndDateTime = dt;
                                            entity.SaveChanges();

                                            CardIssueDetail? checkexistsornot = entity.CardIssueDetails.FirstOrDefault(c => c.NricOrPassport == encData);
                                            if (checkexistsornot != null)
                                            {
                                                checkexistsornot.IssueDate = DateTime.Now;
                                                checkexistsornot.IsActive = true;
                                                entity.SaveChanges();
                                            }
                                        }
                                    }
                                    res1 = 1;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Instance.LogError(ex);
                            _logger.LogError(ex, " Excel Bulk registration save-- error while adding large data");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res1 = 0;
                LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Save excel files is getting error");
            }
            return await Task.FromResult(res1);
        }

        #endregion

        #region ManualCheckIn

        public async Task<RegistrationViewModel> GetManualCheckInListAsync(APIRequest req)
        {
            var RegList = new RegistrationViewModel();
            _logger.LogInformation("Get manual checkin list");
            try
            {
                //  var result = entity.VisitorRegistrations.Where(x => x.IsDeleted == false && x.IsBlockListed == false && x.ManualCheckIn == 1).OrderByDescending(x => x.CreateDateTime).ToList();
                var result = entity.VisitorRegistrations.AsNoTracking().Include(x => x.Locations).AsNoTracking().Where(x => x.IsDeleted == false && x.IsBlockListed == false && x.ManualCheckIn == 1).OrderByDescending(x => x.CreateDateTime).ToList();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        var result_loc = item.Locations.ToList();

                        var vtypename = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;

                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);

                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);
                        foreach (var loc in result_loc)
                        {
                            RegList.RegistrationViewLists.Add(new RegistrationViewList
                            {
                                listId = Convert.ToInt32(item.Id),
                                listVisitorName = item.VisitorName,
                                listVisitorEmail = item.Email,
                                listLocationName = loc.LocationName,
                                listIdTypeName = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name,
                                listNricOrPassport = mask_data,
                                listNricOrPassport2 = decryptedData,
                                listVehicleNo = item.VehicleNo,
                                listVisitorTypeName = vtypename
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
                _logger.LogError("manual checkin list is getting error");
            }
            return await Task.FromResult(RegList);
        }

        //Manual CheckIn Method
        public async Task<int> ManualCheckInSaveAsync(APIRequest req)
        {
            int res1 = 0;
            RegistrationViewModel result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                _logger.LogInformation("Manual chekin save");
                if (result != null)
                {
                    string encData = EncryptionDecryptionSHA256.Encrypt(result.NricOrPassport);
                    var checkexists = entity.VisitorTransactions.Where(x => x.NricOrPassport == encData && x.TransactionDateTime.Value.Date == DateTime.Now.Date).AsNoTracking().ToList();
                    if (checkexists == null || checkexists.Count == 0)
                    {
                        VisitorTransaction res = new VisitorTransaction();
                        var loc = entity.Locations.FirstOrDefault(x => x.LocationName == result.LocationName).Id;

                        res.VisitorRegistrationId = result.Id;
                        res.NricOrPassport = encData;
                        res.TransactionDateTime = DateTime.Now;
                        res.TransactionType = 1;
                        res.LocationId = loc;
                        res.EntryDateTime = DateTime.Now;
                        res.EntryPushed = false;
                        res.ExitPushed = false;
                        res.IsDisabled = false;
                        res.ManualCheckIn = 2;
                        res.ManualCheckInBy = req.RequestString;
                        entity.VisitorTransactions.Add(res);
                        entity.SaveChanges();

                        VisitorRegistration? vreg = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);
                        vreg.ManualCheckIn = 2;
                        vreg.ManualCheckInBy = req.RequestString;
                        entity.SaveChanges();

                        res1 = 1;
                    }
                    else
                    {
                        res1 = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                res1 = 0;
                //LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Manual chekin save");
            }
            return await Task.FromResult(res1);
        }

        #endregion

        #region BlackList

        public async Task<RegistrationViewModel> GetGetBlackListAsync(APIRequest req)
        {
            var RegList = new RegistrationViewModel();
            RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                _logger.LogInformation("Get black list");
                var result = await entity.VisitorRegistrations.Include(x => x.Locations).AsNoTracking().Where(x => x.IsDeleted == false).OrderByDescending(x => x.CreateDateTime).ToListAsync();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        // var result_loc = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.Id == item.Id);
                        var result_loc = item.Locations;
                        var existloc_ids = result_loc.Select(y => y.Id).ToList();

                        StringBuilder sb = new StringBuilder();
                        foreach (var locname in existloc_ids)
                        {
                            sb.Append(entity.Locations.FirstOrDefault(x => x.Id == locname).LocationName);
                            sb.Append(',');
                        }

                        var vtypename = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;

                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        RegList.RegistrationViewLists.Add(new RegistrationViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listVisitorName = item.VisitorName,
                            listVisitorEmail = item.Email,
                            listLocationName = sb.ToString().TrimEnd(','), /*entity.Locations.FirstOrDefault(x => x.Id == existloc_ids[0]).LocationName,*/
                            listIdTypeName = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name,
                            listNricOrPassport = mask_data,
                            listVehicleNo = item.VehicleNo,
                            listVisitorTypeName = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName,
                            listentrydate = Convert.ToDateTime(item.VistStartDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"),
                            listexitdate = Convert.ToDateTime(item.VistEndDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"),
                            duplicateNricOrPassport = item.IsBlockListed
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Get black list is getting error");
            }
            return await Task.FromResult(RegList);
        }

        //Update blicklist through trigger
        public async Task<int> blacklist_Trigger(APIRequest req)
        {
            int res = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                _logger.LogInformation("Blacklist Trigger");
                VisitorRegistration? site = entity.VisitorRegistrations.FirstOrDefault(x => x.Id == Convert.ToInt64(result.Id));
                if (site != null)
                {
                    if (site.IsBlockListed == false)
                    {
                        site.IsBlockListed = true;
                        site.BlacklistBy = req.RequestString;
                        site.BlacklistDateTime = DateTime.Now;
                        site.ReasonForBlacklist = result.ReasonToBlacklist;
                        entity.SaveChanges();

                        BlacklistTransaction blacklistTransaction = new BlacklistTransaction();
                        blacklistTransaction.NricOrPassport = site.NricOrPassport;
                        blacklistTransaction.BlacklistBy = req.RequestString;
                        blacklistTransaction.BlacklistDateTime = DateTime.Now;
                        blacklistTransaction.BlacklistReason = result.ReasonToBlacklist;
                        blacklistTransaction.IsBlacklisted = true;
                        blacklistTransaction.BlacklistResult = "Added to Blacklist";
                        entity.BlacklistTransactions.Add(blacklistTransaction);
                        entity.SaveChanges();
                        res = 1;
                    }
                    else if (site.IsBlockListed == true)
                    {
                        site.IsBlockListed = false;
                        site.UploadtoController = 1;
                        site.BlacklistBy = req.RequestString;
                        site.BlacklistDateTime = DateTime.Now;
                        site.ReasonForBlacklist = result.ReasonToBlacklist;
                        site.IsDisabled = false;
                        entity.SaveChanges();

                        BlacklistTransaction blacklistTransaction = new BlacklistTransaction();
                        blacklistTransaction.NricOrPassport = site.NricOrPassport;
                        blacklistTransaction.BlacklistBy = req.RequestString;
                        blacklistTransaction.BlacklistDateTime = DateTime.Now;
                        blacklistTransaction.BlacklistReason = result.ReasonToBlacklist;
                        blacklistTransaction.IsBlacklisted = false;
                        blacklistTransaction.BlacklistResult = "Removed from Blacklist";
                        entity.BlacklistTransactions.Add(blacklistTransaction);
                        entity.SaveChanges();
                        res = 1;
                    }
                    entity.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                res = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Blacklist Trigger is getting error");
            }
            return await Task.FromResult<int>(res);
        }

        //blacklist Export excel
        public async Task<RegistrationViewModel> GetExportBlackListAsync(APIRequest req)
        {
            var RegList = new RegistrationViewModel();
            RegistrationViewModel model = new RegistrationViewModel();
            try
            {
                _logger.LogInformation("Get black list");
                var result = await entity.VisitorRegistrations.Include(x => x.Locations).AsNoTracking().Where(x => x.IsDeleted == false && x.IsBlockListed == true).OrderByDescending(x => x.CreateDateTime).ToListAsync();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        // var result_loc = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.Id == item.Id);
                        var result_loc = item.Locations;
                        var existloc_ids = result_loc.Select(y => y.Id).ToList();

                        StringBuilder sb = new StringBuilder();
                        foreach (var locname in existloc_ids)
                        {
                            sb.Append(entity.Locations.FirstOrDefault(x => x.Id == locname).LocationName);
                            sb.Append(',');
                        }

                        var vtypename = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;

                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        RegList.RegistrationViewLists.Add(new RegistrationViewList
                        {
                            listId = Convert.ToInt32(item.Id),
                            listVisitorName = item.VisitorName,
                            listVisitorEmail = item.Email,
                            listContactNum = item.VisitorContanctNo,
                            listLocationName = sb.ToString().TrimEnd(','), /*entity.Locations.FirstOrDefault(x => x.Id == existloc_ids[0]).LocationName,*/
                            listIdTypeName = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name,
                            listNricOrPassport = mask_data,
                            listVehicleNo = item.VehicleNo,
                            listVisitorTypeName = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName,
                            listentrydate = Convert.ToDateTime(item.VistStartDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"),
                            listexitdate = Convert.ToDateTime(item.VistEndDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"),
                            duplicateNricOrPassport = item.IsBlockListed,
                            listblacklistdate = item.BlacklistDateTime.ToString(),


                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Get black list is getting error");
            }
            return await Task.FromResult(RegList);
        }


        #endregion

        #endregion

        #region Rakesh
        public async Task<List<Location>> GetsingleregLocationAsync(APIRequest request)
        {
            List<Location>? result = null;
            _logger.LogInformation("Get location list");
            using (var entity = new DataContext())
            {
                try
                {
                    var singleloclist = entity.Locations.Where(x => x.IsDeleted == false);
                    if (singleloclist != null)
                    {
                        result = singleloclist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = string.Format("System internal Error. \n{0}", ex.InnerException);
                    _logger.LogError(ex, "Get location list is getting error");
                }
            }
            return await Task.FromResult<List<Location>>(result);
        }

        public async Task<List<VisitType>> GetSingleVisitorTypeAsync(APIRequest request)
        {
            List<VisitType>? result = null;
            _logger.LogInformation("Getting visitor types");
            using (var entity = new DataContext())
            {
                try
                {
                    var singlevisitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                    if (singlevisitTypeList != null)
                    {
                        result = singlevisitTypeList.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = string.Format("System internal Error. \n{0}", ex.InnerException);
                    _logger.LogError(ex, "Visitor types is getting error");
                }
            }
            return await Task.FromResult<List<VisitType>>(result);
        }

        public async Task<List<VisitorIdentity>> GetSinglevstidentityAsync(APIRequest request)
        {
            List<VisitorIdentity>? result = null;
            _logger.LogInformation("Get Visitor identity types");
            using (var entity = new DataContext())
            {
                try
                {
                    var singlevstidentityList = entity.VisitorIdentities.Where(x => x.IsDeleted == false);
                    if (singlevstidentityList != null)
                    {
                        result = singlevstidentityList.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = string.Format("System internal Error. \n{0}", ex.InnerException);
                    _logger.LogError(ex, "Visitor identity types is getting error");
                }
            }
            return await Task.FromResult<List<VisitorIdentity>>(result);
        }

        public async Task<int> SingleregsaveAsync(APIRequest request)
        {
            int singleregid = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(request.Model.ToString());
            using (var entity = new DataContext())
            {
                _logger.LogInformation("Single registration save");
                try
                {
                    if (result != null)
                    {
                        string dataToEncrypt = result.NricOrPassport.ToUpper();
                        string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);

                        VisitorRegistration? visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);
                        if (visitorRegistration != null)
                        {
                            if (visitorRegistration.IsDeleted == true)
                            {
                                if (visitorRegistration.IdType == 1)
                                {
                                    visitorRegistration.IsDeleted = false;
                                    visitorRegistration.VistStartDateTime = DateTime.Now;
                                    visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                                    entity.SaveChanges();
                                }
                                else if (visitorRegistration.IdType == 2)
                                {
                                    visitorRegistration.IsDeleted = false;
                                    visitorRegistration.VistStartDateTime = DateTime.Now;
                                    visitorRegistration.VistEndDateTime = DateTime.Now.AddYears(1);
                                    entity.SaveChanges();
                                }
                            }
                        }

                        var list = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData && x.IsDeleted == false);

                        if (list == null || list.Locations.Count == 0)
                        {
                            VisitorRegistration nric = new VisitorRegistration();
                            var loc = entity.Locations.FirstOrDefault(x => x.LocationName == result.LocationName).Id;
                            var idtyp = entity.VisitorIdentities.FirstOrDefault(x => x.Name == result.IdTypeName).Id;
                            var vstid = entity.VisitTypes.FirstOrDefault(x => x.VisitTypeName == result.VisitorTypeName && x.IsDeleted == false).Id;
                            DateTime dt = DateTime.Now;
                            if (idtyp == 1)
                            {
                                dt = DateTime.Now.AddHours(24);
                            }
                            if (idtyp == 2)
                            {
                                dt = DateTime.Now.AddYears(1);
                            }

                            nric.IdType = idtyp;
                            nric.VisitorName = result.VisitorName;
                            nric.NricOrPassport = encData;
                            nric.VehicleNo = result.VehicleNo;
                            nric.CompanyName = result.CompanyName;
                            nric.VisitTypeId = vstid;
                            nric.VisitorContanctNo = result.ContactNum;
                            nric.CreateDateTime = DateTime.Now;
                            nric.CreateBy = request.UserName;
                            nric.VisitorStatus = 1;
                            nric.Email = result.VisitorEmail;
                            nric.UnitNo = result.UnitNo;
                            nric.IsBlockListed = false;
                            nric.VistStartDateTime = DateTime.Now;
                            nric.VistEndDateTime = dt;
                            nric.ManualCheckIn = 0;
                            nric.UploadtoController = 1;
                            nric.RegistrationBy = "AdminPortal";
                            nric.IsEnabled = false;
                            nric.EnabledOverStayer = false;
                            nric.PushVisitors = false;
                            nric.IsDisabled = false;

                            entity.VisitorRegistrations.Add(nric);
                            entity.SaveChanges();

                            result.NricOrPassport = nric.NricOrPassport;
                            VisitorRegistration? nric1 = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == result.NricOrPassport);

                            if (nric1 != null)
                            {
                                UserViewModel locationView = new UserViewModel();

                                var lList = new List<string>();
                                lList.Add(loc.ToString());

                                locationView.RoleList = new List<RoleViewModel>();
                                foreach (string lId in lList)
                                {
                                    int locId = 0;
                                    if (int.TryParse(lId, out locId))
                                    {
                                        locationView.RoleList.Add(new RoleViewModel { Id = locId, });
                                    }
                                }

                                var LocationIdList = locationView.RoleList.Select(x => x.Id);

                                var location_List = entity.Locations.Where(x => LocationIdList.Contains(x.Id));

                                if (location_List != null)
                                {
                                    foreach (Location _loc in location_List)
                                    {
                                        nric1.Locations.Add(_loc);
                                    }
                                }
                                entity.SaveChanges();
                            }

                            // ====== Update Card Issue Details for IC
                            if (idtyp == 2)
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
                            // ====== Update Card Issue Details for Passport
                            if (idtyp == 1)
                            {
                                Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                string cardNumber = string.Empty;
                                cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                if (dbCardNum >= 99999)
                                {
                                    seetingValue.Field = RotateCharacters(seetingValue.Field);
                                    //== Reset settings count value
                                    cardNumber = "00001";
                                    dbCardNum = int.Parse(cardNumber);
                                    dbCardNum++; // Increment the integer value
                                    string newValue = dbCardNum.ToString("00000");
                                    seetingValue.Value = newValue;
                                    cardNumber = "A" + request.RequestString + cardNumber + seetingValue.Field;
                                }
                                else
                                {
                                    dbCardNum++; // Increment the integer value
                                    string newValue = dbCardNum.ToString("00000");
                                    seetingValue.Value = newValue;

                                }
                                string encDataPass = string.Empty;
                                encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);
                                CardIssueDetail cardDetails = new CardIssueDetail
                                {
                                    CardNumber = encDataPass,
                                    IsActive = true,
                                    CreateDateTime = DateTime.Now,
                                    IssueDate = DateTime.Now,
                                    NricOrPassport = encData
                                };
                                entity.CardIssueDetails.Add(cardDetails);
                                entity.SaveChanges();

                                msg = new Message(_logger);
                                string messageTemplate = msg.GetMessageString(entity);
                                msg.InsertMessage(cardNumber, result.VisitorEmail, entity);
                            }
                            singleregid = 1;
                        }
                        else if (list.Locations.Count > 0)
                        {
                            //if (list.Locations.Count == 1)
                            //{
                            var idtyp = entity.VisitorIdentities.FirstOrDefault(x => x.Name == result.IdTypeName).Id;
                            int get_locid = entity.Locations.FirstOrDefault(x => x.LocationName == result.LocationName).Id;
                            var existloc_ids = list.Locations.Select(y => y.Id).ToList();
                            if (!existloc_ids.Contains(get_locid))
                            {
                                DateTime dt = DateTime.Now;
                                if (idtyp == 1)
                                {
                                    if (list.VistEndDateTime < dt)
                                    {
                                        dt = DateTime.Now.AddHours(24);
                                        list.VistStartDateTime = DateTime.Now;
                                        list.VistEndDateTime = DateTime.Now.AddHours(24);
                                        entity.SaveChanges();
                                    }
                                }
                                if (idtyp == 2)
                                {
                                    if (list.VistEndDateTime < dt)
                                    {
                                        dt = DateTime.Now.AddYears(1);
                                        list.VistStartDateTime = DateTime.Now;
                                        list.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();
                                    }
                                }
                                UserViewModel locationView = new UserViewModel();
                                var lList = new List<string>();
                                lList.Add(get_locid.ToString());
                                locationView.RoleList = new List<RoleViewModel>();
                                foreach (string lId in lList)
                                {
                                    int locId = 0;
                                    if (int.TryParse(lId, out locId))
                                    {
                                        locationView.RoleList.Add(new RoleViewModel { Id = locId, });
                                    }
                                }
                                var LocationIdList = locationView.RoleList.Select(x => x.Id);
                                var location_List = entity.Locations.Where(x => LocationIdList.Contains(x.Id));
                                if (location_List != null)
                                {
                                    foreach (Location _loc in location_List)
                                    {
                                        list.Locations.Add(_loc);
                                    }
                                }
                                entity.SaveChanges();

                                // ====== Update Card Issue Details for IC
                                if (idtyp == 2)
                                {
                                    if (list.VistEndDateTime < dt)
                                    {
                                        CardIssueDetail? checkexistsornot = entity.CardIssueDetails.FirstOrDefault(c => c.NricOrPassport == encData);
                                        if (checkexistsornot != null)
                                        {
                                            checkexistsornot.IssueDate = DateTime.Now;
                                            checkexistsornot.IsActive = true;
                                            entity.SaveChanges();
                                        }
                                    }
                                }
                                // ====== Update Card Issue Details for Passport
                                if (idtyp == 1)
                                {
                                    Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                    int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                    string cardNumber = string.Empty;
                                    cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                    if (dbCardNum >= 99999)
                                    {
                                        seetingValue.Field = RotateCharacters(seetingValue.Field);
                                        //== Reset settings count value
                                        cardNumber = "00001";
                                        dbCardNum = int.Parse(cardNumber);
                                        dbCardNum++; // Increment the integer value
                                        string newValue = dbCardNum.ToString("00000");
                                        seetingValue.Value = newValue;
                                        cardNumber = "A" + request.RequestString + cardNumber + seetingValue.Field;
                                    }
                                    else
                                    {
                                        dbCardNum++; // Increment the integer value
                                        string newValue = dbCardNum.ToString("00000");
                                        seetingValue.Value = newValue;
                                    }
                                    string encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);

                                    CardIssueDetail? checkexpiry = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData && x.IsActive == true);
                                    if (checkexpiry == null)
                                    {
                                        CardIssueDetail cardDetails = new CardIssueDetail
                                        {
                                            CardNumber = encDataPass,
                                            IsActive = true,
                                            CreateDateTime = DateTime.Now,
                                            IssueDate = DateTime.Now,
                                            NricOrPassport = encData
                                        };
                                        entity.CardIssueDetails.Add(cardDetails);
                                        entity.SaveChanges();

                                        msg = new Message(_logger);
                                        string messageTemplate = msg.GetMessageString(entity);
                                        msg.InsertMessage(cardNumber, result.VisitorEmail, entity);
                                    }
                                }
                                singleregid = 1;
                            }
                            else
                            {
                                DateTime dt = DateTime.Now;
                                if (idtyp == 1)
                                {
                                    if (list.VistEndDateTime < dt)
                                    {
                                        dt = DateTime.Now.AddHours(24);
                                        list.VistStartDateTime = DateTime.Now;
                                        list.VistEndDateTime = DateTime.Now.AddHours(24);
                                        entity.SaveChanges();

                                        // ====== Update Card Issue Details for Passport

                                        Setting? seetingValue = entity.Settings.Where(s => s.Type == "32").FirstOrDefault();
                                        int dbCardNum = Convert.ToInt32(seetingValue.Value);

                                        string cardNumber = string.Empty;
                                        cardNumber = "A" + "32" + dbCardNum.ToString("00000") + seetingValue.Field;

                                        if (dbCardNum >= 99999)
                                        {
                                            seetingValue.Field = RotateCharacters(seetingValue.Field);
                                            //== Reset settings count value
                                            cardNumber = "00001";
                                            dbCardNum = int.Parse(cardNumber);
                                            dbCardNum++; // Increment the integer value
                                            string newValue = dbCardNum.ToString("00000");
                                            seetingValue.Value = newValue;
                                            cardNumber = "A" + request.RequestString + cardNumber + seetingValue.Field;
                                        }
                                        else
                                        {
                                            dbCardNum++; // Increment the integer value
                                            string newValue = dbCardNum.ToString("00000");
                                            seetingValue.Value = newValue;
                                        }
                                        string encDataPass = EncryptionDecryptionSHA256.Encrypt(cardNumber);

                                        CardIssueDetail? checkexpiry = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == encData && x.IsActive == true);
                                        if (checkexpiry == null)
                                        {
                                            CardIssueDetail cardDetails = new CardIssueDetail
                                            {
                                                CardNumber = encDataPass,
                                                IsActive = true,
                                                CreateDateTime = DateTime.Now,
                                                IssueDate = DateTime.Now,
                                                NricOrPassport = encData
                                            };
                                            entity.CardIssueDetails.Add(cardDetails);
                                            entity.SaveChanges();

                                            msg = new Message(_logger);
                                            string messageTemplate = msg.GetMessageString(entity);
                                            msg.InsertMessage(cardNumber, result.VisitorEmail, entity);
                                        }

                                        singleregid = 1;
                                    }
                                    else
                                    {
                                        singleregid = -1;
                                    }
                                }
                                if (idtyp == 2)
                                {
                                    if (list.VistEndDateTime < dt)
                                    {
                                        dt = DateTime.Now.AddYears(1);
                                        list.VistStartDateTime = DateTime.Now;
                                        list.VistEndDateTime = DateTime.Now.AddYears(1);
                                        entity.SaveChanges();

                                        // ====== Update Card Issue Details for IC

                                        CardIssueDetail? checkexistsornot = entity.CardIssueDetails.FirstOrDefault(c => c.NricOrPassport == encData);
                                        if (checkexistsornot != null)
                                        {
                                            checkexistsornot.IssueDate = DateTime.Now;
                                            checkexistsornot.IsActive = true;
                                            entity.SaveChanges();
                                        }
                                        singleregid = 1;
                                    }
                                    else
                                    {
                                        singleregid = -1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ErrorMsg = "Single NRIC/Passport is not registered.";
                        singleregid = 0;
                        _logger.LogInformation("Single NRIC/Passport is not registered");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Single registration save is getting error");
                }
            }
            return await Task.FromResult<int>(singleregid);
        }

        public async Task<RegistrationViewModel> EditVstRegAsync(string id)
        {
            bool IsSelected = false;
            var EditvstList = new RegistrationViewModel();
            RegistrationViewModel model = new RegistrationViewModel();
            _logger.LogInformation("Edit Visitor Registration");

            // string dataToEncrypt = result.NricOrPassport;

            string encData = EncryptionDecryptionSHA256.Encrypt(id);
            try
            {
                var result = entity.VisitorRegistrations.Where(x => x.IsDeleted == false && x.NricOrPassport == encData).ToList();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        var result_loc = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.Id == item.Id && x.IsDeleted == false);

                        var existloc_ids = result_loc.Locations.Select(y => y.Id).ToList();
                        var vtypename1 = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).Id;
                        var idtypename2 = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Id;
                        var vtypename = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;
                        EditvstList.Id = Convert.ToInt32(item.Id);
                        EditvstList.VisitorName = item.VisitorName;
                        EditvstList.VisitorEmail = item.Email;
                        EditvstList.NricOrPassport = mask_data;
                        EditvstList.NricOrPassport2 = decryptedData;
                        EditvstList.CompanyName = item.CompanyName;
                        EditvstList.VehicleNo = item.VehicleNo;
                        EditvstList.UnitNo = item.UnitNo;

                        if (item.VisitorContanctNo != null)
                        {
                            EditvstList.ContactNum = item.VisitorContanctNo.ToString();
                        }
                        else
                        {

                        }

                        var singleloclist = entity.Locations.Where(x => x.IsDeleted == false);

                        if (singleloclist != null)
                        {
                            int lcid = existloc_ids[0];
                            var vstrid = entity.Locations.FirstOrDefault(x => x.Id == lcid && x.IsDeleted == false).Id;
                            foreach (var item1 in singleloclist)
                            {
                                if (vstrid == item1.Id)
                                {
                                    IsSelected = true;
                                }
                                else
                                {
                                    IsSelected = false;
                                }
                                model.LocationViewLists.Add(new LocationList
                                {
                                    lLocationId = item1.Id,
                                    lLocationName = item1.LocationName,
                                    lIsSelected = IsSelected
                                });
                            }
                        }
                        var singletypeidlist = entity.VisitorIdentities.Where(x => x.IsDeleted == false);

                        if (singletypeidlist != null)
                        {
                            int typid = idtypename2;

                            var vstrid1 = entity.VisitorIdentities.FirstOrDefault(x => x.Id == typid && x.IsDeleted == false).Id;
                            foreach (var item2 in singletypeidlist)
                            {
                                if (vstrid1 == item2.Id)
                                {
                                    IsSelected = true;
                                }
                                else
                                {
                                    IsSelected = false;
                                }
                                model.IdTypeViewLists.Add(new IdTypeNameList
                                {
                                    ltIdtypeId = item2.Id,
                                    ltIdTypeName = item2.Name,
                                    ltIsSelected = IsSelected
                                });
                            }
                        }

                        var singlevsttypeidlist = entity.VisitTypes.Where(x => x.IsDeleted == false);

                        if (singlevsttypeidlist != null)
                        {
                            int vsttypid = vtypename1;

                            var vstrid2 = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttypid && x.IsDeleted == false).Id;
                            foreach (var item3 in singlevsttypeidlist)
                            {
                                if (vstrid2 == item3.Id)
                                {
                                    IsSelected = true;
                                }
                                else
                                {
                                    IsSelected = false;
                                }
                                model.visitorTypeLists.Add(new VisitorTypeList
                                {
                                    lVsttypeId = item3.Id,
                                    lVstTypeNmae = item3.VisitTypeName,
                                    lVstIsSelected = IsSelected
                                });
                            }
                        }
                        EditvstList.LocationViewLists = model.LocationViewLists;
                        EditvstList.IdTypeViewLists = model.IdTypeViewLists;
                        EditvstList.visitorTypeLists = model.visitorTypeLists;

                        //getting unitIDs

                        var loc_ids = result[0].Locations.Select(y => y.Id).ToList();
                        var Unitslist = entity.UnitDetails.Where(x => x.LocationId == loc_ids[0]).ToList();
                        foreach (var it in Unitslist)
                        {
                            if (result[0].UnitNo == it.UnitId)
                            {
                                IsSelected = true;
                            }
                            else
                            {
                                IsSelected = false;
                            }
                            model.UnitsDetailLists.Add(new UnitsDetailList
                            {
                                Id = it.Id,
                                LocationId = Convert.ToInt32(it.LocationId),
                                BlockNo = it.BlockNo,
                                UnitNo = it.UnitNo,
                                UnitId = it.UnitId,
                                IsSeleted = IsSelected
                            });
                        }
                        EditvstList.UnitsDetailLists = model.UnitsDetailLists;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit visitor registration is getting error.");
                //_logger.LogError(ex, "Visitor service Id is not getting");
            }
            return await Task.FromResult(EditvstList);
        }

        public async Task<int> UpdateVstRegAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int singleregupid = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(request.Model.ToString());
            try
            {
                _logger.LogInformation("Update Visitor Registration");
                if (result != null)
                {

                    string encData = EncryptionDecryptionSHA256.Encrypt(result.NricOrPassport2);
                    //VisitorRegistration nric = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);
                    VisitorRegistration? nric = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.NricOrPassport == encData);

                    var loc = entity.Locations.FirstOrDefault(x => x.LocationName == result.LocationName).Id;
                    var idtyp = entity.VisitorIdentities.FirstOrDefault(x => x.Name == result.IdTypeName).Id;
                    var vstid = entity.VisitTypes.FirstOrDefault(x => x.VisitTypeName == result.VisitorTypeName && x.IsDeleted == false).Id;
                    DateTime dt = DateTime.Now;
                    if (idtyp == 1)
                    {
                        dt = DateTime.Now.AddHours(24);
                    }
                    if (idtyp == 2)
                    {
                        dt = DateTime.Now.AddYears(1);
                    }

                    nric.IdType = idtyp;
                    nric.VisitorName = result.VisitorName;
                    nric.VehicleNo = result.VehicleNo;
                    nric.CompanyName = result.CompanyName;
                    nric.VisitTypeId = vstid;
                    nric.UpdateDateTime = DateTime.Now;
                    nric.UpdateBy = request.UserName;
                    nric.Email = result.VisitorEmail;
                    nric.VisitorContanctNo = result.ContactNum;
                    nric.UnitNo = result.UnitNo;
                    entity.SaveChanges();

                    result.Id = Convert.ToInt32(nric.Id);
                    VisitorRegistration? nric1 = entity.VisitorRegistrations.Include(y => y.Locations).FirstOrDefault(x => x.Id == nric.Id);

                    if (nric1 != null)
                    {
                        int get_locid = entity.Locations.FirstOrDefault(x => x.LocationName == result.LocationName).Id;
                        var existloc_ids = nric.Locations.Select(y => y.Id).ToList();
                        if (!existloc_ids.Contains(get_locid))
                        {
                            UserViewModel roleView = new UserViewModel();
                            var lList = new List<string>();
                            lList.Add(loc.ToString());

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
                                    nric1.Locations.Add(role);
                                }
                            }
                            entity.SaveChanges();
                        }
                        singleregupid = 1;
                    }
                }
                else
                {
                    ErrorMsg = "Single NRIC/Passport is not registered.";
                    singleregupid = -1;
                    _logger.LogInformation("Single NRIC/Passport is not registered.");
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Single NRIC/Passport is not saved");
            }

            return await Task.FromResult<int>(singleregupid);
        }

        public async Task<int> DeleteVstAsync(APIRequest request)
        {
            ErrorMsg = string.Empty;
            int result = 0;

            try
            {
                _logger.LogInformation("Delete visitor details");
                string encData = EncryptionDecryptionSHA256.Encrypt(request.RequestString);

                VisitorRegistration? vst = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == encData);
                if (vst != null)
                {
                    vst.IsDeleted = true;
                    vst.UpdateBy = request.UserName;
                    vst.UpdateDateTime = DateTime.Now;
                    entity.SaveChanges();
                    result = 1;
                }
                else
                {
                    result = -1;
                    ErrorMsg = "Visitor not exist in the system.";
                    _logger.LogInformation("Visitor not exist in the system.");
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                ErrorMsg = "System internal error.";
                _logger.LogError(ex, "Visitor is not deleted.");

                if (result > 0)
                {
                    ErrorMsg = string.Format("{0} Partial deleted [{1}]", ErrorMsg, result);
                }
                else
                {
                    result = -1;
                }
            }
            return await Task.FromResult<int>(result);
        }
        #endregion

        //unitid method
        public async Task<RegistrationViewModel> GetLocationUnitIDSAsync(APIRequest req)
        {
            RegistrationViewModel model = new RegistrationViewModel();
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                _logger.LogInformation("Get location Unit Ids");
                var Unitslist = entity.UnitDetails.Where(x => x.LocationId == result.Id).ToList();
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
                // model.UnitsDetailLists = model.UnitsDetailLists.O.ToList();
            }
            catch (Exception ex)
            {
                //LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "not loaded location Unit Ids");
            }
            return await Task.FromResult(model);
        }

        //Validate UnitID
        public async Task<int> ValidateUnitIDAsync(APIRequest req)
        {
            int res = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(req.Model.ToString());
            try
            {
                if (result != null)
                {
                    _logger.LogInformation("Validate the unit ids");
                    UnitDetail? site = entity.UnitDetails.FirstOrDefault(x => x.UnitId == result.UnitId);
                    if (site != null)
                    {
                        res = 1;
                    }
                    else
                    {
                        res = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                res = 0;
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Validate unit id is getting error");
            }
            return await Task.FromResult<int>(res);
        }

        //ViewAll Details
        public async Task<RegistrationViewModel> ViewAllDetailsAsync(string id)
        {
            //var ViewList = new RegistrationViewModel();
            RegistrationViewModel ViewList = new RegistrationViewModel();
            try
            {
                string encData = EncryptionDecryptionSHA256.Encrypt(id);
                var result = entity.VisitorRegistrations.Where(x => x.NricOrPassport == encData).ToList();
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        string _active = string.Empty;
                        string _blacklisted = string.Empty;
                        string _overstayer = string.Empty;
                        if (item.IsDeleted == true)
                        {
                            _active = "NO";
                        }
                        else
                        {
                            _active = "YES";
                        }
                        if (item.IsBlockListed == true)
                        {
                            _blacklisted = "YES";
                        }
                        else
                        {
                            _blacklisted = "NO";
                        }
                        if (item.OverStayer == true)
                        {
                            _overstayer = "YES";
                        }
                        else
                        {
                            _overstayer = "NO";
                        }

                        var result_loc = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.Id == item.Id);
                        var existloc_ids = result_loc.Locations.Select(y => y.Id).ToList();
                        StringBuilder sb = new StringBuilder();
                        foreach (var locname in existloc_ids)
                        {
                            sb.Append(entity.Locations.FirstOrDefault(x => x.Id == locname).LocationName);
                            sb.Append(',');
                        }
                        ViewList.IdTypeName = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name;
                        ViewList.VisitorTypeName = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName;
                        ViewList.Id = Convert.ToInt32(item.Id);
                        ViewList.VisitorName = item.VisitorName;
                        ViewList.VisitorEmail = item.Email;
                        ViewList.NricOrPassport = mask_data;
                        ViewList.NricOrPassport2 = item.NricOrPassport;
                        //ViewList.CompanyName = item.CompanyName;
                        ViewList.VehicleNo = item.VehicleNo;
                        //ViewList.UnitNo = item.UnitNo;

                        if (ViewList.VisitorTypeName == "Tenants")
                        {
                            ViewList.UnitNo = item.UnitNo;
                        }
                        else if (ViewList.VisitorTypeName == "Workers")
                        {
                            ViewList.UnitNo = item.UnitNo;
                        }

                        else if (ViewList.VisitorTypeName == "Trade Visitors (contractors, commercial buyers, logistics companies)")
                        {
                            ViewList.CompanyName = item.CompanyName;
                        }


                        ViewList.LocationName = sb.ToString().Trim(',');
                        ViewList.IsActive = _active;
                        ViewList.Blacklisted = _blacklisted;
                        ViewList.OverStayer = _overstayer;
                        ViewList.EntryDate = Convert.ToDateTime(item.VistStartDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        ViewList.ExitDate = Convert.ToDateTime(item.VistEndDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");

                        ViewList._EntryDate = item.VistStartDateTime;
                        ViewList._ExitDate = item.VistEndDateTime;
                        ViewList.ContactNum = item.VisitorContanctNo.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit visitor registration is getting error.");
            }
            return await Task.FromResult(ViewList);
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
        //sandesh
        public async Task<int> UpdateNRICPassportAsync(APIRequest request)
        {
            int Status = 0;
            RegistrationViewModel? result = JsonConvert.DeserializeObject<RegistrationViewModel>(request.Model.ToString());
            VisitorRegistration? visitorRegistration = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == result.NricOrPassport);
            if (visitorRegistration != null)
            {
                if (visitorRegistration.IdType == 1)
                {
                    visitorRegistration.VistStartDateTime = DateTime.Now;
                    visitorRegistration.VistEndDateTime = DateTime.Now.AddHours(24);
                    entity.SaveChanges();

                    Status = 1;
                }
                else if (visitorRegistration.IdType == 2)
                {
                    visitorRegistration.VistStartDateTime = DateTime.Now;
                    visitorRegistration.VistEndDateTime = DateTime.Now.AddYears(1);
                    entity.SaveChanges();

                    Status = 1;
                }
                else
                {
                    Status = 0;
                }
            }
            else
            {
                Status = 0;
            }
            return await Task.FromResult<int>(Status);
        }


        //private IActionResult ExportVisitorRegistrationsToExcel(int locationId)
        public async Task<RegistrationViewModel> GetRegistrationsToExcelAsync(APIRequest req)
        {
            RegistrationViewModel model = new RegistrationViewModel();
            int locationId = Convert.ToInt32(req.RequestString);
            var location = entity.Locations.FirstOrDefault(l => l.Id == locationId);
            var data = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == false).OrderByDescending(x => x.CreateDateTime).ToList();
            var loid = locationId;
            if (data != null)
            {
                foreach (var item in data)
                {
                    int i = 0;
                    int j = 0;

                    List<int> lst = new List<int>();
                    var loc = item.Locations.Where(x => x.Id == locationId).ToList();
                    if (loc.Any())
                    {
                        foreach (var l in loc)
                        {
                            lst.Add(l.Id);
                        }
                    }
                    if (lst.Contains(locationId))
                    {
                        model.RegistrationViewLists.Add(new RegistrationViewList
                        {
                            listLocationId = locationId,
                            listVisitorName = item.VisitorName,
                            listNricOrPassport = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport),
                            listContactNum = item.VisitorContanctNo,
                            listentrydate = item.VistStartDateTime.ToString(),
                            listexitdate = item.VistEndDateTime.ToString(),
                            listVisitorTypeName = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName,
                            listCompanyName = item.CompanyName,
                            listUnitNo = item.UnitNo,
                            listVehicleNo = item.VehicleNo,
                            listVisitorEmail = item.Email,
                            listLocationName = string.Join(", ", location.LocationName)
                        });
                    }
                }

            }
            return await Task.FromResult(model);
        }
    }
}

