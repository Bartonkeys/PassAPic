using System;
using Newtonsoft.Json;

namespace PassAPic.Core.PushRegistration
{
    [Serializable] 
    public class UrbanAirshipPushObject
    {
        public String ToJson()
        {
            return (JsonConvert.SerializeObject(this));
        }

        public UrbanAirshipPushObject(string[] deviceTypes, UAMessage message, UAOptions options, UANotification notification, UAAudience audience)
        {
            device_types = deviceTypes;
            this.message = message;
            this.options = options;
            this.notification = notification;
            this.audience = audience;
        }

        [JsonProperty(PropertyName = "device_types")]
        private String[] device_types { get; set; }

        [JsonProperty(PropertyName = "message")]
        private UAMessage message { get; set; }

        [JsonProperty(PropertyName = "options")]
        private UAOptions options { get; set; }

        [JsonProperty(PropertyName = "notification")]
        private UANotification notification { get; set; }

        [JsonProperty(PropertyName = "audience")]
        private UAAudience audience { get; set; }

        [Serializable]
        public class UAAudience
        {
            public UAAudience(UADeviceTokenOption[] or)
            {
                OR = or;
            }

            [JsonProperty(PropertyName = "OR")]
            private UADeviceTokenOption[] OR { get; set; }
        }

        [Serializable]
        public class UAMessage
        {
            public UAMessage(string content_type, string body, string title)
            {
                this.content_type = content_type;
                this.body = body;
                this.title = title;
            }

            [JsonProperty(PropertyName = "title")]
            private String title { get; set; }

            [JsonProperty(PropertyName = "body")]
            private String body { get; set; }

            [JsonProperty(PropertyName = "content_type")]
            private String content_type { get; set; }
        }

        [Serializable]
        public class UAOptions
        {
            public UAOptions(string expiry)
            {
                this.expiry = expiry;
            }

            [JsonProperty(PropertyName = "expiry")]
            private String expiry { get; set; }
        }

        [Serializable]
        public class UANotification
        {
            public UANotification(UANotificationIos ios, string alert)
            {
                this.ios = ios;
                this.alert = alert;
            }

            [JsonProperty(PropertyName = "alert")]
            private String alert { get; set; }

            [JsonProperty(PropertyName = "ios")]
            private UANotificationIos ios { get; set; }

        }

        [Serializable]
        public class UANotificationIos
        {
            public UANotificationIos(UAUANotificationIosExtra extra)
            {
                this.extra = extra;
            }

            [JsonProperty(PropertyName = "extra")]
            private UAUANotificationIosExtra extra { get; set; }
        }

        [Serializable]
        public class UAUANotificationIosExtra
        {
            public UAUANotificationIosExtra(string url)
            {
                this.url = url;
            }

            [JsonProperty(PropertyName = "url")]
            private String url { get; set; }
        }

        [Serializable]
        public abstract class UADeviceTokenOption
        {

        }

        [Serializable]
        public class UADeviceTokenOptionIos : UADeviceTokenOption
        {
            public UADeviceTokenOptionIos(string deviceToken)
            {
                device_token = deviceToken;
            }

            [JsonProperty(PropertyName = "device_token")]
            private String device_token { get; set; }
        }

        [Serializable]
        public class UADeviceTokenOptionAndroid : UADeviceTokenOption
        {
            [JsonProperty(PropertyName = "apid")]
            private String apid { get; set; }

            public UADeviceTokenOptionAndroid(string apid)
            {
                this.apid = apid;
            }
        }
    }
}