﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ATTSystems.SFA.Model.DBModel;

public partial class UsersAudit
{
    public int Id { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public string UserNameAudit { get; set; }

    public string EmailAudit { get; set; }

    public bool EmailConfirmedAudit { get; set; }

    public string PasswordHashAudit { get; set; }

    public string SecurityStampAudit { get; set; }

    public string PhoneNumberAudit { get; set; }

    public bool PhoneNumberConfirmedAudit { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEndDateUtc { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? UpdateDateTime { get; set; }

    public DateTime CreateDateTime { get; set; }

    public string UpdateBy { get; set; }

    public string CreateBy { get; set; }

    public string Remarks { get; set; }
}