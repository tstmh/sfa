namespace ATTSystems.SFA.Model.ViewModel
{
    public class RegistrationViewModel
    {
        public RegistrationViewModel()
        {
            RegistrationViewLists = new List<RegistrationViewList>();
            LocationViewLists = new List<LocationList>();
            IdTypeViewLists = new List<IdTypeNameList>();
            visitorTypeLists = new List<VisitorTypeList>();
            EntryViewLists = new List<RegistrationViewList>();
            StayoverViewLists = new List<RegistrationViewList>();
            UnitsDetailLists = new List<UnitsDetailList>();
        }
        public int Id { get; set; }
        public string? LocationName { get; set; }
        public int LocationId { get; set; }
        public string? IdTypeName { get; set; }
        public int? IdTypeId { get; set; }
        public string? VisitorName { get; set; }
        public string? NricOrPassport { get; set; }
        public string? NricOrPassport2 { get; set; }
        public string? VisitorEmail { get; set; }
        public string? VehicleNo { get; set; }
        public string? VisitorTypeName { get; set; }
        public int VisitorTypeId { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactNum { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? UnitId { get; set; }
        public string? ReasonToBlacklist { get; set; }
        public string? ReasonToReactivate { get; set; }
        public int EntryCount { get; set; }
        public int ExitCount { get; set; }
        public int LiveCount { get; set; }
        public int StayoverCount { get; set; }
        public string? EntryDate { get; set; }
        public string? ExitDate { get; set; }
        public DateTime? _EntryDate { get; set; }
        public DateTime? _ExitDate { get; set; }
        public string? OverStayer { get; set; }
        public string? Blacklisted { get; set; }
        public string? IsActive { get; set; }
        public string? Modulename { get; set; }

        public List<RegistrationViewList> RegistrationViewLists { get; set; }
        public List<RegistrationViewList>? RegistrationViewListsall { get; set; }
        public List<LocationList> LocationViewLists { get; set; }
        public List<IdTypeNameList> IdTypeViewLists { get; set; }
        public List<VisitorTypeList> visitorTypeLists { get; set; }
        public List<RegistrationViewList> EntryViewLists { get; set; }
        public List<RegistrationViewList> StayoverViewLists { get; set; }
        public List<UnitsDetailList> UnitsDetailLists { get; set; }
    }
    public class LocationList
    {

        public int lId { get; set; }
        public string? lLocationName { get; set; }
        public int lLocationId { get; set; }
        public bool lIsSelected { get; set; }
    }
    public class IdTypeNameList
    {

        public int ltId { get; set; }
        public string? ltIdTypeName { get; set; }
        public int ltIdtypeId { get; set; }
        public bool ltIsSelected { get; set; }
    }
    public class VisitorTypeList
    {

        public int lVstId { get; set; }
        public string? lVstTypeNmae { get; set; }
        public int lVsttypeId { get; set; }
        public bool lVstIsSelected { get; set; }
    }
    public class RegistrationViewList
    {
        public RegistrationViewList() { }
        public int listId { get; set; }
        public string? listLocationName { get; set; }
        public int listLocationId { get; set; }
        public string? listIdTypeName { get; set; }
        public int? listIdTypeId { get; set; }
        public string? listVisitorName { get; set; }
        public string? listNricOrPassport { get; set; }
        public string? listNricOrPassport2 { get; set; }
        public string? listOriginalNricOrPassport { get; set; }
        public string? listVisitorEmail { get; set; }
        public string? listVehicleNo { get; set; }
        public string? listVisitorTypeName { get; set; }
        public int listVisitorTypeId { get; set; }
        public string? listCompanyName { get; set; }
        public string? listBlockNo { get; set; }
        public string? listUnitNo { get; set; }
        public string? ManualEntry { get; set; }
        public bool duplicateNricOrPassport { get; set; }
        public bool listIsSelected { get; set; }
        public string? listContactNum { get; set; }
        public string? listentrydate { get; set; }
        public string? listexitdate { get; set; }
        public string? exitgate { get; set; }
        public string? entrygate { get; set; }
        public string? listblacklistdate { get; set; }

    }

    public class UnitsDetailList
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string? BlockNo { get; set; }
        public string? UnitNo { get; set; }
        public string? UnitId { get; set; }
        public bool IsSeleted { get; set; }

    }
}
