﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FarmOrganizer.Models
{
    public partial class CostType
    {
        public CostType()
        {
            BalanceLedger = new HashSet<BalanceLedger>();
        }

        public long Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<BalanceLedger> BalanceLedger { get; set; }
    }
}