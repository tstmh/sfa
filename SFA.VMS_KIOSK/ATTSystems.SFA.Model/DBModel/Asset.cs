﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ATTSystems.SFA.Model.DBModel
{
    public partial class Asset
    {
        public int Id { get; set; }
        public byte[] AssetName { get; set; }
        public string Description { get; set; }
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Link { get; set; }
        public bool IsDeleted { get; set; }
        public int? AssetType { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateDateTime { get; set; }

        public virtual AssetType AssetTypeNavigation { get; set; }
    }
}