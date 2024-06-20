using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;
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
    public class SemacAccessRepository : IBaseRepository, IDisposable, ISemacAccess
    {
        private string ErrorMsg = string.Empty;
        private DataContext entity = null;
        private ILogger<SemacAccessRepository> logger = null;
        public SemacAccessRepository(DataContext _entity, ILogger<SemacAccessRepository> logger)
        {
            this.logger = logger;
            entity = _entity;
        }
        ~SemacAccessRepository()
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
        public async Task<int> InsertEntry(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Insrting visitor entry..");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    logger.LogInformation("Request string:" + request.RequestString);
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        logger.LogInformation("Auth token:" + auth.Token);
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            logger.LogInformation("Token expiry:" + auth.TokenExpiryDateTime.ToString());
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                foreach (VisitorTransactionList item in model.visitorTransactions)
                                {
                                    var vrg = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport);
                                    if (vrg != null)
                                    {
                                        logger.LogInformation("Visitor present in visitor registration");
                                        VisitorTransaction? vt = entity.VisitorTransactions.FirstOrDefault(x => x.TransId == item.TransID && x.LocationId == item.LocationId && x.Flag==item.Flag);
                                        if (vt == null)
                                        {
                                            VisitorTransaction visitor = new VisitorTransaction();
                                            visitor.TransactionType = item.TransactionType;
                                            visitor.NricOrPassport = item.NricOrPassport;
                                            visitor.TransactionDateTime = item.TransactionDateTime;
                                            visitor.EntryDateTime = item.EntryDateTime;
                                            visitor.EntryDoor = item.EntryDoor;
                                            visitor.EntryDateTime = item.EntryDateTime;
                                            visitor.EntryPushed = true;
                                            visitor.ExitPushed = false;
                                            visitor.LocationId = item.LocationId;
                                            visitor.EntryTerminalId = item.EntryTerminalId;
                                            visitor.TransId = item.TransID;
                                            visitor.Flag = item.Flag;
                                            entity.VisitorTransactions.Add(visitor);
                                            entity.SaveChanges();
                                            logger.LogInformation("Visitor entry saved");
                                            result = 200;
                                        }
                                        else
                                        {

                                            vt.TransactionType = item.TransactionType;
                                            vt.NricOrPassport = item.NricOrPassport;
                                            vt.TransactionDateTime = item.TransactionDateTime;
                                            vt.EntryDateTime = item.EntryDateTime;
                                            vt.EntryDoor = item.EntryDoor;
                                            vt.EntryDateTime = item.EntryDateTime;
                                            vt.EntryPushed = true;
                                            // vt.ExitPushed = false;
                                            vt.LocationId = item.LocationId;
                                            vt.EntryTerminalId = item.EntryTerminalId;
                                            vt.TransId = item.TransID;
                                            vt.Flag = item.Flag;
                                            //entity.VisitorTransactions.Add(visitor);
                                            entity.SaveChanges();
                                            logger.LogInformation("Visitor entry saved");
                                            result = 200;

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor entry method");
            }
            return await Task.FromResult<int>(result);
        }
        public async Task<int> UpdateExit(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("updating visitor exit..");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    logger.LogInformation("query string:" + request.RequestString);
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        logger.LogInformation("Auth token:" + auth.Token);
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            logger.LogInformation("token expiry date:" + auth.TokenExpiryDateTime.ToString());
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel>(request.Model.ToString());
                            if (model != null)
                            {
                                logger.LogInformation("Model data");
                                foreach (VisitorTransactionList item in model.visitorTransactions)
                                {
                                    VisitorTransaction? visitor = entity.VisitorTransactions.FirstOrDefault(x => x.TransId == item.TransID && x.LocationId == item.LocationId && x.Flag==item.Flag);
                                    if (visitor != null)
                                    {
                                        logger.LogInformation("visitor present");
                                        visitor.TransactionType = item.TransactionType;
                                        visitor.TransactionDateTime = item.TransactionDateTime;
                                        visitor.ExitDateTime = item.ExitDateTime;
                                        visitor.ExitDoor = item.ExitDoor;
                                        visitor.ExitTerminalId = item.ExitTerminalId;
                                        visitor.LocationId = item.LocationId;
                                        visitor.TransId = item.TransID;
                                        visitor.ExitPushed = true;
                                        visitor.Flag = item.Flag;
                                        entity.SaveChanges();
                                        logger.LogInformation("updated visitor exit..");
                                        result = 200;
                                    }
                                    else
                                    {
                                        VisitorTransaction vt = new VisitorTransaction();
                                        vt.TransactionType = item.TransactionType;
                                        vt.TransactionDateTime = item.TransactionDateTime;
                                        vt.NricOrPassport = item.NricOrPassport;
                                        vt.ExitDateTime = item.ExitDateTime;
                                        vt.ExitDoor = item.ExitDoor;
                                        vt.ExitTerminalId = item.ExitTerminalId;
                                        vt.ExitPushed = true;
                                        vt.LocationId = item.LocationId;
                                        vt.TransId = item.TransID;
                                        vt.Flag = item.Flag;
                                        entity.VisitorTransactions.Add(vt);
                                        entity.SaveChanges();
                                        logger.LogInformation("updated visitor exit..");

                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Update Exit method");
            }
            return await Task.FromResult<int>(result);
        }
        public async Task<int> UpdateTerminalStatus(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("updating terminal status..");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? term = JsonConvert.DeserializeObject<VisitorViewModel>(request.Model.ToString());
                            if (term != null)
                            {
                                foreach (var item in term.terminals)
                                {
                                    Terminal? _term = entity.Terminals.FirstOrDefault(x => x.TerminalId == item.TerminalId);
                                    if (_term != null)
                                    {
                                        _term.IsOnline = item.IsOnline;
                                        entity.SaveChanges();
                                        logger.LogInformation("updated terminal status..Terminal:{0}", item.TerminalId);
                                        result = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdateTerminalStatus");
            }
            return await Task.FromResult<int>(result);
        }
        public async Task<TokenAuth?> GetUserAsync(AuthRequest request)
        {

            string result = string.Empty;
            TokenAuth? tokenAuth = new TokenAuth();
            try
            {
                logger.LogInformation("getting user info to generate token");
                using (var entity = new DataContext())
                {
                    if (!string.IsNullOrEmpty(request.UserKey))
                    {
                        tokenAuth = entity.TokenAuths.FirstOrDefault(x => x.UserKey == request.UserKey);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error in getting user info to generate token");
            }

            return await Task.FromResult<TokenAuth?>(tokenAuth);
        }
        public async Task<Setting?> GetExpiryDurationAsync()
        {
            Setting? setting = null;
            try
            {
                logger.LogInformation("getting token expire duration");
                setting = entity.Settings.FirstOrDefault(x => x.Field == "AccessTokenMin");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "error in getting token expire duration.");
            }
            return await Task.FromResult(setting);
        }
        public async Task<int> UpdateTokenAuth(TokenAuth? tAuth)
        {
            int result = 0;
            try
            {
                if (tAuth != null)
                {
                    logger.LogInformation("Update token details");
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Id == tAuth.Id);
                    if (auth != null)
                    {
                        auth.Token = tAuth.Token;

                        auth.TokenRequestDateTime = tAuth.TokenRequestDateTime;
                        auth.TokenExpiryDateTime = tAuth.TokenExpiryDateTime;
                        entity.SaveChanges();
                        result = 1;
                        logger.LogInformation("Token details are updated");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Update token details error");
            }
            return await Task.FromResult<int>(result);
        }

        /*public async Task<int> PushVisitorAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Insrting visitor...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorRegistration? model = JsonConvert.DeserializeObject<VisitorRegistration?>(request.Model.ToString());
                            if (model != null)
                            {
                                VisitorRegistration? vr = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport != model.NricOrPassport);
                                if (vr == null)
                                {
                                    VisitorRegistration? visitor = new VisitorRegistration();
                                    visitor = model;
                                    entity.VisitorRegistrations.Add(visitor);
                                    entity.SaveChanges();
                                    logger.LogInformation("Visitor synch success,name:{0}", model.VisitorName);
                                    result = 200;
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor Synch method");
            }
            return await Task.FromResult<int>(result);
        }
        public async Task<int> PushCardDetailsAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Insrting Card issue details...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            CardIssueDetail? model = JsonConvert.DeserializeObject<CardIssueDetail?>(request.Model.ToString());
                            if (model != null)
                            {
                                CardIssueDetail? vr = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport != model.NricOrPassport && x.IsActive == true);
                                if (vr == null)
                                {
                                    CardIssueDetail? card = new CardIssueDetail();
                                    card = model;
                                    entity.CardIssueDetails.Add(card);
                                    entity.SaveChanges();
                                    logger.LogInformation("Card details synch success,NRIC_Passport:{0}", model.NricOrPassport);
                                    result = 200;
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor Synch method");
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<int> PushOverStayerAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("updating overstay visitor...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorRegistration? model = JsonConvert.DeserializeObject<VisitorRegistration?>(request.Model.ToString());
                            if (model != null)
                            {
                                VisitorRegistration? vr = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == model.NricOrPassport);
                                if (vr != null)
                                {
                                    vr.OverStayer = true;
                                    entity.VisitorRegistrations.Add(vr);
                                    entity.SaveChanges();
                                    logger.LogInformation("Overstay visitor synch success,name:{0}", model.VisitorName);
                                    result = 200;
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor Synch method");
            }
            return await Task.FromResult<int>(result);
        }*/


        public async Task<int> PushVisitorAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Insrting visitor...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                foreach (var item in model.visitorsLists)
                                {

                                    VisitorRegistration? vr = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport);
                                    if (vr == null)
                                    {
                                        VisitorRegistration? visitor = new VisitorRegistration();

                                        visitor.IdType = item.IdType;
                                        visitor.VisitorName = item.VisitorName;
                                        visitor.NricOrPassport = item.NricOrPassport;
                                        visitor.VehicleNo = item.VehicleNo;
                                        visitor.CompanyName = item.CompanyName;
                                        visitor.VisitTypeId = item.VisitTypeId;
                                        visitor.BlockNo = item.BlockNo;
                                        visitor.VisitorContanctNo = item.VisitorContanctNo;
                                        visitor.CreateDateTime = item.CreateDateTime;
                                        visitor.CreateBy = item.CreateBy;
                                        visitor.VisitorStatus = item.VisitorStatus;
                                        visitor.Email = item.Email;
                                        visitor.UnitNo = item.UnitNo;
                                        visitor.IsBlockListed = item.IsBlockListed;
                                        visitor.VistStartDateTime = item.VistStartDateTime;
                                        visitor.VistEndDateTime = item.VistEndDateTime;
                                        visitor.ManualCheckIn = item.ManualCheckIn;
                                        visitor.UploadtoController = item.UploadtoController;
                                        visitor.RegistrationBy = item.RegistrationBy;
                                        visitor.PushVisitors = true; //Modified By Sanju 20240222
                                        visitor.Locations.Clear();
                                        foreach (var locat in item.locationsid.Split(','))
                                        {
                                            int locid = Convert.ToInt32(locat);
                                            Location? loc = entity.Locations.FirstOrDefault(x => x.Id == locid);
                                            if (loc != null)
                                            {
                                                visitor.Locations.Add(loc);
                                            }
                                        }
                                        entity.VisitorRegistrations.Add(visitor);
                                        entity.SaveChanges();
                                        logger.LogInformation("Visitor synch success,name:{0}", item.VisitorName);
                                        result = 200;
                                    }
                                    else
                                    {
                                        vr.IdType = item.IdType;
                                        vr.VisitorName = item.VisitorName;
                                        vr.NricOrPassport = item.NricOrPassport;
                                        vr.VehicleNo = item.VehicleNo;
                                        vr.CompanyName = item.CompanyName;
                                        vr.VisitTypeId = item.VisitTypeId;
                                        vr.BlockNo = item.BlockNo;
                                        vr.VisitorContanctNo = item.VisitorContanctNo;
                                        vr.CreateDateTime = item.CreateDateTime;
                                        vr.CreateBy = item.CreateBy;
                                        vr.VisitorStatus = item.VisitorStatus;
                                        vr.Email = item.Email;
                                        vr.UnitNo = item.UnitNo;
                                        vr.IsBlockListed = item.IsBlockListed;
                                        vr.VistStartDateTime = item.VistStartDateTime;
                                        vr.VistEndDateTime = item.VistEndDateTime;
                                        vr.ManualCheckIn = item.ManualCheckIn;
                                        vr.UploadtoController = item.UploadtoController;
                                        vr.RegistrationBy = item.RegistrationBy;
                                        vr.PushVisitors = true; //Modified By Sanju 20240222
                                        vr.Locations.Clear();
                                        foreach (var locat in item.locationsid.Split(','))
                                        {
                                            int locid = Convert.ToInt32(locat);
                                            Location? loc = entity.Locations.FirstOrDefault(x => x.Id == locid);
                                            if (loc != null)
                                            {
                                                vr.Locations.Add(loc);
                                            }
                                        }

                                        entity.SaveChanges();
                                        logger.LogInformation("Visitor synch success,name:{0}", item.VisitorName);
                                        result = 200;

                                    }

                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor Synch method");
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<int> PushVisitorLocationsAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Push Visitor location...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                List<int> c = new List<int>();
                                foreach (var item in model.visitorsLists)
                                {
                                    List<int> a = new List<int>();
                                    List<int> b = new List<int>();
                                    VisitorRegistration? vt = entity.VisitorRegistrations.Include(x => x.Locations).FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport);
                                    if (vt != null)
                                    {
                                        foreach (var lc in vt.Locations)
                                        {
                                            a.Add(lc.Id);
                                        }
                                        foreach (var l in item.locationsid.Split(','))
                                        {
                                            int l1 = Convert.ToInt32(l);
                                            if (!a.Contains(l1))
                                            {
                                                Location? lcn = entity.Locations.FirstOrDefault(x => x.Id == l1);
                                                if (lcn != null)
                                                {
                                                    vt.Locations.Add(lcn);
                                                    entity.SaveChanges();
                                                    result = 200;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor location Synch method");
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<int> PushCardDetailsAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Insrting Card issue details...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                foreach (var item in model.cards)
                                {
                                    CardIssueDetail? vr = entity.CardIssueDetails.FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport && x.IsActive == true);
                                    if (vr == null)
                                    {
                                        CardIssueDetail? card = new CardIssueDetail();
                                        card.CardNumber = item.CardNumber;
                                        card.IssueDate = item.IssueDate;
                                        card.IsActive = item.IsActive;
                                        card.CreateDateTime = item.CreateDateTime;
                                        card.NricOrPassport = item.NricOrPassport;
                                        card.IsPushed = item.IsPushed;
                                        entity.CardIssueDetails.Add(card);
                                        entity.SaveChanges();
                                        logger.LogInformation("Card details synch success,NRIC_Passport:{0}", item.NricOrPassport);
                                        result = 200;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Card Issue details Synch method");
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<int> PushOverStayerAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("updating overstay visitor...");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                foreach (var item in model.visitorsLists)
                                {
                                    VisitorRegistration? vr = entity.VisitorRegistrations.FirstOrDefault(x => x.NricOrPassport == item.NricOrPassport);
                                    if (vr != null)
                                    {
                                        vr.OverStayer = true;
                                        entity.SaveChanges();
                                        logger.LogInformation("Overstay visitor synch success,name:{0}", item.VisitorName);
                                        result = 200;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in Visitor Synch method");
            }
            return await Task.FromResult<int>(result);
        }

        public async Task<int> PushMessageLogsAsync(APIRequest request)
        {
            int result = 0;
            try
            {
                logger.LogInformation("Pushing MessageLogs");
                if (!string.IsNullOrEmpty(request.RequestString))
                {
                    TokenAuth? auth = entity.TokenAuths.FirstOrDefault(x => x.Token == request.RequestString.ToString());
                    if (auth != null)
                    {
                        if (auth.TokenExpiryDateTime > DateTime.Now)
                        {
                            VisitorViewModel? model = JsonConvert.DeserializeObject<VisitorViewModel?>(request.Model.ToString());
                            if (model != null)
                            {
                                foreach (var item in model.LogLists)
                                {
                                    MessageLogs? vr = entity.MessageLogs.FirstOrDefault(x => x.CardNumber == item.CardNumber && x.SentStatus == 0);
                                    if (vr == null)
                                    {
                                        MessageLogs msg = new MessageLogs();
                                        msg.CardNumber = item.CardNumber;
                                        msg.SentStatus = item.SentStatus;
                                        msg.Message = item.Message;
                                        msg.Recipient = item.Recipient;
                                        msg.IsPushed = true;
                                        entity.MessageLogs.Add(msg);
                                        entity.SaveChanges();
                                        logger.LogInformation("Messagelog synch success,name:{0}", item.Recipient);
                                        result = 200;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = 303;
                        }
                    }
                    else
                    {
                        result = 302;
                    }
                }
                else
                {
                    result = 301;
                }
            }
            catch (Exception ex)
            {
                result = 300;
                logger.LogError(ex, "Error in MessageLog Synch method");
            }
            return await Task.FromResult<int>(result);
        }
    }
}
