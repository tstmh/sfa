namespace ATTSystems.SFA.Web.Helper
{
    using ATTSystems.NetCore.Logger;
    using ATTSystems.NetCore.Model.HttpModel;
    using ATTSystems.NetCore.Model.ViewModel;
    using ATTSystems.NetCore.Utilities;
    using ATTSystems.SFA.Model.HttpModel;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public class WebAPIHelper
    {           
        public static async Task<APIResponse?> APIRequestAsync(string baseuri, string relativeUri, APIRequest request)
        {
            APIResponse? result = null;

            try
            {
                WebAPIClient webApiClient = new WebAPIClient();


                HttpResponseMessage response = await webApiClient.PostAsync(baseuri, relativeUri, JsonConvert.SerializeObject(request));

                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<APIResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, ClassType: WebAPIHelper, Method: APIRequestAsync, Message: {0} ", ex.Message));

                result = null;
            }

            return result;
        }

        public static async Task<AuthResponse?> UserAuthAsync(string relativeUri, LoginViewModel loginViewModel)
        {
            AuthResponse? result = null;
            try
            {
                WebAPIClient webApiClient = new WebAPIClient();

                var response = await webApiClient.PostAsync(ConfigurationManager.AppSetting["AppSettings:APIBaseUri"], relativeUri, JsonConvert.SerializeObject(loginViewModel));

                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<AuthResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, ClassType: WebAPIHelper, Method: UserAuthAsync, Message: {0} ", ex.Message));
                result = null;
            }

            return result;

        }

        public static async Task<APIResponse?> ChangePasswordAsync(string relativeUri, ChangePasswordViewModel model)
        {
            APIResponse? result = null;

            try
            {
                WebAPIClient webApiClient = new WebAPIClient();
                var response = await webApiClient.PostAsync(ConfigurationManager.AppSetting["APIBaseUri"], relativeUri, JsonConvert.SerializeObject(model));

                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<APIResponse>(response.Content.ReadAsStringAsync().Result);

            }
            catch (Exception ex)
            {
                  LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, ClassType: WebAPIHelper, Method: ChangePasswordAsync, Message: {0} ", ex.Message));
                result = null;
            }

            return result;
        }

        public static async Task<AppResponse?> AppRequestAsync(string relativeUri, APIRequest request)
        {
            AppResponse? result = null;

            try
            {
                WebAPIClient webApiClient = new WebAPIClient();
                var response = await webApiClient.PostAsync(ConfigurationManager.AppSetting["AppSettings:APIBaseUri"], relativeUri, JsonConvert.SerializeObject(request));

                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }

                result = JsonConvert.DeserializeObject<AppResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                 LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, ClassType: WebAPIHelper, Method: APIRequestAsync - AppResponse, Message: {0} ", ex.Message));

                result = null;
            }

            return result;
        }
    }
}


