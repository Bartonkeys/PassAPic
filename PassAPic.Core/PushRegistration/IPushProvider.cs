using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.PushRegistration
{
    public interface IPushProvider
    {
        void PushToDevices(List<Data.PushRegister> listOfPushDevices, String pushMessageToSend);
    }
}
