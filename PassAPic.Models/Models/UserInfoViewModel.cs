﻿namespace PassAPic.Models.Models
{
    public class UserInfoViewModel
    {
        public string UserName { get; set; }

        public bool HasRegistered { get; set; }

        public string LoginProvider { get; set; }

        public int? UserId { get; set; }
    }
}