using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.DAL.Interface;
using ATTSystems.SFA.DAL;
using ATTSystems.SFA.Model.HttpModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ATTSystems.NetCore.Model.DBModel;
using ATTSystems.SFA.Model.DBModel;

namespace ATTSystems.SAF.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemacAccessController : ControllerBase
    {

        private ISemacAccess _isemac;
        private IBaseRepository _ibaseReposit;
        private ILogger<SemacAccessController> _logger;
        public SemacAccessController(ISemacAccess isemac, IBaseRepository baseRepository, ILogger<SemacAccessController> logger)
        {
            _isemac = isemac;
            _ibaseReposit = baseRepository;
            _logger = logger;
        }

        [HttpPost]
        [Route("InsertEntry")]
        public async Task<IActionResult> InsertEntry(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.InsertEntry(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Visitor Pusched Succesfully to AWS";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: InsertEntry, Message: {0} ", ex.Message));

            }
            return Ok(result);
        }
        [HttpPost]
        [Route("UpdateExit")]
        public async Task<IActionResult> UpdateExit(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.UpdateExit(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Visitor Pusched Succesfully to AWS";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: UpdateExit, Message: {0} ", ex.Message));

            }
            return Ok(result);
        }
        [HttpPost]
        [Route("TerminalStatus")]
        public async Task<IActionResult> TerminalStatus(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.UpdateTerminalStatus(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Terminal status updated Succesfully to AWS";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: TerminalStatus, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("Verification")]
        public async Task<IActionResult> Verification(AuthRequest request)
        {
            AuthTokenResponse result = new AuthTokenResponse();
            try
            {
                TokenAuth? auth = await _isemac.GetUserAsync(request);
                if (auth != null)
                {
                    result.resultId = auth.Id;
                    string enc = EncryptionDecryptionSHA256.Encrypt(request.UserCode);
                    string decryptCode = EncryptionDecryptionSHA256.Decrypt(auth.PasswordHash);
                    if (decryptCode == request.UserCode)
                    {
                        _logger.LogInformation("Token Authentication");
                        DateTime now = DateTime.Now;

                        // Generate Token
                        string key = string.Format("{0}{1}", now.ToString("HHmmss"), now.ToString("yyyyMMdd"));
                        key = ATTSystems.NetCore.CryptographyHash.ConvertToSHA256(key);

                        // Get Expiry duration in min
                        int tokMin = 20;
                        Setting? setting = await _isemac.GetExpiryDurationAsync();
                        if (setting != null)
                        {
                            int tmp = 0;
                            if (int.TryParse(setting.Value, out tmp))
                            {
                                tokMin = tmp > 0 ? tmp : tokMin;
                            }
                        }
                        auth.Token = key;
                        auth.TokenRequestDateTime = now;
                        auth.TokenExpiryDateTime = now.AddMinutes(tokMin);
                        int res = await _isemac.UpdateTokenAuth(auth);

                        result.AccessToken = key;
                        result.resultCode = 200;
                        result.resultDescription = "";
                    }
                    else
                    {
                        result.resultCode = 303;
                        result.resultDescription = "303 Invalid Code.";
                        _logger.LogInformation("303 Invalid Code.");
                    }
                }
                else
                {
                    result.resultCode = 302;
                    result.resultId = 0;
                    result.resultDescription = "302 Key Not found Key and Code.";
                    _logger.LogInformation("302 Key Not found Key and Code.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Verification");
            }
            return Ok(result);
        }


        [HttpPost]
        [Route("PushVisitor")]
        public async Task<IActionResult> PushVisitor(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.PushVisitorAsync(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Visitor added Succesfully to cloud";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: PushVisitor, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("PushCardDetails")]
        public async Task<IActionResult> PushCardDetails(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.PushCardDetailsAsync(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Carddetails added Succesfully to cloud";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: PushCardDetails, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("PushOverStayerVisitors")]
        public async Task<IActionResult> PushOverStayerVisitors(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.PushOverStayerAsync(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Overstayer pushed Succesfully to cloud";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: PushOverStayerVisitors, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }
        [HttpPost]
        [Route("PushVisitorLocations")]
        public async Task<IActionResult> PushVisitorLocations(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.PushVisitorLocationsAsync(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Visitor locations pushed Succesfully to cloud";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: PushOverStayerVisitors, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("PushMessageLogs")]
        public async Task<IActionResult> PushMessageLogs(APIRequest request)
        {
            AppResponse result = new AppResponse();
            try
            {
                var res = await _isemac.PushMessageLogsAsync(request);
                if (res == 200)
                {
                    result.Succeeded = true;
                    result.resultDescription = "Message logs pushed Succesfully to cloud";
                    result.Code = 200;
                }
                else if (res == 301)
                {
                    result.resultCode = 301;
                    result.resultDescription = "301 - Invalid Access.";
                }
                else if (res == 302)
                {
                    result.resultCode = 302;
                    result.resultDescription = "302 - Invalid User Access.";
                }
                else if (res == 303)
                {
                    result.resultCode = 303;
                    result.resultDescription = "303 - Expired Access.";
                }

            }
            catch (Exception ex)
            {
                result.resultCode = 300;
                result.resultDescription = ex.Message;
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: API, Controller: SemacAccess, Action: PushMessageLogs, Message: {0} ", ex.Message));
            }
            return Ok(result);
        }
    }
}

