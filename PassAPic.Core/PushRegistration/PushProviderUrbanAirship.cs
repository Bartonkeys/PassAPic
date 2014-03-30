using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassAPic.Core.PushRegistration
{
    public class PushProviderUrbanAirship : IPushProvider
    {
        private const String UaPushUrl = "https://go.urbanairship.com/api/push";
        private const String UaAcceptHeader = "application/vnd.urbanairship+json; version=3;";
        private const String UaContentType = "application/json";
        private const String UaAppKey = "ecxPiq1-R0i8m_8fqFoEYw";
        private const String UaAppMasterSecret = "zh2OX5qGRVKtQR8gYfTFVg";
        private const String UaMessageContentType = "text/html";

        void IPushProvider.PushToDevices(List<Data.PushRegister> listOfPushDevices, string pushMessageToSend)
        {
            //We could check for ios and/or droid devices before creating this array - assuming BOTH types for now
            string[] deviceArray = {"ios", "android"};

            //Create a list of all device tokens using Urban Airship object format
            //http://docs.urbanairship.com/reference/api/v3/push.html#push-object
            var deviceTokenOptionList = new List<UrbanAirshipPushObject.UADeviceTokenOption>();
            foreach (var pushDevice in listOfPushDevices)
            {
                switch (pushDevice.DeviceType)
                {
                    case PushRegisterService.DeviceTypeAndroid:
                        deviceTokenOptionList.Add(
                            new UrbanAirshipPushObject.UADeviceTokenOptionAndroid(pushDevice.DeviceToken));
                        break;

                    case PushRegisterService.DeviceTypeIos:
                        deviceTokenOptionList.Add(
                            new UrbanAirshipPushObject.UADeviceTokenOptionIos(pushDevice.DeviceToken));
                        break;
                }

            }

            var urbanAirshipPushObject = new UrbanAirshipPushObject(
                deviceArray,
                new UrbanAirshipPushObject.UAMessage(UaMessageContentType, pushMessageToSend, "It's your turn..."),
                new UrbanAirshipPushObject.UAOptions("2015-04-01T12:00:00"),
                new UrbanAirshipPushObject.UANotification(
                    new UrbanAirshipPushObject.UANotificationIos(
                        new UrbanAirshipPushObject.UAUANotificationIosExtra("http://www.passapic.com")),
                    "It's your turn..."),
                new UrbanAirshipPushObject.UAAudience(deviceTokenOptionList.ToArray()));

            //Try calling the Urban Airship API
            SendObjectAsJson(urbanAirshipPushObject);
        }

        private static void SendObjectAsJson(UrbanAirshipPushObject urbanAirshipPushObject)
        {
            string authInfo = UaAppKey + ":" + UaAppMasterSecret;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(UaPushUrl);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = UaContentType;
            httpWebRequest.Accept = UaAcceptHeader;
            httpWebRequest.Credentials = new NetworkCredential(UaAppKey, UaAppMasterSecret);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = urbanAirshipPushObject.ToJson();
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
        }
    }
}


