using Organilog.IServices;
using System;
using Xamarin.Forms;

namespace Organilog.Constants
{
    public static class ApiURI
    {
        public static string APP_VERSION_NUMBER => "3.0";

        public static string APP_VERSION = "v3.0";
        public static string API_MOBILE_TO_SERVER_VERSION = "10";
        public static string API_MOBILE_TO_SERVER_MEDIA_VERSION = "11";
        public static string API_SERVER_TO_MOBILE_VERSION = "17";
        public static string API_SERVER_TO_MOBILE_INVOICE_VERSION = "2";
        public static string API_SERVER_TO_MOBILE_ISSUE_VERSION = "2";
        public static string API_SERVER_TO_MOBILE_PRODUCT_VERSION = "9";
        public static string API_HISTORY_VERSION = "1";


        private static readonly int deviceType = Device.RuntimePlatform == Device.Android ? 1 : 2;

        private static string version_os = Xamarin.Essentials.DeviceInfo.Version.ToString();

        private static IDevice device = Device.Android == Device.RuntimePlatform? DependencyService.Get<IDevice>(): null;
        //string deviceIdentifier = device.GetIdentifier();

        private static string uid = device!=null ? device.GetIdentifier(): "3042342242";
        
        public static string URL_BASE(string account) => string.Format("http://{0}.organilog.com/script/api/", account);

        public static string URL_BASE_ROOT(string account) => string.Format("http://{0}.organilog.com/", account);

        public static string URL_GET_LOGIN(string userName, string password) =>
            string.Format("get-login.php?user_name={0}&device_type={1}&os={2}&uid={3}&password={4}", userName,deviceType,version_os,uid, password);

        public static string URL_GET_SYNC(string userName, string password, string lastSync = "0") =>
            string.Format("get-sync.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&wifi=1&last_synchro={4}&os={5}&password={6}",
                userName, deviceType, API_SERVER_TO_MOBILE_VERSION, APP_VERSION, lastSync,version_os,password);

        public static string URL_GET_MEDIA(string account, int accountId, int userId, string mediaYear, string mediaMonth, string mediaName) =>
            string.Format("http://{0}.organilog.com/media/{1}/{2}/{3}/{4}/{5}", account, accountId, userId, mediaYear, mediaMonth, mediaName);

        public static string URL_SET_MEDIA(string account, string userName, string password, int method, string mediaInfo) =>
            string.Format("http://{0}.organilog.com/script/api/set-media.php?user_name={1}&api_version={2}&appVersion={3}&sMethod={4}{5}&os={6}&password={7}",
                account, userName,  API_MOBILE_TO_SERVER_MEDIA_VERSION, APP_VERSION, method, mediaInfo,version_os, password);

        public static string URL_PDF(string account, int intId, string nonce) =>
            string.Format("http://{0}.organilog.com/intervention_view.php?id={1}&nonce={2}", account, intId, nonce);

        public static string URL_SET_SYNC(string userName, string password, int method, int netWorkStatus = 1) =>
            string.Format("set-sync.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&sMethod={4}&wifi={5}&os={6}&format=json&password={7}",
                userName, deviceType, API_MOBILE_TO_SERVER_VERSION, APP_VERSION, method, netWorkStatus,version_os, password);

        public static string URL_SET_TRACKING(string userName, string password) =>
            string.Format("set-tracking.php?user_name={0}&api_version={1}&appVersion={2}&os={3}&password={4}",
                userName, 1, APP_VERSION,version_os, password);

        public static string URL_SET_GEOLOC(string userName, string password) =>
            string.Format("set-geoloc.php?user_name={0}&api_version={1}&appVersion={2}&os={3}&password={4}",
                userName, 3, APP_VERSION,version_os, password);
        
        public static string URL_GET_INTERVENTION_HISTORY(string userName, string password, int intId) =>
            string.Format("get-interventions-historique-sync.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&&int_id={4}&os={5}&password={6}",
                userName, deviceType, API_HISTORY_VERSION, APP_VERSION, intId,version_os, password);

        public static string URL_SET_INTERVENTION_ASSIGN(string userName, string password, int intId) =>
            string.Format("set-intervention-assignation.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&&intervention_id={4}&os={5}&password={6}",
                userName, deviceType, API_HISTORY_VERSION, APP_VERSION, intId,version_os, password);

        public static string URL_GET_PRODUCTS(string userName, string password, string lastSync = "0",int page=0) =>
            string.Format("get-products.php?user_name={0}&device_type=${1}&api_version=${2}&last_synchro={3}&page_nbr={4}&os={5}&password={6}",
                userName, deviceType, API_SERVER_TO_MOBILE_VERSION, lastSync,page,version_os, password);

        public static string URL_GET_PRODUCT_BY_NAME(string userName, string password, string nom) =>
            string.Format("get-product-by-name.php?user_name={0}&device_type=${1}&name=${2}&os={3}&password={4}",
                userName, deviceType, nom,version_os, password);

        public static string URL_GET_INVOICE(string userName, string password, string lastSync = "0",int page = 0) =>
            string.Format("get-invoices-api.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&wifi=1&last_synchro={4}&page={5}&os={6}&password={7}",
                userName, deviceType, API_SERVER_TO_MOBILE_INVOICE_VERSION, APP_VERSION, lastSync,page,version_os, password);

        public static string URL_SET_INVOICE(string userName, string password) =>
            string.Format("set-invoices-api.php?user_name={0}&api_version={1}&appVersion={2}&os={3}&password={4}",
                userName, API_MOBILE_TO_SERVER_VERSION, APP_VERSION,version_os, password);

        public static string URL_GET_ISSUE(string userName, string password,int page, string lastSync = "0") =>
            string.Format("get-issues-api.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&wifi=1&last_synchro={4}&page={5}&os={6}&password={7}",
                userName, deviceType, API_SERVER_TO_MOBILE_ISSUE_VERSION, APP_VERSION, lastSync,page,version_os, password);

        public static string URL_SET_ISSUE(string userName, string password) =>
            string.Format("set-issues-api.php?user_name={0}&api_version={1}&appVersion={2}&os={3}&password={4}",
                userName, API_SERVER_TO_MOBILE_ISSUE_VERSION, APP_VERSION,version_os, password);

        public static string URL_SET_EQUIPMENT(string userName, string password) =>
           string.Format("set-equipement-api.php?user_name={0}&api_version={1}&appVersion={2}&os={3}&password={4}",
               userName, API_MOBILE_TO_SERVER_VERSION, APP_VERSION, version_os, password);

        public static string URL_GET_EQUIPMENT(string userName, string password, int page, string lastSync = "0") =>
            string.Format("get-equipements-api.php?user_name={0}&device_type={1}&api_version={2}&appVersion={3}&wifi=1&last_synchro={4}&page={5}&os={6}&password={7}",
                userName, deviceType, API_SERVER_TO_MOBILE_INVOICE_VERSION, APP_VERSION, lastSync, page, version_os, password);


    }
}