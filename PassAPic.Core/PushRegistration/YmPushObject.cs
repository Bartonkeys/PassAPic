using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.PushRegistration
{
    public class YmPushObject 
    {
        public int Id { get; set; }
        public Guid PushApplicationGuid { get; set; }
        public List<PushQueueMember> MemberList { get; set; }
        public String PushMessage { get; set; }

    }

    public class PushQueueMember
    {
        public int Id { get; set; }
        public int PushBadgeNumber { get; set; }
    }
}
