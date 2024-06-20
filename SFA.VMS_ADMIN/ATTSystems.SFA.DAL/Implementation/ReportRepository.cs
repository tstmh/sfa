using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL.Implementation
{
    public class ReportRepository : IBaseRepository, IDisposable, IReport
    {
        private string ErrorMsg = string.Empty;
        private DataContext entity;
        private readonly ILogger<ReportRepository> _logger;
        string IBaseRepository.GetErrorMsg()
        {

            return ErrorMsg;
        }
        public ReportRepository(ILogger<ReportRepository> logger)
        {
            _logger = logger;
            entity = new DataContext();
        }
        public void Dispose()
        {

        }
        public async Task<ReportViewModel> GetVisitorAccessListtAsync(APIRequest req)
        {
            var RegList = new ReportViewModel();
            _logger.LogInformation("Get visitor access list");
            DateTime sdate = DateTime.Now;
            DateTime edate = sdate.AddDays(-6);
            try
            {
                var result = entity.VisitorTransactions.Where(x => x.TransactionDateTime <= sdate && x.TransactionDateTime >= edate).OrderByDescending(x => x.EntryDateTime).ToList();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        var reg = entity.VisitorRegistrations.Where(x => x.NricOrPassport == item.NricOrPassport && x.IsDeleted == false).ToList();
                        if (reg.Count > 0)
                        {


                            string _entrydate = string.Empty;
                            string _exitdate = string.Empty;

                            string endate = string.Empty;
                            string eedate = string.Empty;
                            //string tdate;
                            if (item.EntryDateTime != null)
                            {
                                endate = Convert.ToDateTime(item.EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                                _entrydate = (endate).ToString();
                            }
                            else
                            {
                                _entrydate = "";
                            }
                            if (item.ExitDateTime != null)
                            {
                                eedate = Convert.ToDateTime(item.ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                                _exitdate = (eedate).ToString();
                            }
                            else
                            {
                                _exitdate = "";
                            }
                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(reg[0].NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(reg[0].NricOrPassport);
                            var mask_data1 = decryptedData1;


                            RegList.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = item.Id,
                                VisitorNamelst = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport).VisitorName,
                                EntryDateTimelst1 = _entrydate,
                                ExitDateTimelst1 = _exitdate,
                                TransactionDateTimelst = item.TransactionDateTime,
                                VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == reg[0].VisitTypeId).VisitTypeName,
                                LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item.LocationId).LocationName,
                                //NricOrPassportlst = entity.VisitorRegistrations.FirstOrDefault(x => x.Id == item.VisitorRegistrationId).NricOrPassport,
                                NricOrPassportlst = mask_data,
                                Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == reg[0].IdType).Name,
                                NricOrPassport1lst = mask_data1

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visitor access list is getting error");
                //LoggerHelper.Instance.LogError(ex);
            }

            return await Task.FromResult(RegList);
        }
        public async Task<ReportViewModel> GetBlockVisitorAccessListtAsync(APIRequest req)
        {
            var RegList = new ReportViewModel();
            _logger.LogInformation("Get black visitor access list");
            try
            {
                var result = entity.BlacklistTransactions.Where(x => x.BlacklistDateTime != null).OrderByDescending(x => x.BlacklistDateTime).ToList();
                // var result = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == true).OrderByDescending(x => x.BlacklistDateTime).Take(100).ToList();

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        var result1 = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.NricOrPassport == item.NricOrPassport).ToList();
                        if (result1.Count > 0)
                        {
                            //DateTime? _entrydate;
                            string _entrydate = string.Empty;
                            string endate = string.Empty;
                            if (item.BlacklistDateTime != null)
                            {
                                endate = Convert.ToDateTime(item.BlacklistDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                                _entrydate = (endate).ToString();
                            }
                            else
                            {
                                _entrydate = "";
                            }
                            var loc_ids = result1[0].Locations.Select(y => y.Id).ToList();
                            StringBuilder strlc = new StringBuilder();
                            foreach (var item1 in loc_ids)
                            {
                                strlc.Append(entity.Locations.FirstOrDefault(x => x.Id == Convert.ToInt32(item1)).LocationName);
                                strlc.Append(",");
                            }
                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(result1[0].NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);


                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(result1[0].NricOrPassport);
                            var mask_data1 = decryptedData1;


                            RegList.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = item.Id,
                                VisitorNamelst = result1[0].VisitorName,
                                VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == result1[0].VisitTypeId).VisitTypeName,
                                // LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                LocationNamelst = strlc.ToString().Trim(','),
                                BlacklistDateTimelst1 = _entrydate,
                                BlacklistBylst = item.BlacklistBy,
                                ReasonForBlacklistlst = item.BlacklistReason,
                                NricOrPassportlst = mask_data,
                                Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == result1[0].IdType).Name,
                                NricOrPassport1lst = mask_data1,
                                Blackliststatus = item.BlacklistResult

                            });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Black listed visitor is getting error");
                //LoggerHelper.Instance.LogError(ex);
            }

            return await Task.FromResult(RegList);
        }

        public async Task<ReportViewModel> GetAudittrailAsync(APIRequest req)
        {
            var RegList = new ReportViewModel();
            _logger.LogInformation("Get audit trail details");
            try
            {
                DateTime today = DateTime.Now;
                DateTime yesterday = today.AddDays(-1);
                var result = entity.VisitorTransactions.AsNoTracking().Where(x => x.ManualCheckIn == 2 || x.ReactivatedDateTime != null).ToList();
                // var result = entity.VisitorTransactions.AsNoTracking().Where(x => x.ManualCheckIn == 2).OrderByDescending(x => x.EntryDateTime).ToList();
                if (result.Count > 0)
                {
                    foreach (var item1 in result)
                    {
                        //var vsttrans = entity.VisitorRegistrations.Where(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && x.ManualCheckIn == 2).AsNoTracking().ToList();
                        //var vsttrans1 = entity.VisitorRegistrations.Where(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && item1.ReactivatedDateTime != null).AsNoTracking().ToList();
                        //if (vsttrans.Count > 0)
                        //{

                        //DateTime? _entrydate;
                        //DateTime? _exitdate;
                        string _entrydate = string.Empty;
                        string _exitdate = string.Empty;
                        string endate = string.Empty;
                        string eendate = string.Empty;
                        if (item1.EntryDateTime != null)
                        {
                            endate = Convert.ToDateTime(item1.EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                            _entrydate = endate.ToString();
                        }
                        else
                        {
                            _entrydate = "";
                        }
                        if (item1.ExitDateTime != null)
                        {
                            eendate = Convert.ToDateTime(item1.ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                            _exitdate = eendate.ToString();
                        }
                        else
                        {
                            _exitdate = "";
                        }

                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item1.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item1.NricOrPassport);
                        var mask_data1 = decryptedData1;
                        var visitor = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport);
                        string remarks = string.Empty;
                        if (item1.ReactivatedDateTime != null)
                        {
                            remarks = "Re-Activated";
                        }
                        else if (item1.ManualCheckIn == 2)
                        {
                            remarks = "Manual checkin";
                        }
                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            Idlst = item1.Id,
                            VisitorNamelst = visitor.VisitorName,
                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == visitor.VisitTypeId).VisitTypeName,
                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                            VisitStratDateTimelst1 = _entrydate,
                            VisitEndDateTimelst1 = _exitdate,
                            ManualCheckInBylst = item1.ManualCheckInBy,
                            NricOrPassportlst = mask_data,
                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == visitor.IdType).Name,
                            NricOrPassport1lst = mask_data1,
                            Remarks = remarks,
                        });
                        // }

                        //if (vsttrans1.Count > 0)
                        //{

                        //    //DateTime? _entrydate;
                        //    //DateTime? _exitdate;
                        //    string _entrydate = string.Empty;
                        //    string _exitdate = string.Empty;
                        //    string endate = string.Empty;
                        //    string eendate = string.Empty;
                        //    if (item1.EntryDateTime != null)
                        //    {
                        //        endate = Convert.ToDateTime(item1.EntryDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        //        _entrydate = endate.ToString();
                        //    }
                        //    else
                        //    {
                        //        _entrydate = "";
                        //    }
                        //    if (item1.ExitDateTime != null)
                        //    {
                        //        eendate = Convert.ToDateTime(item1.ExitDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        //        _exitdate = eendate.ToString();
                        //    }
                        //    else
                        //    {
                        //        _exitdate = "";
                        //    }
                        //    string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans1[0].NricOrPassport);
                        //    var length = decryptedData.Length;
                        //    var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        //    string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans1[0].NricOrPassport);
                        //    var mask_data1 = decryptedData1;

                        //    RegList.VisitorAccessLists.Add(new VisitorAccessList
                        //    {
                        //        Idlst = vsttrans1[0].Id,
                        //        VisitorNamelst = vsttrans1[0].VisitorName,
                        //        VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans1[0].VisitTypeId).VisitTypeName,
                        //        LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                        //        VisitStratDateTimelst1 = _entrydate,
                        //        VisitEndDateTimelst1 = _exitdate,
                        //        ManualCheckInBylst = vsttrans1[0].ManualCheckInBy,
                        //        NricOrPassportlst = mask_data,
                        //        Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans1[0].IdType).Name,
                        //        NricOrPassport1lst = mask_data1,
                        //        Remarks = "Re-Activated"
                        //    });
                        //}
                    }
                }

            }
            catch (Exception ex)
            {
                //LoggerHelper.Instance.LogError(ex);
                _logger.LogError(ex, "Get audit trail details");
            }

            return await Task.FromResult(RegList);
        }
        public async Task<ReportViewModel> GetSearchVisitoraccessAsync(APIRequest request)
        {
            ReportViewModel result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());
            try
            {
                _logger.LogInformation("Search visitor access list");
                if (result.VisitTypeId == 0 && result.LocationId == 0)
                {
                    var vstaccslsts = (from vt in entity.VisitorTransactions
                                       join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                       join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                       join vi in entity.VisitorIdentities on vr.IdType equals vi.Id
                                       join lc in entity.Locations on vt.LocationId equals lc.Id
                                       where vr.IsDeleted == false && vt.TransactionDateTime.Value.Date >= result.StartDate.Value.Date && vt.TransactionDateTime.Value.Date <= result.EndDate.Value.Date
                                       select new
                                       {
                                           vt.Id,
                                           vr.VisitorName,
                                           vst.VisitTypeName,
                                           lc.LocationName,
                                           vt.EntryDateTime,
                                           vt.ExitDateTime,
                                           vi.Name,
                                           vr.NricOrPassport
                                           //vt.TransactionDateTime
                                       }
                                   ).ToList();
                    if (vstaccslsts != null)
                    {
                        foreach (var item in vstaccslsts)
                        {
                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var mask_data1 = decryptedData1;

                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),
                                VisitorNamelst = item.VisitorName,
                                VisitTypeNamelst = item.VisitTypeName,
                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                //TransactionDateTimelst = item.TransactionDateTime,
                                LocationNamelst = item.LocationName,
                                Namelst = item.Name,
                                NricOrPassportlst = mask_data,
                                NricOrPassport1lst = mask_data1

                            });

                        }
                    }
                }
                else if (result.VisitTypeId != 0 && result.LocationId == 0)
                {
                    var vstaccslsts = (from vt in entity.VisitorTransactions
                                       join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                       join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                       join vi in entity.VisitorIdentities on vr.IdType equals vi.Id
                                       join lc in entity.Locations on vt.LocationId equals lc.Id
                                       where vr.IsDeleted == false && vr.VisitTypeId == result.VisitTypeId && vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date
                                       select new
                                       {
                                           vt.Id,
                                           vr.VisitorName,
                                           vst.VisitTypeName,
                                           lc.LocationName,
                                           vt.EntryDateTime,
                                           vt.ExitDateTime,
                                           vi.Name,
                                           vr.NricOrPassport
                                           // vt.TransactionDateTime
                                       }

                                  ).ToList();
                    if (vstaccslsts != null)
                    {
                        foreach (var item in vstaccslsts)
                        {

                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var mask_data1 = decryptedData1;

                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),
                                VisitorNamelst = item.VisitorName,
                                VisitTypeNamelst = item.VisitTypeName,
                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                //TransactionDateTimelst = item.TransactionDateTime,
                                LocationNamelst = item.LocationName,
                                Namelst = item.Name,
                                NricOrPassportlst = mask_data,
                                NricOrPassport1lst = mask_data1
                            });

                        }
                    }
                }
                else if (result.LocationId != 0 && result.VisitTypeId == 0)
                {
                    var vstaccslsts = (from vt in entity.VisitorTransactions
                                       join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                       join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                       join vi in entity.VisitorIdentities on vr.IdType equals vi.Id
                                       join lc in entity.Locations on vt.LocationId equals lc.Id
                                       where vr.IsDeleted == false && vt.LocationId == result.LocationId && vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date
                                       select new
                                       {
                                           vt.Id,
                                           vr.VisitorName,
                                           vst.VisitTypeName,
                                           lc.LocationName,
                                           vt.EntryDateTime,
                                           vt.ExitDateTime,
                                           vi.Name,
                                           vr.NricOrPassport
                                           // vt.TransactionDateTime
                                       }

                                  ).ToList();
                    if (vstaccslsts != null)
                    {
                        foreach (var item in vstaccslsts)
                        {
                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var mask_data1 = decryptedData1;

                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),
                                VisitorNamelst = item.VisitorName,
                                VisitTypeNamelst = item.VisitTypeName,
                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                //TransactionDateTimelst = item.TransactionDateTime,
                                LocationNamelst = item.LocationName,
                                Namelst = item.Name,
                                NricOrPassportlst = mask_data,
                                NricOrPassport1lst = mask_data1
                            });

                        }
                    }
                }
                else
                {
                    var vstaccslsts = (from vt in entity.VisitorTransactions
                                       join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                       join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                       join vi in entity.VisitorIdentities on vr.IdType equals vi.Id
                                       join lc in entity.Locations on vt.LocationId equals lc.Id
                                       where vr.IsDeleted == false && vr.VisitTypeId == result.VisitTypeId && vt.LocationId == result.LocationId && vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date
                                       select new
                                       {
                                           vt.Id,
                                           vr.VisitorName,
                                           vst.VisitTypeName,
                                           lc.LocationName,
                                           vt.EntryDateTime,
                                           vt.ExitDateTime,
                                           vi.Name,
                                           vr.NricOrPassport

                                           // vt.TransactionDateTime
                                       }

                                   ).ToList();
                    if (vstaccslsts != null)
                    {
                        foreach (var item in vstaccslsts)
                        {

                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var mask_data1 = decryptedData1;

                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),
                                VisitorNamelst = item.VisitorName,
                                VisitTypeNamelst = item.VisitTypeName,
                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                //TransactionDateTimelst = item.TransactionDateTime,
                                LocationNamelst = item.LocationName,
                                Namelst = item.Name,
                                NricOrPassportlst = mask_data,
                                NricOrPassport1lst = mask_data1
                            });

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search visitor access list is getting error");
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
            }
            return await Task.FromResult(result);
        }
        public async Task<ReportViewModel> GetSearchblkVisitorAsync(APIRequest request)
        {
            var reglist = new ReportViewModel();
            ReportViewModel? result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());

            _logger.LogInformation("Search black listed visitor");
            try
            {
                if (result != null)
                {
                    if (result.NricOrPassport == null && result.StartDate != null && result.EndDate != null)
                    {
                        var blc = entity.BlacklistTransactions.Where(x => x.BlacklistDateTime != null && x.BlacklistDateTime.Value.Date >= result.StartDate.Value.Date && x.BlacklistDateTime.Value.Date <= result.EndDate.Value.Date).OrderByDescending(x => x.BlacklistDateTime).ToList();
                        // var result1 = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == true && x.BlacklistDateTime.Value.Date >= result.StartDate.Value.Date && x.BlacklistDateTime.Value.Date <= result.EndDate.Value.Date).OrderByDescending(x => x.BlacklistDateTime).ToList();

                        if (blc != null)
                        {
                            if (blc.Count > 0)
                            {
                                foreach (var item in blc)
                                {
                                    var result1 = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.IsDeleted == false && x.NricOrPassport == item.NricOrPassport);
                                    if (result1 != null)
                                    {
                                        var loc_ids = result1.Locations;
                                        StringBuilder strlc = new StringBuilder();
                                        foreach (var item1 in loc_ids)
                                        {

                                            strlc.Append(item1.LocationName);

                                            strlc.Append(",");
                                        }
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(result1.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(result1.NricOrPassport);
                                        var mask_data1 = decryptedData1;

                                        reglist.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item.Id,
                                            VisitorNamelst = result1.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == result1.VisitTypeId).VisitTypeName,
                                            //LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                            LocationNamelst = strlc.ToString().Trim(','),
                                            BlacklistDateTimelst = item.BlacklistDateTime,
                                            BlacklistBylst = item.BlacklistBy,
                                            ReasonForBlacklistlst = item.BlacklistReason,
                                            NricOrPassportlst = mask_data,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == result1.IdType).Name,
                                            NricOrPassport1lst = mask_data1,
                                            Blackliststatus = item.BlacklistResult

                                        });
                                    }

                                }
                            }

                        }
                    }
                    else if (result.NricOrPassport != null && result.StartDate == null && result.EndDate == null)
                    {
                        string dataToEncrypt = result.NricOrPassport;

                        string encData = EncryptionDecryptionSHA256.Encrypt(dataToEncrypt);
                        var blc = entity.BlacklistTransactions.Where(x => x.NricOrPassport == encData).OrderByDescending(x => x.BlacklistDateTime).ToList();
                        // var result1 = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == true && x.BlacklistDateTime.Value.Date >= result.StartDate.Value.Date && x.BlacklistDateTime.Value.Date <= result.EndDate.Value.Date && x.NricOrPassport == encData).OrderByDescending(x => x.BlacklistDateTime).ToList();

                        if (blc != null)
                        {
                            if (blc.Count > 0)
                            {
                                foreach (var item in blc)
                                {
                                    var result1 = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.NricOrPassport == item.NricOrPassport).ToList();
                                    if (result1.Count > 0)
                                    {
                                        var loc_ids = result1[0].Locations.Select(y => y.Id).ToList();
                                        StringBuilder strlc = new StringBuilder();
                                        foreach (var item1 in loc_ids)
                                        {

                                            strlc.Append(entity.Locations.FirstOrDefault(x => x.Id == Convert.ToInt32(item1)).LocationName);

                                            strlc.Append(",");
                                        }
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(result1[0].NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(result1[0].NricOrPassport);
                                        var mask_data1 = decryptedData1;

                                        reglist.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item.Id,
                                            VisitorNamelst = result1[0].VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == result1[0].VisitTypeId).VisitTypeName,
                                            //LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                            LocationNamelst = strlc.ToString().Trim(','),
                                            BlacklistDateTimelst = item.BlacklistDateTime,
                                            BlacklistBylst = item.BlacklistBy,
                                            ReasonForBlacklistlst = item.BlacklistReason,
                                            NricOrPassportlst = mask_data,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == result1[0].IdType).Name,
                                            NricOrPassport1lst = mask_data1,
                                            Blackliststatus = item.BlacklistResult

                                        });
                                    }

                                }
                            }

                        }
                    }
                    else if (result.StartDate != null && result.EndDate != null && result.NricOrPassport != null)
                    {
                        string nric = EncryptionDecryptionSHA256.Encrypt(result.NricOrPassport);
                        var blc = entity.BlacklistTransactions.Where(x => x.BlacklistDateTime != null && x.BlacklistDateTime.Value.Date >= result.StartDate.Value.Date && x.BlacklistDateTime.Value.Date <= result.EndDate.Value.Date && x.NricOrPassport == nric).OrderByDescending(x => x.BlacklistDateTime).ToList();
                        // var result1 = entity.VisitorRegistrations.Include(x => x.Locations.Where(x => x.Id == result.LocationId)).Where(x => x.IsDeleted == false && x.IsBlockListed == true && x.BlacklistDateTime.Value.Date >= result.StartDate.Value.Date && x.BlacklistDateTime.Value.Date <= result.EndDate.Value.Date).OrderByDescending(x => x.BlacklistDateTime).ToList();

                        if (blc != null)
                        {
                            if (blc.Count > 0)
                            {
                                foreach (var item in blc)
                                {
                                    var result1 = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.IsDeleted == false && x.NricOrPassport == item.NricOrPassport);
                                    if (result1 != null)
                                    {
                                        var loc_ids = result1.Locations.ToList();
                                        string locname = string.Empty;
                                        StringBuilder sb = new StringBuilder();
                                        foreach (var location in loc_ids)
                                        {
                                            sb.Append(location.LocationName);
                                            sb.Append(", ");
                                        }
                                        locname = sb.ToString().TrimEnd(',');

                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(result1.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(result1.NricOrPassport);
                                        var mask_data1 = decryptedData1;

                                        reglist.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item.Id,
                                            VisitorNamelst = result1.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == result1.VisitTypeId).VisitTypeName,
                                            LocationNamelst = locname,
                                            BlacklistDateTimelst = item.BlacklistDateTime,
                                            BlacklistBylst = item.BlacklistBy,
                                            ReasonForBlacklistlst = item.BlacklistReason,
                                            NricOrPassportlst = mask_data,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == result1.IdType).Name,
                                            NricOrPassport1lst = mask_data1,
                                            Blackliststatus = item.BlacklistResult

                                        });


                                    }

                                }
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Search black listed visitor is getting error");
            }
            return await Task.FromResult(reglist);
        }
        public async Task<ReportViewModel> GetSearchAudittrailAsync(APIRequest request)
        {
            var RegList = new ReportViewModel();
            ReportViewModel? result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());
            try
            {
                if (result != null)
                {
                    _logger.LogInformation("Search audit trail visitor");
                    if (result.LocationId == 0 && result.VisitTypeId == 0 && result.AuditTypeId != 0)
                    {
                        if (result.AuditTypeId == 1)
                        {
                            var vsttrans = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == true).ToList();

                            if (vsttrans.Count > 0)
                            {
                                foreach (var item in vsttrans)
                                {
                                    var loc_ids = item.Locations;

                                    string locname = string.Empty;
                                    StringBuilder sb = new StringBuilder();
                                    foreach (var location in loc_ids)
                                    {
                                        sb.Append(location.LocationName);
                                        sb.Append(", ");
                                    }
                                    locname = sb.ToString().TrimEnd(',');
                                    RegList.VisitorAccessLists.Add(new VisitorAccessList
                                    {
                                        Idlst = vsttrans[0].Id,
                                        VisitorNamelst = vsttrans[0].VisitorName,
                                        VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans[0].VisitTypeId).VisitTypeName,
                                        LocationNamelst = locname,
                                        EntryDateTimelst = item.VistStartDateTime,
                                        ExitDateTimelst = item.VistEndDateTime,
                                        ManualCheckInBylst = vsttrans[0].ManualCheckInBy,
                                        Remarks = "Manual checkin"
                                    });
                                }

                            }
                        }
                        else if (result.AuditTypeId == 2)
                        {
                            var result1 = entity.VisitorTransactions.AsNoTracking().Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.ManualCheckIn == 2).OrderByDescending(x => x.EntryDateTime).ToList();

                            if (result1.Count > 0)
                            {
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false);

                                    if (vsttrans != null)
                                    {
                                        var loc_ids = vsttrans.Locations;

                                        string locname = string.Empty;
                                        StringBuilder sb = new StringBuilder();
                                        foreach (var location in loc_ids)
                                        {
                                            sb.Append(location.LocationName);
                                            sb.Append(", ");
                                        }
                                        locname = sb.ToString().TrimEnd(',');
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = locname,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Manual checkin"

                                        });
                                    }
                                }
                            }
                        }
                        else if (result.AuditTypeId == 3)
                        {
                            var result1 = entity.VisitorTransactions.AsNoTracking().Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.ReactivatedDateTime != null).OrderByDescending(x => x.EntryDateTime).ToList();

                            if (result1.Count > 0)
                            {
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false);

                                    if (vsttrans != null)
                                    {
                                        var loc_ids = vsttrans.Locations;

                                        string locname = string.Empty;
                                        StringBuilder sb = new StringBuilder();
                                        foreach (var location in loc_ids)
                                        {
                                            sb.Append(location.LocationName);
                                            sb.Append(", ");
                                        }
                                        locname = sb.ToString().TrimEnd(',');
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = locname,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Re-Activate"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (result.LocationId != 0 && result.VisitTypeId == 0 && result.AuditTypeId != 0)
                    {
                        if (result.AuditTypeId == 1)
                        {
                            var vsttrans = entity.VisitorRegistrations.Include(x => x.Locations.Where(x => x.Id == result.LocationId)).Where(x => x.IsDeleted == false && x.IsBlockListed == true).ToList();

                            if (vsttrans.Count > 0)
                            {
                                foreach (var item in vsttrans)
                                {
                                    var loc_ids = item.Locations.Select(y => y.Id).ToList();
                                    RegList.VisitorAccessLists.Add(new VisitorAccessList
                                    {
                                        Idlst = vsttrans[0].Id,
                                        VisitorNamelst = vsttrans[0].VisitorName,
                                        VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans[0].VisitTypeId).VisitTypeName,
                                        LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                        EntryDateTimelst = item.VistStartDateTime,
                                        ExitDateTimelst = item.VistEndDateTime,
                                        ManualCheckInBylst = vsttrans[0].ManualCheckInBy,
                                    });
                                }

                            }
                        }
                        else if (result.AuditTypeId == 2)
                        {
                            var result1 = entity.VisitorTransactions.AsNoTracking().Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.LocationId == result.LocationId && x.ManualCheckIn == 2).OrderByDescending(x => x.EntryDateTime).ToList();

                            if (result1.Count > 0)
                            {
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Manual checkin"
                                        });
                                    }
                                }
                            }
                        }
                        else if (result.AuditTypeId == 3)
                        {
                            //var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate && x.EntryDateTime <= result.EndDate && x.LocationId == result.LocationId).Take(100).ToList();
                            var result1 = entity.VisitorTransactions.AsNoTracking().Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.ReactivatedDateTime != null && x.LocationId == result.LocationId).ToList();
                            if (result1.Count > 0)
                            {
                                // result1 = result1.Where(x => x.EntryDateTime.Value < result.StartDate.Value.AddDays(-1) && x.ExitDateTime == null).ToList();
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Re-Activate"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (result.LocationId == 0 && result.VisitTypeId != 0 && result.AuditTypeId != 0)
                    {

                        if (result.AuditTypeId == 1)
                        {
                            var vsttrans = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.IsBlockListed == true && x.VisitTypeId == result.VisitTypeId).ToList();

                            if (vsttrans.Count > 0)
                            {
                                foreach (var item in vsttrans)
                                {
                                    var loc_ids = item.Locations.Select(y => y.Id).ToList();
                                    RegList.VisitorAccessLists.Add(new VisitorAccessList
                                    {
                                        Idlst = vsttrans[0].Id,
                                        VisitorNamelst = vsttrans[0].VisitorName,
                                        VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans[0].VisitTypeId).VisitTypeName,
                                        LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                        EntryDateTimelst = item.VistStartDateTime,
                                        ExitDateTimelst = item.VistEndDateTime,
                                        ManualCheckInBylst = vsttrans[0].ManualCheckInBy,

                                    });
                                }
                            }
                        }

                        else if (result.AuditTypeId == 2)
                        {
                            var result1 = entity.VisitorTransactions.AsNoTracking().Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.ManualCheckIn == 2).OrderByDescending(x => x.EntryDateTime).ToList();

                            if (result1.Count > 0)
                            {
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && x.VisitTypeId == result.VisitTypeId);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Manual checkin"
                                        });
                                    }
                                }
                            }
                        }
                        else if (result.AuditTypeId == 3)
                        {
                            //var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate && x.EntryDateTime <= result.EndDate).Take(100).ToList();
                            var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.ReactivatedDateTime != null).ToList();

                            if (result1.Count > 0)
                            {
                                // result1 = result1.Where(x => x.EntryDateTime.Value < result.StartDate.Value.AddDays(-1) && x.ExitDateTime == null).ToList();
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && x.VisitTypeId == result.VisitTypeId);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Re-Activate"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (result.LocationId != 0 && result.VisitTypeId != 0 && result.AuditTypeId != 0)
                    {
                        if (result.AuditTypeId == 1)
                        {
                            var vsttrans = entity.VisitorRegistrations.Include(x => x.Locations.Where(x => x.Id == result.LocationId)).Where(x => x.IsDeleted == false && x.IsBlockListed == true && x.VisitTypeId == result.VisitTypeId).ToList();

                            if (vsttrans.Count > 0)
                            {
                                foreach (var item in vsttrans)
                                {
                                    var loc_ids = item.Locations.Select(y => y.Id).ToList();
                                    RegList.VisitorAccessLists.Add(new VisitorAccessList
                                    {
                                        Idlst = vsttrans[0].Id,
                                        VisitorNamelst = vsttrans[0].VisitorName,
                                        VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans[0].VisitTypeId).VisitTypeName,
                                        LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == loc_ids[0]).LocationName,
                                        EntryDateTimelst = item.VistStartDateTime,
                                        ExitDateTimelst = item.VistEndDateTime,
                                        ManualCheckInBylst = vsttrans[0].ManualCheckInBy,
                                    });
                                }
                            }
                        }
                        else if (result.AuditTypeId == 2)
                        {
                            var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.EntryDateTime.Value.Date <= result.EndDate.Value.Date && x.LocationId == result.LocationId && x.ManualCheckIn == 2).ToList();

                            if (result1.Count > 0)
                            {
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && x.VisitTypeId == result.VisitTypeId);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Manual checkin"
                                        });
                                    }
                                }
                            }
                        }
                        else if (result.AuditTypeId == 3)
                        {
                            //var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate && x.EntryDateTime <= result.EndDate && x.LocationId == result.LocationId).Take(100).ToList();
                            var result1 = entity.VisitorTransactions.OrderByDescending(x => x.EntryDateTime).Where(x => x.EntryDateTime.Value.Date >= result.StartDate.Value.Date && x.ExitDateTime.Value.Date <= result.EndDate.Value.Date && x.ReactivatedDateTime != null && x.LocationId == result.LocationId).ToList();
                            if (result1.Count > 0)
                            {
                                // result1 = result1.Where(x => x.EntryDateTime.Value < result.StartDate.Value.AddDays(-1) && x.ExitDateTime == null).ToList();
                                foreach (var item1 in result1)
                                {
                                    var vsttrans = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item1.NricOrPassport && x.IsDeleted == false && x.VisitTypeId == result.VisitTypeId);

                                    if (vsttrans != null)
                                    {
                                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var length = decryptedData.Length;
                                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(vsttrans.NricOrPassport);
                                        var mask_data1 = decryptedData1;
                                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                                        {
                                            Idlst = item1.Id,
                                            VisitorNamelst = vsttrans.VisitorName,
                                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == vsttrans.VisitTypeId).VisitTypeName,
                                            LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item1.LocationId).LocationName,
                                            EntryDateTimelst = item1.EntryDateTime,
                                            ExitDateTimelst = item1.ExitDateTime,
                                            ManualCheckInBylst = item1.ManualCheckInBy,
                                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == vsttrans.IdType).Name,
                                            NricOrPassportlst = mask_data,
                                            NricOrPassport1lst = mask_data1,
                                            Remarks = "Re-Activated"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Search audit trail visitor is getting error");
            }
            return await Task.FromResult(RegList);
        }
        public async Task<List<VisitType>> GetVisitorTypeAsync(APIRequest request)
        {
            _logger.LogInformation("Get visitor types");
            List<VisitType> result = null;
            try
            {
                var visitTypeList = entity.VisitTypes.Where(x => x.IsDeleted == false);
                if (visitTypeList != null)
                {
                    result = visitTypeList.ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal Error. \n{0}", ex.InnerException);
                _logger.LogError(ex, "Get visitor types is getting error");
            }

            return await Task.FromResult<List<VisitType>>(result);
        }
        public async Task<List<Location>> GetLocationAsync(APIRequest request)
        {
            List<Location> result = null;
            _logger.LogInformation("Get location list");
            using (var entity = new DataContext())
            {
                try
                {
                    var loclist = entity.Locations.Where(x => x.IsDeleted == false);
                    if (loclist != null)
                    {
                        result = loclist.ToList();
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

        #region graph

        public async Task<ReportViewModel> GetSearchlinechartAsync(APIRequest request)
        {
            ReportViewModel result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());

            _logger.LogInformation("get search line chart");
            int _countvstacs = 0;
            try
            {
                if (result.VisitTypeId == 0 && result.LocationId == 0)
                {
                    string str = result.VisitTypeName;
                    result.transLists = new List<TransList>();
                    var vstaccesslist1 = (from vt in entity.VisitorTransactions
                                          join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                          join lc in entity.Locations on vt.LocationId equals lc.Id
                                          join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                          where vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date && vr.IsDeleted == false

                                          select new
                                          {
                                              vt.Id,
                                              vt.TransactionDateTime,
                                              vt.ExitDateTime,
                                              vt.EntryDateTime,
                                              vr.VisitTypeId,
                                              vst.VisitTypeName
                                          }).ToList();

                    if (vstaccesslist1 != null)
                    {
                        //_countvstacs=vstaccesslist.Count;
                        foreach (var item in vstaccesslist1)
                        {
                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),
                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                VisitTypeNamelst = result.VisitTypeName,
                            });
                        }
                    }
                    var translist1 = (vstaccesslist1.GroupBy(x => new { _date = x.EntryDateTime.Value.Date, _vttype = x.VisitTypeName }).Select(x => new { Date = x.Key._date, vstypnm = x.Key._vttype, count = x.Count() })).ToList();
                    if (translist1 != null)
                    {
                        foreach (var item in translist1)
                        {
                            result.transLists.Add(new TransList
                            {
                                Countlst = item.count,
                                Datelst = item.Date,
                                Vsttypnamelst = item.vstypnm
                            });
                            _countvstacs = _countvstacs + item.count;
                        }
                    }
                    int sum1 = _countvstacs;
                    result.Nooftranscation = sum1.ToString();
                    result.transLists = result.transLists.OrderByDescending(x => x.Datelst).ToList();
                    result.transLists1 = new List<TransList>();
                    foreach (var item in result.transLists)
                    {
                        result.transLists1.Add(new TransList
                        {
                            Countlst = item.Countlst,
                            Datelst1 = item.Datelst.ToShortDateString(),
                            Vsttypnamelst = item.Vsttypnamelst
                        });
                    }
                }
                else if (result.VisitTypeId != 0 && result.LocationId == 0)
                {
                    var vstaccesslist1 = (from vt in entity.VisitorTransactions
                                          join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                          join lc in entity.Locations on vt.LocationId equals lc.Id
                                          join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                          where vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date && vr.IsDeleted == false && vr.VisitTypeId == result.VisitTypeId

                                          select new
                                          {
                                              vt.Id,
                                              vt.TransactionDateTime,
                                              vt.ExitDateTime,
                                              vt.EntryDateTime,
                                              vr.VisitTypeId,
                                              vst.VisitTypeName
                                          }).ToList();

                    if (vstaccesslist1 != null)
                    {
                        //_countvstacs=vstaccesslist.Count;
                        foreach (var item in vstaccesslist1)
                        {
                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),

                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                VisitTypeNamelst = result.VisitTypeName,
                            });
                        }
                    }
                    var translist1 = (vstaccesslist1.GroupBy(x => new { _date = x.EntryDateTime.Value.Date, _vttype = x.VisitTypeName }).Select(x => new { Date = x.Key._date, vstypnm = x.Key._vttype, count = x.Count() })).ToList();
                    if (translist1 != null)
                    {

                        foreach (var item in translist1)
                        {
                            result.transLists.Add(new TransList
                            {

                                Countlst = item.count,
                                Datelst = item.Date,
                                Vsttypnamelst = item.vstypnm
                            });
                            _countvstacs = _countvstacs + item.count;
                        }
                    }

                    int sum1 = _countvstacs;
                    result.Nooftranscation = sum1.ToString();

                    result.transLists = result.transLists.OrderByDescending(x => x.Datelst).ToList();
                    result.transLists1 = new List<TransList>();
                    foreach (var item in result.transLists)
                    {
                        result.transLists1.Add(new TransList
                        {
                            Countlst = item.Countlst,
                            Datelst1 = item.Datelst.ToShortDateString(),
                            Vsttypnamelst = item.Vsttypnamelst
                        });
                    }

                }

                else if (result.VisitTypeId == 0 && result.LocationId != 0)
                {
                    //string str = result.VisitTypeName;
                    //result.transLists = new List<TransList>();


                    var vstaccesslist1 = (from vt in entity.VisitorTransactions
                                          join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                          join lc in entity.Locations on vt.LocationId equals lc.Id
                                          join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                          where vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date && vr.IsDeleted == false && vt.LocationId == result.LocationId


                                          select new
                                          {
                                              vt.Id,
                                              vt.TransactionDateTime,
                                              vt.ExitDateTime,
                                              vt.EntryDateTime,
                                              vr.VisitTypeId,
                                              vst.VisitTypeName
                                          }).ToList();



                    if (vstaccesslist1 != null)
                    {
                        //_countvstacs=vstaccesslist.Count;
                        foreach (var item in vstaccesslist1)
                        {
                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),

                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                VisitTypeNamelst = result.VisitTypeName,
                            });
                        }
                    }
                    var translist1 = (vstaccesslist1.GroupBy(x => new { _date = x.EntryDateTime.Value.Date, _vttype = x.VisitTypeName }).Select(x => new { Date = x.Key._date, vstypnm = x.Key._vttype, count = x.Count() })).ToList();
                    if (translist1 != null)
                    {
                        foreach (var item in translist1)
                        {
                            result.transLists.Add(new TransList
                            {

                                Countlst = item.count,
                                Datelst = item.Date,
                                Vsttypnamelst = item.vstypnm
                            });
                            _countvstacs = _countvstacs + item.count;
                        }
                    }
                    int sum1 = _countvstacs;
                    result.Nooftranscation = sum1.ToString();

                    result.transLists = result.transLists.OrderByDescending(x => x.Datelst).ToList();
                    result.transLists1 = new List<TransList>();
                    foreach (var item in result.transLists)
                    {
                        result.transLists1.Add(new TransList
                        {
                            Countlst = item.Countlst,
                            Datelst1 = item.Datelst.ToShortDateString(),
                            Vsttypnamelst = item.Vsttypnamelst
                        });
                    }
                }
                else
                {
                    var vstaccesslist = (from vt in entity.VisitorTransactions
                                         join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                         join lc in entity.Locations on vt.LocationId equals lc.Id
                                         join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                         where vr.IsDeleted == false && vt.EntryDateTime.Value.Date >= result.StartDate.Value.Date && vt.EntryDateTime.Value.Date <= result.EndDate.Value.Date && vr.VisitTypeId == result.VisitTypeId && lc.Id == result.LocationId

                                         select new
                                         {
                                             vt.Id,
                                             vt.TransactionDateTime,
                                             vt.ExitDateTime,
                                             vt.EntryDateTime,
                                             vr.VisitTypeId,
                                             vst.VisitTypeName
                                         }).ToList();


                    if (vstaccesslist != null)
                    {
                        //_countvstacs=vstaccesslist.Count;
                        foreach (var item in vstaccesslist)
                        {
                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = Convert.ToInt32(item.Id),

                                EntryDateTimelst = item.EntryDateTime,
                                ExitDateTimelst = item.ExitDateTime,
                                VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == result.VisitTypeId).VisitTypeName,

                            });
                        }
                    }
                    var translist = (vstaccesslist.GroupBy(x => new { _date = x.EntryDateTime.Value.Date, _vttype = x.VisitTypeName }).Select(x => new { Date = x.Key._date, vstypnm = x.Key._vttype, count = x.Count() })).ToList();
                    if (translist != null)
                    {
                        result.transLists = new List<TransList>();
                        foreach (var item in translist)
                        {
                            result.transLists.Add(new TransList
                            {

                                Countlst = item.count,
                                Datelst = item.Date,
                                Vsttypnamelst = item.vstypnm
                            });
                            _countvstacs = _countvstacs + item.count;
                        }
                    }

                    int sum = _countvstacs;
                    result.Nooftranscation = sum.ToString();

                }
                result.transLists = result.transLists.OrderByDescending(x => x.Datelst).ToList();
                result.transLists1 = new List<TransList>();
                foreach (var item in result.transLists)
                {
                    result.transLists1.Add(new TransList
                    {
                        Countlst = item.Countlst,
                        Datelst1 = item.Datelst.ToShortDateString(),
                        Vsttypnamelst = item.Vsttypnamelst
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "get search line chart is getting error");
            }
            return await Task.FromResult<ReportViewModel>(result);
        }
        public async Task<ReportViewModel> GetDefaultlinechartAsync(APIRequest request)
        {

            ReportViewModel? result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());
            _logger.LogInformation("Get default line cahrt");
            DateTime sdate = DateTime.Now;
            DateTime edate = sdate.AddDays(-6);
            int _countvstacs = 0;

            try
            {
                var vstaccesslist = (from vt in entity.VisitorTransactions
                                     join vr in entity.VisitorRegistrations on vt.NricOrPassport equals vr.NricOrPassport
                                     join lc in entity.Locations on vt.LocationId equals lc.Id
                                     join vst in entity.VisitTypes on vr.VisitTypeId equals vst.Id
                                     where vt.TransactionDateTime <= sdate.Date && vt.TransactionDateTime >= edate.Date && vr.IsDeleted == false

                                     select new
                                     {
                                         vt.Id,
                                         vt.TransactionDateTime,
                                         vt.ExitDateTime,
                                         vt.EntryDateTime,
                                         vr.VisitTypeId,
                                         vst.VisitTypeName
                                     }).OrderByDescending(x => x.EntryDateTime).ToList();


                if (vstaccesslist != null)
                {
                    //_countvstacs=vstaccesslist.Count;
                    foreach (var item in vstaccesslist)
                    {
                        result.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            Idlst = Convert.ToInt32(item.Id),

                            EntryDateTimelst = item.EntryDateTime,
                            ExitDateTimelst = item.ExitDateTime,
                            VisitTypeNamelst = result.VisitTypeName,

                        });



                    }
                    //result.VisitorAccessLists[0].Countlst = _countvstacs;

                }
                var translist = (vstaccesslist.GroupBy(x => new { _date = x.TransactionDateTime.Value.Date, _vttype = x.VisitTypeName }).Select(x => new { Date = x.Key._date, vstypnm = x.Key._vttype, count = x.Count() })).ToList();
                if (translist != null)
                {
                    result.transLists = new List<TransList>();
                    foreach (var item in translist)
                    {
                        result.transLists.Add(new TransList
                        {

                            Countlst = item.count,
                            Datelst = item.Date,
                            Vsttypnamelst = item.vstypnm
                        });
                        _countvstacs = _countvstacs + item.count;
                    }
                }

                int sum = _countvstacs;
                result.Nooftranscation = sum.ToString();

                result.transLists = result.transLists.OrderByDescending(x => x.Datelst).ToList();
                result.transLists1 = new List<TransList>();
                foreach (var item in result.transLists)
                {
                    result.transLists1.Add(new TransList
                    {
                        Countlst = item.Countlst,
                        Datelst1 = item.Datelst.ToShortDateString(),
                        Vsttypnamelst = item.Vsttypnamelst
                    });
                }




            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Get default line cahrt is getting error");
            }

            return await Task.FromResult<ReportViewModel>(result);
        }

        #endregion

        #region User Audit Trial
        public async Task<List<User>> GetUserAsync(APIRequest request)
        {
            List<User> result = null;
            _logger.LogInformation("Get user list");
            using (var entity = new DataContext())
            {
                try
                {
                    var userlist = entity.Users.Where(x => x.IsDeleted == false);
                    if (userlist != null)
                    {
                        result = userlist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMsg = string.Format("System internal Error. \n{0}", ex.InnerException);
                    _logger.LogError(ex, "Get user list is getting error");
                }
            }
            return await Task.FromResult<List<User>>(result);
        }
        public async Task<ReportViewModel> GetUserVisitorListtAsync(APIRequest req)
        {
            var RegList = new ReportViewModel();
            _logger.LogInformation("Get User Audit Trail list");
            try
            {
                var reg = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.UpdateBy != null).OrderByDescending(x => x.UpdateDateTime).ToList();
                //var reg = entity.VisitorRegistrations.Where(x => x.IsDeleted == false && x.UpdateBy != null).ToList();
                if (reg.Count > 0)
                {
                    foreach (var item in reg)
                    {
                        var loc_ids = item.Locations.Select(y => y.Id).ToList();
                        StringBuilder strlc = new StringBuilder();
                        foreach (var item1 in loc_ids)
                        {

                            strlc.Append(entity.Locations.FirstOrDefault(x => x.Id == Convert.ToInt32(item1)).LocationName);

                            strlc.Append(",");

                        }
                        string _updatetime = string.Empty;
                        string update = string.Empty;

                        //string tdate;
                        if (item.UpdateDateTime != null)
                        {
                            update = Convert.ToDateTime(item.UpdateDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                            _updatetime = (update).ToString();
                        }
                        else
                        {
                            _updatetime = "";
                        }
                        string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var length = decryptedData.Length;
                        var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                        string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                        var mask_data1 = decryptedData1;
                        RegList.VisitorAccessLists.Add(new VisitorAccessList
                        {
                            Idlst = item.Id,
                            VisitorNamelst = item.VisitorName,
                            VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == reg[0].VisitTypeId).VisitTypeName,
                            //LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item.IdType).LocationName,
                            LocationNamelst = strlc.ToString().Trim(','),
                            ModifiedBylst = item.UpdateBy,
                            ModifiedDateTime1lst = _updatetime,
                            NricOrPassportlst = mask_data,
                            Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == reg[0].IdType).Name,
                            NricOrPassport1lst = mask_data1
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User Audit Trail is getting error");
                //LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult(RegList);
        }
        public async Task<ReportViewModel> GetSearchVisitorUseraccessAsync(APIRequest request)
        {

            ReportViewModel result = JsonConvert.DeserializeObject<ReportViewModel>(request.Model.ToString());
            try
            {
                _logger.LogInformation("Search User Audit Trail Details");
                if (result.ModifiedTypeId == 1 && result.UserName != null)
                {
                    var visitordetails = entity.VisitorRegistrations.Include(x => x.Locations).Where(x => x.IsDeleted == false && x.UpdateBy == result.UserName && x.UpdateDateTime.Value.Date >= result.StartDate.Value.Date && x.UpdateDateTime.Value.Date <= result.EndDate.Value.Date && x.UpdateBy != null).OrderByDescending(x => x.UpdateDateTime).ToList();
                    //var visitordetails = entity.VisitorRegistrations.Where(x => x.IsDeleted == false && x.UpdateBy == result.UserName && x.UpdateDateTime.Value.Date >= result.StartDate.Value.Date && x.UpdateDateTime.Value.Date <= result.EndDate.Value.Date && x.UpdateBy!=null).ToList();
                    if (visitordetails.Count > 0)
                    {
                        foreach (var item in visitordetails)
                        {
                            var loc_ids = item.Locations.Select(y => y.Id).ToList();
                            StringBuilder strlc = new StringBuilder();
                            foreach (var item1 in loc_ids)
                            {

                                strlc.Append(entity.Locations.FirstOrDefault(x => x.Id == Convert.ToInt32(item1)).LocationName);

                                strlc.Append(",");
                            }
                            string decryptedData = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var length = decryptedData.Length;
                            var mask_data = new String('*', length - 4) + decryptedData.Substring(length - 4);

                            string decryptedData1 = EncryptionDecryptionSHA256.Decrypt(item.NricOrPassport);
                            var mask_data1 = decryptedData1;
                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = item.Id,
                                VisitorNamelst = item.VisitorName,
                                VisitTypeNamelst = entity.VisitTypes.FirstOrDefault(x => x.Id == item.VisitTypeId).VisitTypeName,
                                // LocationNamelst = entity.Locations.FirstOrDefault(x => x.Id == item.IdType).LocationName,
                                LocationNamelst = strlc.ToString().Trim(','),
                                ModifiedBylst = item.UpdateBy,
                                ModifiedDateTimelst = item.UpdateDateTime,
                                NricOrPassportlst = mask_data,
                                Namelst = entity.VisitorIdentities.FirstOrDefault(x => x.Id == item.IdType).Name,
                                NricOrPassport1lst = mask_data1

                            });
                        }
                    }
                }
                else
                {
                    var userdetails = entity.UsersAudits.Where(x => x.IsDeleted == false && x.UpdateBy == result.UserName && x.UpdateDateTime.Value.Date >= result.StartDate.Value.Date && x.UpdateDateTime.Value.Date <= result.EndDate.Value.Date && x.UpdateBy != null).ToList();
                    if (userdetails.Count > 0)
                    {
                        foreach (var item1 in userdetails)
                        {

                            result.VisitorAccessLists.Add(new VisitorAccessList
                            {
                                Idlst = item1.Id,
                                UserNamelst = item1.UserName,
                                ModifiedDateTimelst = item1.UpdateDateTime,
                                ModifiedBylst = item1.UpdateBy,
                                UserEmaillst = item1.Email,
                                Remarks = item1.Remarks.TrimEnd(','),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = string.Format("System internal error.\n{0}", ex.Message);
                _logger.LogError(ex, "Search user audit trail details is getting error");
            }
            return await Task.FromResult(result);
        }
        #endregion

        #region Login Audit Trail

        public async Task<List<User>> LoginUserAsync(APIRequest request)
        {
            List<User> result = null;

            using (var entity = new DataContext())
            {
                try
                {
                    var userlist = entity.Users.Where(x => x.IsDeleted == false);
                    if (userlist != null)
                    {
                        result = userlist.ToList();
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Instance.LogError(ex);
                }
            }
            return await Task.FromResult<List<User>>(result);
        }

        public async Task<ReportViewModel> LoginUserTrackingAsync(APIRequest req)
        {
            var userlist = new ReportViewModel();
            try
            {
                var today = DateTime.Today;
                DateTime monthStart = new DateTime(today.Year, today.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var usersessiondtls = entity.UsersSessionsTrackings.Where(x => x.AttemptedDateTime.Value.Date >= monthStart.Date && x.AttemptedDateTime <= monthEnd.Date).OrderByDescending(x => x.AttemptedDateTime).ToList();

                if (usersessiondtls.Count > 0)
                {
                    foreach (var item in usersessiondtls)
                    {
                        //DateTime? _entrydate;
                        //DateTime? _exitdate;
                        //string endate;
                        //string eendate;
                        //if (item.LoginDateTime != null)
                        //{
                        //    endate = Convert.ToDateTime(item.LoginDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        //    _entrydate = Convert.ToDateTime(endate);
                        //}
                        //else
                        //{
                        //    _entrydate = null;
                        //}
                        //if (item.LogoutDateTime != null)
                        //{
                        //    eendate = Convert.ToDateTime(item.LogoutDateTime).ToString("dd/MM/yyyy HH:mm:ss tt");
                        //    _exitdate = Convert.ToDateTime(eendate);
                        //}
                        //else
                        //{
                        //    _exitdate = null;
                        //}

                        userlist._usersessiontrackinglist.Add(new UserSessionTrackingList
                        {
                            Id = item.Id,
                            UserName = item.UserName,
                            Status = item.Status,
                            LoginDateTime = item.LoginDateTime,
                            LogoutDateTime = item.LogoutDateTime,
                            AttemptedDateTime = item.AttemptedDateTime,
                            Remarks = item.Remarks,

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }


            return await Task.FromResult(userlist);
        }
        public async Task<ReportViewModel> SearchLoginUserTrackingAsync(APIRequest req)
        {
            ReportViewModel result = JsonConvert.DeserializeObject<ReportViewModel>(req.Model.ToString());
            try
            {
                if (result.LogUserName == "All")
                {
                    var user = entity.UsersSessionsTrackings.Where(x => x.AttemptedDateTime.Value.Date >= result.LFomDate.Value.Date && x.AttemptedDateTime.Value.Date <= result.LToDate.Value.Date).OrderByDescending(x => x.AttemptedDateTime).ToList();
                    if (user.Count > 0)
                    {
                        foreach (var item in user)
                        {
                            result._usersessiontrackinglist.Add(new UserSessionTrackingList
                            {
                                Id = item.Id,
                                UserName = item.UserName,
                                Status = item.Status,
                                LoginDateTime = item.LoginDateTime,
                                LogoutDateTime = item.LogoutDateTime,
                                AttemptedDateTime = item.AttemptedDateTime,
                                Remarks = item.Remarks,
                            });
                        }
                    }
                }
                else
                {
                    var user = entity.UsersSessionsTrackings.Where(x => x.UserName == result.LogUserName && x.AttemptedDateTime.Value.Date >= result.LFomDate.Value.Date && x.AttemptedDateTime.Value.Date <= result.LToDate.Value.Date).OrderByDescending(x => x.AttemptedDateTime).ToList();
                    if (user.Count > 0)
                    {
                        foreach (var item in user)
                        {
                            result._usersessiontrackinglist.Add(new UserSessionTrackingList
                            {
                                Id = item.Id,
                                UserName = item.UserName,
                                Status = item.Status,
                                LoginDateTime = item.LoginDateTime,
                                LogoutDateTime = item.LogoutDateTime,
                                AttemptedDateTime = item.AttemptedDateTime,
                                Remarks = item.Remarks,

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            return await Task.FromResult(result);
        }
        #endregion

    }
}
