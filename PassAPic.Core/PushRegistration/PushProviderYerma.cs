using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassAPic.Core.PushRegistration
{
    public class PushProviderYerma : IPushProvider
    {
        private const String YmPushUrl = "http://ympushserver.cloudapp.net/PushQueue";//"http://127.0.0.1:81/PushQueue"; //
        private const String YmContentType = "application/json";
        private const int DeviceTypeIos = 0;
        private const int DeviceTypeAndroid = 1;
        //private const String UaAppKey = "ecxPiq1-R0i8m_8fqFoEYw";
        //private const String UaAppMasterSecret = "zh2OX5qGRVKtQR8gYfTFVg";
        //private const String UaMessageContentType = "text/html";

        async Task<string> IPushProvider.PushToDevices(int id, List<PushQueueMember> memberList, string pushMessageToSend)
        {
            
            var ymPushObject = new YmPushObject()
            {
                Id = id,
                PushApplicationGuid =
                    Guid.Parse(System.Configuration.ConfigurationSettings.AppSettings["PushApplicationGuid"]),
                MemberList = memberList,
                PushMessage = pushMessageToSend
            };
           
            return await SendObjectAsJson(ymPushObject);
        }

        private static async Task<string> SendObjectAsJson(YmPushObject ymPushObject)
        {
            
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(YmPushUrl);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = YmContentType;
            
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = await Task.Run(() => JsonConvert.SerializeObject(ymPushObject));
                await streamWriter.WriteAsync(json);
                await streamWriter.FlushAsync();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = await streamReader.ReadToEndAsync();
                    return result;
                }
            }
        }
    }
}
