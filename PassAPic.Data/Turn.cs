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
    
    public partial class Turn
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string Image { get; set; }
    
        public virtual Game Game { get; set; }
        public virtual User Users { get; set; }
    }
}
