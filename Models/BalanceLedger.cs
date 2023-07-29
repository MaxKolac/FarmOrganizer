﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace FarmOrganizer.Models
{
    public partial class BalanceLedger
    {
        public int Id { get; set; }
        public int IdCostType { get; set; }
        public int IdCropField { get; set; }
        public DateTime DateAdded { get; set; }
        public double BalanceChange { get; set; }
        public string Notes { get; set; }

        public virtual CostType IdCostTypeNavigation { get; set; }
        public virtual CropField IdCropFieldNavigation { get; set; }

        public override string ToString()
        {
            string s =
                $"Id = {Id}; " +
                $"CostType = {IdCostType}; " +
                $"CropField = {IdCropField}; " +
                $"DateAdded Raw = {DateAdded};" +
                $"BalanceChange = {BalanceChange}; " +
                $"Notes = {Notes}";
            return s;
        }
    }
}