//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PassAPic.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Leaderboard
    {
        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> TotalScore { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string Username { get; set; }
    }
}