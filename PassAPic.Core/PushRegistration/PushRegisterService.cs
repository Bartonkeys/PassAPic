using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Contracts;
using Ninject;

namespace PassAPic.Core.PushRegistration
{
    public class PushRegisterService
    {
        protected IPushProvider PushProvider;

        public const int DeviceTypeIos = 0;
        public const int DeviceTypeAndroid = 1;
        public const int DeviceTypeWindowsPhone = 2;

        public const String ImageGuessPushString = "A user has sent you a new picture to guess!";
        public const String WordGuessPushString = "A user has sent you a new word to draw!";

        public PushRegisterService(IPushProvider pushProvider)
        {
            PushProvider = pushProvider;
        }

        public void SendPush(int userId, String pushMessageToSend, List<Data.PushRegister> listOfPushDevices)
        {
            PushProvider.PushToDevices(listOfPushDevices, pushMessageToSend);
        }
        
    }
}
