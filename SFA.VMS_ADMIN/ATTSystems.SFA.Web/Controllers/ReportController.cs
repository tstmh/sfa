using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.ViewModel;




using ATTSystems.SFA.Web.Helper;
using System.Net;
using System.Data;
using DocumentFormat.OpenXml.Drawing;

namespace ATTSystems.SFA.Web.Controllers
{
    [NoDirectAccess]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;
        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }
        readonly Type? type = typeof(string);
        public string? FDate { get; set; }
        public string? TDate { get; set; }
        public int ModifiedTypeIdlst { get; private set; }
        public string? Usrrnm { get; private set; }
        public int LocationId { get; set; }
        public int VisitTypeId { get; set; }
        public int Searchid { get; private set; }
        public int Aditid { get; private set; }
        //Login User
        public string? LogFdate { get; private set; }
        public string? LogTodate { get; private set; }
        public string? LogUsrrnm { get; private set; }
        public string? Nricpassport { get; set; }
        public IActionResult Index()
        {
            return View();
        }
        // priyanka
        #region Visitor Access
        public async Task<ActionResult> VisitorslDetails()
        {
            APIRequest req = new()
            {
                RequestType = "ListAudit",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };
            _logger.LogInformation("Getting visitor details");
            var response = await WebAPIHelper.AppRequestAsync("/Report/GetVisitorAccessList/", req);

            ReportViewModel result = new();

            if (response != null && response.VisitorAccessLists != null)
            {
                result.VisitorAccessLists = new List<VisitorAccessList>(response.VisitorAccessLists);
                _logger.LogInformation("Getting visitor details successfully");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }
        public async Task<ActionResult> SearchVisitorAccessList(ReportViewModel iModel)
        {
            _logger.LogInformation("Search visitor details");
            APIRequest req = new()
            {
                RequestType = "GetRSearchedlist",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = iModel,
                Message = string.Empty
            };
            ReportViewModel? result = new();
            var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchVisitorAccess/", req);
            try
            {
                if (resp != null && resp.Succeeded && resp.VisitorAccessLists != null)
                {
                    result = new ReportViewModel();
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                    _logger.LogInformation("Searched visitor details successfully");
                }
                else
                {
                    result = new ReportViewModel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search visitor details getting error");
            }
            return Json(result);
        }
        public async Task<ActionResult> ExportVisitor()
        {
            _logger.LogInformation("Export to excel visitor details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetVisitorAccessList/", request);
                if (resp != null)
                {
                    if (resp.VisitorAccessLists == null)
                    {
                        _logger.LogInformation("Export to excel visitor details No Data...!");
                    }
                    else
                    {
                        Attenlist = resp.VisitorAccessLists;
                        foreach (var record in Attenlist)
                        {
                            Attenlist2.Add(record);
                        }
                        _logger.LogInformation("Export to excel visitor details data getting");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Export to excel visitor details getting error");
            }
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new DocumentFormat.OpenXml.Spreadsheet.Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Visitor Access Details"
                    };

                    sheets?.AppendChild(sheet);

                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Access Details", 2));
                    rowIdex += 1; //empty row

                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,ID Type,Passport/NRIC/FIN,Location,Entry DateTime,Exit DateTime";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;
                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.EntryDateTimelst1 != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.EntryDateTimelst1, 10));
                            }
                            if (item.ExitDateTimelst1 != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.ExitDateTimelst1, 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));

                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Access Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in exporting report");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Visitor Access Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        public async Task<ActionResult> ExportSearchVisitor(string parameters)
        {
            _logger.LogInformation("Export to excel search visitor details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] values = parameters.Split(';');
                FDate = parameters.Split(';')[0];
                TDate = parameters.Split(';')[1];
                LocationId = int.Parse(values[2]);
                VisitTypeId = int.Parse(values[3]);
            }
            ReportViewModel repModel = new()
            {
                StartDate = Convert.ToDateTime(FDate),
                EndDate = Convert.ToDateTime(TDate),
                LocationId = LocationId,
                VisitTypeId = VisitTypeId
            };
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchVisitorAccess/", request);

                if (resp == null)
                {
                    _logger.LogInformation("Export to excel search visitor details is no data");
                }
                else
                {
                    if (resp.VisitorAccessLists != null && resp.VisitorAccessLists.Count > 0)
                        Attenlist = resp.VisitorAccessLists;
                    foreach (var record in Attenlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel search visitor details is error");
            }

            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Visitor Access Details"
                    };
                    sheets?.AppendChild(sheet);
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Access Details", 2));
                    rowIdex += 1;

                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name, Visitor Type,ID Type, Passport/NRIC/FIN, Location, Entry DateTime, Exit DateTime";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;

                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.EntryDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, Convert.ToDateTime(item.EntryDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            if (item.ExitDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, Convert.ToDateTime(item.ExitDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));
                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Access Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Visitor Access Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        #endregion

        #region Black List
        public async Task<ActionResult> Blacklistvisitor()
        {
            _logger.LogInformation("Getting Black listed visitor details");
            APIRequest req = new()
            {
                RequestType = "ListVisitor",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Report/GetBlockVisitorAccessList/", req);

            ReportViewModel result = new();

            if (response != null && response.VisitorAccessLists != null)
            {
                result.VisitorAccessLists = new List<VisitorAccessList>(response.VisitorAccessLists);
                _logger.LogInformation("Getting black listed visitor details successfully");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }
        public async Task<ActionResult> SearchblkVisitorList(ReportViewModel iModel)
        {
            _logger.LogInformation("Search black listed visitor details");
            APIRequest req = new()
            {
                RequestType = "GetRSearchedlist",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = iModel,
                Message = string.Empty
            };
            dynamic showMessageString = string.Empty;
            ReportViewModel result = new();
            var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchblkVisitor/", req);
            try
            {
                if (resp != null && resp.Succeeded && resp.VisitorAccessLists != null)
                {
                    result = new ReportViewModel();
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                    _logger.LogInformation("Searched black listed visitor details successfully");
                }
                else
                {
                    result = new ReportViewModel();
                    _logger.LogInformation("Searched black listed visitor details getting no data");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
                _logger.LogError("Search black listed visitor details getting error");
            }
            return Json(result);
        }
        public async Task<ActionResult> Exportbloklist()
        {
            _logger.LogInformation("Export to excel black listed visitor details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetBlockVisitorAccessList/", request);
                if (resp != null && resp.VisitorAccessLists == null)
                {
                    _logger.LogInformation("Export to excel black listed visitor details getting No Data...!");
                }
                else if (resp != null && resp.VisitorAccessLists != null)
                {
                    Attenlist = resp.VisitorAccessLists;
                    foreach (var record in Attenlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Export to excel black listed visitor details getting error");
            }
            //#region generate Excel - if error then generate header only
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Blacklist Visitor Details"
                    };
                    sheets?.AppendChild(sheet);
                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Blacklist Visitor Details", 2));
                    rowIdex += 1; //empty row
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,ID Type,Passport/NRIC/FIN,Location,Blacklist DateTime,Blacklist Status,Blacklist By,Reason For Black/UnBlacklist";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;
                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.BlacklistDateTimelst1 != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.BlacklistDateTimelst1, 10));
                            }
                            if (item.Blackliststatus != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.Blackliststatus, 5));
                            }
                            if (item.BlacklistBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, item.BlacklistBylst, 5));
                            }

                            if (item.ReasonForBlacklistlst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, item.ReasonForBlacklistlst, 5));
                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Blacklist Visitor Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in exporting report");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Blacklist Visitor Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        public async Task<ActionResult> ExportSearchblkVisitor(string parameters)
        {
            _logger.LogInformation("Export to excel searched black list visitor details");
            ReportViewModel repModel = new ReportViewModel();
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            if (!string.IsNullOrEmpty(parameters))
            {
                FDate = parameters.Split(';')[0];
                TDate = parameters.Split(';')[1];
                if (FDate != "")
                {
                    repModel.StartDate = Convert.ToDateTime(FDate);
                }
                if (TDate != "")
                {
                    repModel.EndDate = Convert.ToDateTime(TDate);
                }
                if (parameters.Split(';')[2] == "")
                {
                    repModel.NricOrPassport = null;
                }
                else
                {
                    repModel.NricOrPassport = parameters.Split(';')[2];
                }
            }

            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchblkVisitor/", request);
                if (resp != null && resp.VisitorAccessLists == null)
                {
                    _logger.LogInformation("Export to excel searched black list visitor details getting No Data...!");
                }
                else if (resp != null && resp.VisitorAccessLists != null)
                {
                    Attenlist = resp.VisitorAccessLists;
                    foreach (var record in Attenlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel getting error");
            }
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Blacklist Visitor Details"
                    };

                    sheets?.AppendChild(sheet);

                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Blacklist Visitor Details", 2));
                    rowIdex += 1;

                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,ID Type,Passport/NRIC/FIN,Location,Blacklist DateTime,BlackList Status,Blacklist By,Reason For Black/UnBlacklist";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;

                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);
                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.BlacklistDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, Convert.ToDateTime(item.BlacklistDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            if (item.Blackliststatus != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.Blackliststatus, 5));
                            }
                            if (item.BlacklistBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, item.BlacklistBylst, 5));
                            }
                            if (item.ReasonForBlacklistlst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, item.ReasonForBlacklistlst, 5));
                            }

                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Blacklist Visitor Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();

                    result = stream.ToArray();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Blacklist Visitor Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        #endregion

        #region Visitor Audit Trail
        public async Task<ActionResult> AudittrailDetails()
        {
            _logger.LogInformation("Getting audit trail visitor details");
            APIRequest req = new()
            {
                RequestType = "ListAudittrail",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Report/GetAudittrailList/", req);

            ReportViewModel result = new();

            if (response != null && response.VisitorAccessLists != null)
            {
                result.VisitorAccessLists = new List<VisitorAccessList>(response.VisitorAccessLists);
                _logger.LogInformation("Getting audit trail visitor details successfully");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }
        public async Task<ActionResult> SearchAudittrailDetails(ReportViewModel iModel)
        {
            _logger.LogInformation("Search visitor audit trail details");
            APIRequest req = new()
            {
                RequestType = "GetASearchedlist",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = iModel,
                Message = string.Empty
            };
            ReportViewModel result = new();
            var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchAudittrail/", req);
            try
            {
                if (resp != null && resp.Succeeded && resp.VisitorAccessLists != null)
                {
                    result = new ReportViewModel();
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                    _logger.LogInformation("Searched visitor audit trail details successfully");
                }
                else
                {
                    result = new ReportViewModel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Searched visitor audit trail details getting error");
            }
            return Json(result);
        }
        public async Task<ActionResult> ExportAudittrailDetails()
        {
            _logger.LogInformation("Export to excel visitor audit trail details");

            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetAudittrailList/", request);
                if (resp != null)
                {
                    if (resp.VisitorAccessLists == null)
                    {
                        _logger.LogInformation("Export to excel visitor audit trail details getting no data");
                    }
                    else
                    {
                        Attenlist = resp.VisitorAccessLists;
                        foreach (var record in Attenlist)
                        {
                            Attenlist2.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel visitor audit trail details getting error");
            }
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Visitor Audit Trail Details"
                    };

                    sheets?.AppendChild(sheet);
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Audit Trail Details", 2));
                    rowIdex += 1; //empty row

                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,Id Type,Passport/NRIC/FIN,Location,Entry DateTime,Exit DateTime,ManualCheckIn By,Remarks";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;

                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.VisitStratDateTimelst1 != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.VisitStratDateTimelst1, 10));
                            }
                            if (item.VisitEndDateTimelst1 != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.VisitEndDateTimelst1, 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));

                            }
                            if (item.ManualCheckInBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, item.ManualCheckInBylst, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, _blank, 10));

                            }
                            if (item.Remarks != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, item.Remarks, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, _blank, 10));

                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Audit Trail Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Visitor Audit Trail Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));

        }
        public async Task<ActionResult> ExportSearchAudittrai(string parameters)
        {
            //#region Get Data from DB
            _logger.LogInformation("Export to excel search visitor audit trail details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] values = parameters.Split(';');
                FDate = parameters.Split(';')[0];
                TDate = parameters.Split(';')[1];
                LocationId = int.Parse(values[2]);
                VisitTypeId = int.Parse(values[3]);
                Searchid = int.Parse(values[4]);
            }
            ReportViewModel repModel = new()
            {
                StartDate = Convert.ToDateTime(FDate),
                EndDate = Convert.ToDateTime(TDate),
                LocationId = LocationId,
                VisitTypeId = VisitTypeId,
                AuditTypeId = Searchid,
            };
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchAudittrail/", request);
                if (resp != null && resp.VisitorAccessLists != null)
                {
                    if (resp.VisitorAccessLists.Count <= 0)
                    {
                        _logger.LogInformation("Export to excel search visitor audit trail details getting no data");
                    }
                    else
                    {
                        Attenlist = resp.VisitorAccessLists;
                        foreach (var record in Attenlist)
                        {
                            Attenlist2.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel search visitor audit trail details getting error");
            }
            //#region generate Excel - if error then generate header only
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Visitor Audit Trail Details"
                    };

                    sheets?.AppendChild(sheet);

                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Audit Trail Details", 2));
                    rowIdex += 1;
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,Id Type,Passport/NRIC/FIN,Location,Entry DateTime,Exit DateTime,ManualCheckIn By,Remarks";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;

                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.EntryDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, Convert.ToDateTime(item.EntryDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            if (item.ExitDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, Convert.ToDateTime(item.ExitDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));
                            }
                            if (item.ManualCheckInBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, item.ManualCheckInBylst, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(8), rowIdex, _blank, 10));
                            }
                            if (item.Remarks != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, item.Remarks, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(9), rowIdex, _blank, 10));
                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Audit Traillist Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("Visitor Audit Trail Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        #endregion

        #region Load Location and Visitor Type
        public async Task<ActionResult> LoadLocation()
        {

            _logger.LogInformation("Getting location names");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "LoadLocation",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Report/GetLocationList/", req);

            List<VisitorAccessList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<VisitorAccessList>();
                    result = response.VisitorAccessLists;
                    _logger.LogInformation("Getting locations successfully");
                }
                else
                {
                    result = new List<VisitorAccessList>();
                    _logger.LogInformation("Getting locations is no data");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting location is error");
            }
            return Json(result);
        }
        public async Task<ActionResult> LoadVisitorType()
        {
            _logger.LogInformation("Getting visitor types name");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "LoadVisitorType",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Report/GetLoadVisitorType/", req);

            List<VsttypList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<VsttypList>();
                    result = response.VsttypList;
                    _logger.LogInformation("Getting visitor types successfully");
                }
                else
                {
                    result = new List<VsttypList>();
                    _logger.LogInformation("Getting visitor types is no data");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting visitor types is error");
            }
            return Json(result);
        }
        #endregion

        #region graph 

        [HttpPost]
        public async Task<ActionResult> Searchvisitorlinechart(ReportViewModel model)
        {
            _logger.LogInformation("Loading Graph");
            ChartData chartData = new();

            APIRequest req = new()
            {
                RequestType = "GetSearchlinechart",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = model
            };
            ReportViewModel result = new();
            try
            {
                var response = await WebAPIHelper.AppRequestAsync("/Report/GetSearchlinechart/", req);
                if (response != null && response.transLists1 != null)
                {
                    _logger.LogInformation("Getting data to graph");
                    result.transLists1 = response.transLists1;
                    DataTable dt = new();
                    if (type != null)
                    {
                        dt.Columns.Add("Date", type);
                        dt.Columns.Add("TCount", type);
                        dt.Columns.Add("Vsttypnm", type);
                    }
                    foreach (var dc in result.transLists1)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Date"] = Convert.ToDateTime(dc.Datelst1).ToString("dd/MM/yyyy");
                        dr["TCount"] = Convert.ToInt32(dc.Countlst).ToString();
                        dr["Vsttypnm"] = dc.Vsttypnamelst;

                        dt.Rows.Add(dr);
                    }
                    string?[] Labels = dt.AsEnumerable().Select(p => p.Field<string>("Date")).Distinct().ToArray();
                    chartData.Labels = Labels;
                    string?[] Instru = (dt.AsEnumerable().Select(p => p.Field<string>("Vsttypnm"))).Distinct().ToArray();
                    chartData.DatasetLabels = Instru;

                    List<string> datasetLabels = new();
                    List<string[]> datasetDatas = new();
                    for (int i = 0; i < chartData.DatasetLabels.Length; i++)
                    {
                        List<string> data = new();
                        for (int j = 0; j < Labels.Length; j++)
                        {
                            string? list = (dt.AsEnumerable().Where(p => p.Field<string>("Date") == Labels[j] && p.Field<string>("Vsttypnm") == chartData.DatasetLabels[i]).Select(p => p.Field<String>("TCount"))).FirstOrDefault();
                            if (list != null)
                            {
                                data.Add(list);
                            }
                        }
                        datasetDatas.Add(data.ToArray());
                    }
                    chartData.DatasetDatas = datasetDatas;
                }
                else
                {
                    result = new ReportViewModel();
                    _logger.LogInformation("Getting no data to graph");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting graph is error");
            }
            return Json(chartData);
        }
        [HttpPost]
        public async Task<ActionResult> Defaultvisitorlinechart(ReportViewModel model)
        {
            _logger.LogInformation("Loading Graph");
            ChartData? chartData = new();

            APIRequest req = new()
            {
                RequestType = "GetDefaultlinechart",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = model
            };
            ReportViewModel result = new();
            
            try
            {
                var response = await WebAPIHelper.AppRequestAsync("/Report/GetDefaultlinechart/", req);
                if (response != null && response.transLists1 != null)
                {
                    _logger.LogInformation("Getting data to graph");
                    result.transLists1 = response.transLists1;

                    DataTable dt = new();
                    if (type != null)
                    {
                        dt.Columns.Add("Date", type);
                        dt.Columns.Add("TCount", type);
                        dt.Columns.Add("Vsttypnm", type);
                    }
                    foreach (var dc in result.transLists1)
                    {
                        DataRow dr = dt.NewRow();
                        dr["Date"] = Convert.ToDateTime(dc.Datelst1).ToString("dd/MM/yyyy");

                        dr["TCount"] = Convert.ToInt32(dc.Countlst).ToString();
                        dr["Vsttypnm"] = dc.Vsttypnamelst;

                        dt.Rows.Add(dr);
                    }
                    string?[] Labels = dt.AsEnumerable().Select(p => p.Field<string>("Date")).Distinct().ToArray();
                    chartData.Labels = Labels;
                    string?[] Instru = dt.AsEnumerable().Select(p => p.Field<string>("Vsttypnm")).Distinct().ToArray();
                    chartData.DatasetLabels = Instru;

                    List<string> datasetLabels = new();
                    List<string[]> datasetDatas = new();
                    for (int i = 0; i < chartData.DatasetLabels.Length; i++)
                    {
                        List<string> data = new();
                        for (int j = 0; j < Labels.Length; j++)
                        {
                            string? list = dt.AsEnumerable().Where(p => p.Field<string>("Date") == Labels[j] && p.Field<string>("Vsttypnm") == chartData.DatasetLabels[i]).Select(p => p.Field<String>("TCount")).FirstOrDefault();
                            if (list != null)
                            {
                                data.Add(list);
                            }
                        }
                        datasetDatas.Add(data.ToArray());
                    }
                    chartData.DatasetDatas = datasetDatas;
                }
                else
                {
                    result = new ReportViewModel();
                    _logger.LogInformation("Getting no data to graph");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Getting graph is error:" + ex);
            }
            return Json(chartData);
        }
        public class ChartData
        {
            public string?[]? Labels { get; set; }
            public string?[]? DatasetLabels { get; set; }
            public List<string[]>? DatasetDatas { get; set; }
            public string? PDate { get; set; }
        }

        #endregion

        #region User Audit Trail
        public async Task<ActionResult> LoadUser()
        {
            _logger.LogInformation("Getting users name");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "LoadUser",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };
            var response = await WebAPIHelper.AppRequestAsync("/Report/GetUserList/", req);
            List<VisitorAccessList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<VisitorAccessList>();
                    result = response.VisitorAccessLists;
                    _logger.LogInformation("Getting users successfully");
                }
                else
                {
                    result = new List<VisitorAccessList>();
                    _logger.LogInformation("Getting users is no data");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Getting users is error");
            }
            return Json(result);
        }
        public async Task<ActionResult> AudittrailDetailsUsr()
        {
            _logger.LogInformation("Getting user audit trail details");
            APIRequest req = new()
            {
                RequestType = "ListAudit",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Report/GetVisitorUserAccessList/", req);

            ReportViewModel result = new();

            if (response != null && response.VisitorAccessLists != null)
            {
                result.VisitorAccessLists = new List<VisitorAccessList>(response.VisitorAccessLists);
                _logger.LogInformation("Getting user audit trail details successfully");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }
        public async Task<ActionResult> SearchVisitorUserAccessList(ReportViewModel iModel)
        {
            _logger.LogInformation("Search user audit trail details");
            APIRequest req = new()
            {
                RequestType = "GetRSearchedlist",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = iModel,
                Message = string.Empty
            };
            ReportViewModel result = new();
            var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchVisitorUserAccess/", req);
            try
            {
                if (resp != null && resp.Succeeded && resp.VisitorAccessLists != null)
                {
                    result = new ReportViewModel();
                    result.VisitorAccessLists = resp.VisitorAccessLists;
                    _logger.LogInformation("Search user audit trail details successfully");
                }
                else
                {
                    result = new ReportViewModel();
                    _logger.LogInformation("Search user audit trail details getting no data");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search user audit trail details getting error");
            }
            return Json(result);
        }
        public async Task<ActionResult> ExportVisitorUser()
        {
            _logger.LogInformation("Export to excel user audit trail details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();

            //string FDate = string.Empty;
            //string TDate = string.Empty;

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetVisitorUserAccessList/", request);
                if (resp == null || resp.VisitorAccessLists == null)
                {
                    _logger.LogInformation("Export to excel user audit trail details getting no data");
                }
                else
                {
                    Attenlist = resp.VisitorAccessLists;
                    foreach (var record in Attenlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel user audit trail details getting error");
            }
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "User Audit Trail Details"
                    };

                    sheets?.AppendChild(sheet);

                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "User Audit Trail Details", 2));
                    rowIdex += 1;
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,Id Type,Passport/NRIC/FIN,Location,Modified By,Modified DateTime";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;
                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.VisitorNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                            }
                            if (item.VisitTypeNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                            }
                            if (item.Namelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                            }
                            if (item.NricOrPassport1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                            }
                            if (item.LocationNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                            }
                            if (item.ModifiedBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.ModifiedBylst, 5));
                            }
                            if (item.ModifiedDateTime1lst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.ModifiedDateTime1lst, 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));
                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "User Audit Trail Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("User Audit Trail Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        public async Task<ActionResult> ExportSearchVisitorUser(string parameters)
        {
            //#region Get Data from DB
            _logger.LogInformation("Export to excel search user audit trail details");
            byte[]? result = null;
            List<VisitorAccessList> Attenlist = new();
            var Attenlist2 = new List<VisitorAccessList>();
            int ModifiedTypeId = 0;

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] values = parameters.Split(';');
                FDate = parameters.Split(';')[0];
                TDate = parameters.Split(';')[1];
                ModifiedTypeIdlst = int.Parse(values[2]); // Assuming the location ID is at index 2
                Usrrnm = parameters.Split(';')[3];
                ModifiedTypeId = int.Parse(values[2]);
            }
            ReportViewModel repModel = new()
            {
                StartDate = Convert.ToDateTime(FDate),
                EndDate = Convert.ToDateTime(TDate),
                ModifiedTypeId = ModifiedTypeIdlst,
                UserName = Usrrnm,
            };
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchVisitorUserAccess/", request);
                if (resp != null)
                {
                    if (resp.VisitorAccessLists == null || resp.VisitorAccessLists.Count == 0)
                    {
                        _logger.LogInformation("Export to excel search user audit trail details getting no data");
                        //LoggerHelper.Instance.TraceLog("No Data...!");
                    }
                    else
                    {
                        Attenlist = resp.VisitorAccessLists;
                        foreach (var record in Attenlist)
                        {
                            Attenlist2.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export to excel search user audit trail details getting error");
            }
            //#region generate Excel - if error then generate header only
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "User Audit Trail Details"
                    };
                    sheets?.AppendChild(sheet);

                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "User Audit Trail Details", 2));
                    rowIdex += 1;
                    string h1;
                    var cellIdex = 0;
                    if (ModifiedTypeId == 1)
                    {
                        h1 = "Sl.No,Visitor Name,Visitor Type,Id Type,Passport/NRIC/FIN,Location,Modified By,Modified DateTime";
                    }
                    else
                    {
                        h1 = "Sl.No,User Name,User Email,Modified By,Modified DateTime,Remarks";

                    }
                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (ModifiedTypeId == 1)
                    {
                        if (Attenlist2 != null && Attenlist2.Count > 0)
                        {
                            int serialNumber = 1;
                            foreach (VisitorAccessList item in Attenlist2)
                            {
                                row = new Row { RowIndex = ++rowIdex };
                                sheetData.AppendChild(row);

                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                                serialNumber++;

                                if (item.VisitorNamelst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.VisitorNamelst, 5));
                                }
                                if (item.VisitTypeNamelst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.VisitTypeNamelst, 5));
                                }
                                if (item.Namelst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.Namelst, 5));
                                }
                                if (item.NricOrPassport1lst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.NricOrPassport1lst, 5));
                                }
                                if (item.LocationNamelst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.LocationNamelst, 5));
                                }
                                if (item.ModifiedBylst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.ModifiedBylst, 5));
                                }
                                if (item.ModifiedDateTimelst != null)
                                {
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, Convert.ToDateTime(item.ModifiedDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                                }
                                else
                                {
                                    string _blank = string.Empty;
                                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(7), rowIdex, _blank, 10));

                                }
                            }
                        }
                    }
                    else
                    {
                        int serialNumber = 1;
                        foreach (VisitorAccessList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.UserNamelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.UserNamelst, 5));
                            }
                            if (item.UserEmaillst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.UserEmaillst, 5));
                            }
                            if (item.ModifiedBylst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.ModifiedBylst, 5));
                            }

                            if (item.ModifiedDateTimelst != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, Convert.ToDateTime(item.ModifiedDateTimelst).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, _blank, 10));

                            }
                            if (item.Remarks != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.Remarks, 10));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, _blank, 10));

                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "User Audit Trail list Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Exporting Report.");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format("User Audit Trail Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        #endregion

        #region Login Audit Trail
        public async Task<ActionResult> LoadLoginUser()
        {
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "LoadUser",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };
            var response = await WebAPIHelper.AppRequestAsync("/Report/GetLoginUser/", req);
            List<VisitorAccessList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<VisitorAccessList>();
                    result = response.VisitorAccessLists;
                }
                else
                {
                    result = new List<VisitorAccessList>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        public async Task<ActionResult> LoginUserTracking()
        {
            APIRequest req = new()
            {
                RequestType = "listlogin",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };
            var response = await WebAPIHelper.AppRequestAsync("/Report/GetLoginusertracking/", req);
            ReportViewModel result = new();
            try
            {
                if (response != null && response._usersessiontrackinglist != null)
                {
                    result._usersessiontrackinglist = new List<UserSessionTrackingList>(response._usersessiontrackinglist);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return Json(result);
        }
        public async Task<ActionResult> ExportLoginUser()
        {
            byte[]? result = null;
            List<UserSessionTrackingList> Attenlist = new List<UserSessionTrackingList>();
            var Attenlist2 = new List<UserSessionTrackingList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetLoginusertracking/", request);
                if (resp == null || resp._usersessiontrackinglist == null)
                {
                    LoggerHelper.Instance.TraceLog("No Data...!");
                }
                else
                {
                    Attenlist = resp._usersessiontrackinglist;
                    foreach (var record in Attenlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            //#region generate Excel - if error then generate header only
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new Columns();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Login Audit Trail User Details"
                    };

                    sheets?.AppendChild(sheet);

                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Login Audit Trail User Details", 2));
                    rowIdex += 1; //empty row



                    var cellIdex = 0;
                    string h1 = "Sl.No,User Name,Satus,LoginDateTime,LogoutDateTime,AttemptedDateTime,Remarks";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }
                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;
                        foreach (UserSessionTrackingList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);
                            string _blank = String.Empty;

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.UserName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.UserName, 5));
                            }
                            if (item.Status != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.Status, 5));
                            }
                            if (item.LoginDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, Convert.ToDateTime(item.LoginDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 5));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, _blank, 5));
                            }
                            if (item.LogoutDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, Convert.ToDateTime(item.LogoutDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 5));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, _blank, 5));
                            }
                            if (item.AttemptedDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, Convert.ToDateTime(item.AttemptedDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 5));
                            }
                            if (item.Remarks != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.Remarks, 10));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, _blank, 10));
                            }
                        }
                    }
                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Login Audit Trail User Details Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Type: WebApp, Controller: Report, Action: ExportVisitorUser, Message: {0} ", ex.Message));
                return StatusCode((int)HttpStatusCode.InternalServerError, "Error in Exporting Report.");
            }
            return File(result, "application/vnd.ms-excel", string.Format("Login Audit Trail Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));
        }
        public async Task<ActionResult> SearchLoginUserTracking(ReportViewModel iModel)
        {
            APIRequest req = new()
            {
                RequestType = "GetSearchlist",
                RequestString = User?.Identity?.Name ?? string.Empty,
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = iModel,
                Message = string.Empty
            };

            ReportViewModel result = new();
            var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchLoginUserTracking/", req);
            try
            {
                if (resp != null && resp.Succeeded && resp._usersessiontrackinglist != null)
                {
                    result = new ReportViewModel();
                    result._usersessiontrackinglist = resp._usersessiontrackinglist;
                }
                else
                {
                    result = new ReportViewModel();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        public async Task<ActionResult> ExportSearchLoginUser(string parameters)
        {
            byte[]? result = null;
            List<UserSessionTrackingList> Attenlist = new();
            var Attenlist2 = new List<UserSessionTrackingList>();
            string LFomDate = string.Empty;
            string LToDate = string.Empty;
            if (!string.IsNullOrEmpty(parameters))
            {
                //string[] values = parameters.Split(';');
                LogFdate = parameters.Split(';')[0];
                LogTodate = parameters.Split(';')[1];
                LogUsrrnm = parameters.Split(';')[2];
            }
            ReportViewModel repModel = new()
            {
                LFomDate = Convert.ToDateTime(LogFdate),
                LToDate = Convert.ToDateTime(LogTodate),
                LogUserName = LogUsrrnm,
            };
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Report/GetSearchLoginUserTracking/", request);
                if (resp != null)
                {
                    if (resp._usersessiontrackinglist == null)
                    {
                        LoggerHelper.Instance.TraceLog("No Data...!");
                    }
                    else
                    {
                        Attenlist = resp._usersessiontrackinglist;
                        foreach (var record in Attenlist)
                        {
                            Attenlist2.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);
            }
            //#region generate Excel - if error then generate header only
            try
            {
                using (MemoryStream stream = new())
                {
                    var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
                    var workbookpart = document.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    WorkbookStylesPart stylePart = workbookpart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CustomDocumentHelper.GenerateStylesheet();
                    stylePart.Stylesheet.Save();

                    var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet();

                    Columns columns = new();
                    columns.Append(new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true });
                    columns.Append(new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    columns.Append(new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true });
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document?.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document?.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = " Login Audit Trail User Details"
                    };
                    sheets?.AppendChild(sheet);

                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Login Audit Trail User Details", 2));
                    rowIdex += 1; //empty row
                    string h1;
                    var cellIdex = 0;
                    h1 = "Sl.No,User Name,Satus,LoginDateTime,LogoutDateTime,AttemptedDateTime,Remarks";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }

                    if (Attenlist2 != null && Attenlist2.Count > 0)
                    {
                        int serialNumber = 1;

                        foreach (UserSessionTrackingList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            string _blank = String.Empty;

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.UserName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.UserName, 5));
                            }
                            if (item.Status != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.Status, 5));
                            }
                            if (item.LoginDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, Convert.ToDateTime(item.LoginDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 5));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, _blank, 5));
                            }
                            if (item.LogoutDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, Convert.ToDateTime(item.LogoutDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 5));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, _blank, 5));
                            }
                            if (item.AttemptedDateTime != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, Convert.ToDateTime(item.AttemptedDateTime).ToString("dd/MM/yyyy HH:mm:ss tt"), 10));
                            }
                            if (item.Remarks != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.Remarks, 10));
                            }
                            else
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, _blank, 10));
                            }
                        }
                    }

                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Audit Trail UserList Generated:", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document?.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance.LogError(ex);

            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", string.Format(" Login Audit Trail User Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));

        }
        #endregion
    }
}

