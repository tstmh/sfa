﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ATTSystems.SFA.Model.DBModel
{
    public partial class Terminal
    {
        public long Id { get; set; }
        public string TerminalName { get; set; }
        public string ClientIp { get; set; }
        public string ClientPort { get; set; }
        public string ServerIp { get; set; }
        public string ServerPort { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public string UpdateBy { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsOnline { get; set; }
        public int? TerminalId { get; set; }
    }
}