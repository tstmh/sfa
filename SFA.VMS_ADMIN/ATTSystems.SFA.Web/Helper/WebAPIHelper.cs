namespace ATTSystems.SFA.Web.Helper
{
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
            catch 
            {
                result = null;
            }
            return result;
        }

        public static async Task<AuthResponse?> UserAuthAsync(string relativeUri, /*LoginViewModel loginViewModel*/ APIRequest request)
        {
            AuthResponse? result = null;
            try
            {
                WebAPIClient webApiClient = new WebAPIClient();
                string? ss = ConfigurationManager.AppSetting["AppSettings:APIBaseUri"];

                var response = await webApiClient.PostAsync(ConfigurationManager.AppSetting["AppSettings:APIBaseUri"], relativeUri, JsonConvert.SerializeObject(/*loginViewModel*/request));

                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }
                result = JsonConvert.DeserializeObject<AuthResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        public static async Task<AppResponse?> ChangePasswordAsync(string baseuri, string relativeUri, APIRequest request)
        {
            AppResponse? result = null;
            try
            {
                WebAPIClient webApiClient = new WebAPIClient();
                var response = await webApiClient.PostAsync(baseuri, relativeUri, JsonConvert.SerializeObject(request));
                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }
                result = JsonConvert.DeserializeObject<AppResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
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
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        public static async Task<APIResponse?> Addasync(string relativeUri, /*UserViewModel model*/ APIRequest request)
        {
            APIResponse? result = null;
            try
            {
                WebAPIClient webApiClient = new WebAPIClient();
                var response = await webApiClient.PostAsync(ConfigurationManager.AppSetting["AppSettings:APIBaseUri"], relativeUri, JsonConvert.SerializeObject(request));
                if ((response == null) || (response.IsSuccessStatusCode == false))
                {
                    return null;
                }
                result = JsonConvert.DeserializeObject<APIResponse>(response.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
    }
}


