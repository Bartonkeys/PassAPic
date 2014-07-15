using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PassAPic.Core.AccountManagement
{
    public static class EmailVerification
    {
        public static bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$");
            return regex.IsMatch(email);
        }
    }
}
