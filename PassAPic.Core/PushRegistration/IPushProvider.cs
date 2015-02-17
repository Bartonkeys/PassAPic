using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.PushRegistration
{
    public interface IPushProvider
    {
        Task<string> PushToDevices(int id, List<PushQueueMember> memberList, String pushMessageToSend);
    }
}
