﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FarmOrganizer.Models
{
    public partial class BalanceLedger
    {
        public long Id { get; set; }
        public long IdCostType { get; set; }
        public long IdCropField { get; set; }
        public long DateAdded { get; set; }
        public double BalanceChange { get; set; }
        public string Notes { get; set; }

        public virtual CostType IdCostTypeNavigation { get; set; }
        public virtual CropField IdCropFieldNavigation { get; set; }
    }
}