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
    
    public partial class Game_Comments
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Nullable<long> Likes { get; set; }
        public int GameId { get; set; }
        public System.DateTime DateCreated { get; set; }
        public int UserId { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}
