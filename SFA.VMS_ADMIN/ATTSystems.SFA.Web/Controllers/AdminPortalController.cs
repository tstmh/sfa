using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.SFA.Model.DBModel;
using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.Web.Helper;
using ATTSystems.SFA.Web.Models;
using ATTSystems.SFA.Web.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using ExcelDataReader;
using ATTSystems.NetCore.Utilities;
using Microsoft.AspNetCore.Authorization;
using ATTSystems.NetCore.Logger;
using DocumentFormat.OpenXml.Drawing;
using System.Linq.Expressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using OfficeOpenXml;
using Microsoft.DotNet.Scaffolding.Shared.Project;

namespace ATTSystems.SFA.Web.Controllers
{
    [NoDirectAccess]
    public class AdminPortalController : Controller
    {
        private readonly ILogger<AdminPortalController> _logger;
        public AdminPortalController(ILogger<AdminPortalController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Santosh Wali

        #region Dashboard

        [HttpGet]
        public async Task<ActionResult> Dashboard()
        {
            _logger.LogInformation("Getting transaction list");
            APIRequest req = new()
            {
                RequestType = "TransactionList",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            RegistrationViewModel? result = new();

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetDashboardList/", req);
            if (response != null)
            {
                result.EntryViewLists = new List<RegistrationViewList>();
                if (response.EntryViewLists != null && response.EntryViewLists.Count > 0)
                {
                    _logger.LogInformation("Getting transaction count");
                    ViewBag.EntryCount = response.EntryCount;
                    ViewBag.ExitCount = response.ExitCount;
                    ViewBag.LiveCount = response.LiveCount;
                    ViewBag.StayoverCount = response.StayoverCount;

                    result.EntryViewLists = response.EntryViewLists;
                }
                else
                {
                    _logger.LogInformation("Transaction count is 0");
                    ViewBag.EntryCount = response.EntryCount;
                    ViewBag.ExitCount = response.ExitCount;
                    ViewBag.LiveCount = response.LiveCount;
                    ViewBag.StayoverCount = response.StayoverCount;

                }
                if (response.StayoverViewLists != null && response.StayoverViewLists.Count > 0)
                {
                    result.StayoverViewLists = response.StayoverViewLists;
                    ViewBag.StayoverCount = response.StayoverCount;
                    _logger.LogInformation("Getting stayover count");
                }
                else
                {
                    _logger.LogInformation("Stayover count is 0");
                }
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> ReactivateVisitor([FromBody] RegistrationViewModel datalist)
        {
            _logger.LogInformation("Reactivate visitor");
            JsonResp jsonResp = new();
            try
            {
                APIRequest req = new()
                {
                    RequestType = "ReactivateVisitor",
                    RequestString = User?.Identity?.Name ?? string.Empty,
                    Model = datalist
                };

                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/ReactivateVisitor/", req);
                if (resp != null)
                {
                    if (resp.Code == 0)
                    {
                        jsonResp.resultCode = 300;
                        jsonResp.resultDescription = "Visitor Reactivation Failed";
                        _logger.LogInformation("Visitor Reactivation Failed");
                    }
                    else
                    {
                        jsonResp.resultCode = 200;
                        jsonResp.resultDescription = "Visitor Reactivated Successfully";
                        _logger.LogInformation("Visitor Reactivated Successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visitor Reactivated getting error");

            }
            return Json(jsonResp);
        }

        //overstayer visitor
        public async Task<ActionResult> ExportOverstayerVisitor(int locationId, string locationName)
        {
            _logger.LogInformation("Export to excel Overstayer visitor details");
            byte[]? result = null;
            List<RegistrationViewList> Overstaylist = new();
            var Attenlist2 = new List<RegistrationViewList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = repModel,
                    RequestString = locationId.ToString()
                };
                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/ExportOverstayerVisitor/", request);
                if (resp != null)
                {
                    if (resp.registrationViewLists == null)
                    {
                        _logger.LogInformation("Export to excel Overstayer_Jurong Fishery Port getting No Data...!");
                    }
                    else
                    {
                        Overstaylist = resp.registrationViewLists;
                        foreach (var record in Overstaylist)
                        {
                            Attenlist2.Add(record);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Export to excel Overstayer_Jurong Fishery getting error");
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

                    Columns columns = new Columns(
                   new Column() { Min = 1, Max = 8, Width = 14, CustomWidth = true },
                   new Column() { Min = 9, Max = 9, Width = 20, CustomWidth = true },
                   new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true },
                   new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true },
                   new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true },
                   new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true },
                   new Column() { Min = 10, Max = 10, Width = 30, CustomWidth = true }
                );
                    worksheetPart.Worksheet.AppendChild(columns);

                    var sheetData = new SheetData();
                    worksheetPart.Worksheet.AppendChild(sheetData);

                    SheetView sheetView = new();
                    sheetView.ShowGridLines = new BooleanValue(false);
                    sheetView.WorkbookViewId = 0;
                    worksheetPart.Worksheet.SheetViews = new SheetViews();
                    worksheetPart.Worksheet.SheetViews.AppendChild(sheetView);
                    var sheets = document.WorkbookPart?.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document.WorkbookPart?.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Overstayer Report"
                    };
                    sheets?.AppendChild(sheet);
                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Report Type:  Overstayer Visitor Details", 2));
                    rowIdex += 1; //empty row
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,Location,Entry DateTime,Contact Number";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }

                    if (Attenlist2 == null || Attenlist2.Count == 0)
                    {
                        row = new Row { RowIndex = ++rowIdex };
                        sheetData.AppendChild(row);
                        var cell = CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "No data available", 2);
                        cell.StyleIndex = 3;
                        row.AppendChild(cell);

                        var fontId = 1;
                        var cellFormat = new CellFormat
                        {
                            FontId = Convert.ToUInt32(fontId),
                            FillId = 0,
                            BorderId = 0,
                            ApplyFont = true
                        };

                        var cellFormats = stylePart.Stylesheet.CellFormats ?? new CellFormats();
                        if (cellFormats != null)
                        {
                            cellFormats.Append(cellFormat);
                        }
                        stylePart.Stylesheet.CellFormats = cellFormats;
                        stylePart.Stylesheet.Save();

                        MergeCells mergeCells;
                        if (worksheetPart.Worksheet.Elements<MergeCells>().Any())
                        {
                            mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().First();
                        }
                        else
                        {
                            mergeCells = new MergeCells();
                            worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                        }

                        mergeCells = new MergeCells(new MergeCell() { Reference = new StringValue($"A{rowIdex}:B{rowIdex}") },
                       new MergeCell() { Reference = new StringValue($"C{rowIdex}:E{rowIdex}") });

                        worksheetPart.Worksheet.Save();
                    }
                    else
                    {
                        int serialNumber = 1;
                        foreach (RegistrationViewList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.listVisitorName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.listVisitorName, 5));
                            }
                            if (item.listVisitorTypeName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.listVisitorTypeName, 10));
                            }
                            if (item.listLocationName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.listLocationName, 10));
                            }

                            if (item.listentrydate != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.listentrydate, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, _blank, 10));
                            }
                            if (item.listContactNum != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.listContactNum, 5));
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
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Overstayer Visitor Details", 0));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in exporting report");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", $"Overstayer_{locationName}.xlsx");

        }

        #endregion

        #region Registration

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> PreRegistration()
        {
            _logger.LogInformation("Getting visitor registration detail");
            APIRequest req = new()
            {
                RequestType = "ListRegisters",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            RegistrationViewModel result = new();

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetRegisterList/", req);

            if (response != null && response.registrationViewLists != null)
            {
                result.RegistrationViewLists = response.registrationViewLists;
                _logger.LogInformation("Getting visitor registration details");
            }
            else
            {
                result = new RegistrationViewModel();
                _logger.LogInformation("Getting visitor registration details is not found");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> GetRegisteredVisitor()
        {
            int totalRecord = 3000;
            int filterRecord = 15;
            var draw = Request.Form["draw"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "1");

            APIRequest req = new APIRequest()
            {
                RequestString = pageSize.ToString(),
                RequestType = skip.ToString(),
            };
            var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetRegisterList/", req);

            var returnObj = new
            {
                draw = draw,
                recordsTotal = totalRecord,
                recordsFiltered = filterRecord,
                data = resp?.registrationViewLists
            };

            return Json(returnObj);
        }

        public async Task<ActionResult> ExportVisitorRegistrationsToExcel(int locationId, string locationName)
        {
            byte[]? result = null;
            RegistrationViewModel? model = new RegistrationViewModel();
            APIRequest req = new()
            {
                RequestType = "ExportRegisters",
                UserName = User?.Identity?.Name ?? string.Empty,
                RequestString = locationId.ToString()
            };
            try
            {
                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetExcelregisterlist/", req);
                if (resp != null)
                {
                    if (resp.registrationViewLists != null)
                    {
                        model.RegistrationViewLists = resp.registrationViewLists;
                    }

                }

                //#region generate Excel - if error then generate header only

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
                        Name = locationName
                    };
                    sheets?.AppendChild(sheet);
                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Report Type:  Registered Visitor Details", 2));

                    MergeCells mergeCells = new MergeCells();
                    string lastRow = (rowIdex + 1).ToString();
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A1:D1") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A2:D2") });
                    // Example of dynamic merging based on data rows count
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{lastRow}:B{lastRow}") });
                    mergeCells.Append(new MergeCell() { Reference = new StringValue($"D{lastRow}:E{lastRow}") });
                    worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());

                    rowIdex += 1; //empty row
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,NRIC/Passport,Contact Number,Registered Date,Expire Date,Visitor Type,Company Name,Unit Id,Vehicle No,Email,Location";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }

                    List<RegistrationViewList>? filteredList = model.RegistrationViewLists.Where(item => item.listLocationId == locationId).ToList();
                    if (filteredList == null || filteredList.Count == 0)
                    {
                        row = new Row { RowIndex = ++rowIdex };
                        sheetData.AppendChild(row);
                        var cell = CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "No data available", 2);
                        cell.StyleIndex = 3;
                        row.AppendChild(cell);

                        var fontId = 1;
                        var cellFormat = new CellFormat
                        {
                            FontId = Convert.ToUInt32(fontId),
                            FillId = 0,
                            BorderId = 0,
                            ApplyFont = true
                        };

                        var cellFormats = stylePart.Stylesheet.CellFormats ?? new CellFormats();
                        cellFormats.Append(cellFormat);
                        stylePart.Stylesheet.CellFormats = cellFormats;
                        stylePart.Stylesheet.Save();

                        if (worksheetPart.Worksheet.Elements<MergeCells>().Any())
                        {
                            mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().First();
                        }
                        else
                        {
                            mergeCells = new MergeCells();
                            worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                        }

                        //mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:F5") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{rowIdex}:B{rowIdex}") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"D{rowIdex}:E{rowIdex}") });

                        worksheetPart.Worksheet.Save();
                    }
                    else
                    {
                        int serialNumber = 1;
                        foreach (var item in model.RegistrationViewLists)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.listVisitorName ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.listNricOrPassport ?? string.Empty, 10));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.listContactNum ?? string.Empty, 10));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.listentrydate ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.listexitdate ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.listVisitorTypeName ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(7), rowIdex, item.listCompanyName ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(8), rowIdex, item.listUnitNo ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(9), rowIdex, item.listVehicleNo ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(10), rowIdex, item.listVisitorEmail ?? string.Empty, 5));
                            row.AppendChild(CreateTextCellWithBorder(CustomDocumentHelper.ColumnLetter(11), rowIdex, item.listLocationName ?? string.Empty, 5));
                        }
                    }

                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CreateTextCellWithoutBorder(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Registered Visitor Details"));
                    row.AppendChild(CreateTextCellWithoutBorder(CustomDocumentHelper.ColumnLetter(3), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt")));


                    workbookpart.Workbook.Save();
                    document?.Dispose();

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in exporting report");
            }
            return File(result ?? Array.Empty<byte>(), "application/vnd.ms-excel", $"Visitor Registration - {locationName}.xlsx");
        }
        public static Cell CreateTextCellWithBorder(string header, UInt32 rowIndex, string text, int styleIndex)
        {
            Cell cell = new()
            {
                CellReference = header + rowIndex,
                DataType = CellValues.String,
                CellValue = new CellValue(text),
                StyleIndex = 5
            };
            return cell;
        }
        public static Cell CreateTextCellWithoutBorder(string header, UInt32 rowIndex, string text)
        {
            Cell cell = new Cell()
            {
                CellReference = header + rowIndex,
                DataType = CellValues.String,
                CellValue = new CellValue(text)
            };

            return cell;
        }


        public ActionResult UploadRegistration()
        {
            _logger.LogInformation("Upload excel");
            return PartialView("_UploadExcel");
        }

        [HttpPost]
        public async Task<ActionResult> UploadFiles(RegistrationViewModel model)
        {
            _logger.LogInformation("Uploading files");
            JsonResp jsonResp = new();
            List<RegistrationViewList>? result = null;
            if (Request.ContentLength > 0)
            {
                try
                {
                    IFormFileCollection files = Request.Form.Files;
                    for (int a = 0; a < files.Count; a++)
                    {
                        IFormFile file = files[a];
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        using var reader = ExcelReaderFactory.CreateReader(file.OpenReadStream());
                        if (reader.RowCount == 2)
                        {
                            result = new List<RegistrationViewList>();
                        }
                        else
                        {
                            do
                            {
                                string? NRIC_NO = string.Empty;
                                int _c = reader.RowCount;
                                int i = 0;
                                while (reader.Read()) //Each row of the file
                                {
                                    var sheet = reader.Name;

                                    if (sheet != "Info")
                                    {
                                        if (sheet == "SFA Staff")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        var idtypename = reader.GetValue(2).ToString();
                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 1,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 1,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 1,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 1,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (sheet == "Tenants" || sheet == "Workers")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        int _typeid = 0;
                                                        if (sheet == "Tenants")
                                                        {
                                                            _typeid = 2;
                                                        }
                                                        if (sheet == "Workers")
                                                        {
                                                            _typeid = 3;
                                                        }

                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = _typeid,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listUnitNo = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = _typeid,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listUnitNo = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = _typeid,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listUnitNo = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = _typeid,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listUnitNo = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (sheet == "Trade Visitors")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 4,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listCompanyName = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 4,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listCompanyName = reader.GetValue(8).ToString()
                                                                    });
                                                                }

                                                            }
                                                            else
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 4,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listCompanyName = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 4,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                        listCompanyName = reader.GetValue(8).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (sheet == "Public")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 5,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 5,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }

                                                            }
                                                            else
                                                            {
                                                                var idtypename = reader.GetValue(2).ToString();
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 5,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 5,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (sheet == "Other Government Agency Staff")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        var idtypename = reader.GetValue(2).ToString();
                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {

                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 6,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 6,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 6,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 6,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (sheet == "Managing Agent and Staff")
                                        {
                                            if (_c > 0)
                                            {
                                                if (i > 1 && i < _c)
                                                {
                                                    var checklocation = reader.GetValue(1);
                                                    if (checklocation != null)
                                                    {
                                                        var idtypename = reader.GetValue(2).ToString();
                                                        var checknric_no = reader.GetValue(3);
                                                        if (checknric_no != null)
                                                        {
                                                            NRIC_NO = checknric_no.ToString();
                                                        }
                                                        if (NRIC_NO != null)
                                                        {
                                                            var vehicle_no = reader.GetValue(6);
                                                            if (vehicle_no != null)
                                                            {

                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 7,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 7,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVehicleNo = reader.GetValue(6).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (idtypename == "Passport")
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 7,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listVisitorEmail = reader.GetValue(5).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString(),
                                                                    });
                                                                }
                                                                else
                                                                {
                                                                    model.RegistrationViewLists.Add(new RegistrationViewList
                                                                    {
                                                                        listVisitorTypeId = 7,
                                                                        listLocationName = reader.GetValue(1).ToString(),
                                                                        listIdTypeName = reader.GetValue(2).ToString(),
                                                                        listNricOrPassport = reader.GetValue(3).ToString(),
                                                                        listVisitorName = reader.GetValue(4).ToString(),
                                                                        listContactNum = reader.GetValue(7).ToString()
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        i++;
                                    }
                                    else
                                    {
                                        reader.Close();
                                    }
                                }
                            } while (reader.NextResult()); //Move to NEXT SHEET

                            APIRequest req = new()
                            {
                                RequestType = "",
                                Model = model
                            };

                            var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetNricOrPassportNotExistenceAsync/", req);
                            result = new List<RegistrationViewList>();
                            if (resp != null)
                            {
                                if (resp.registrationViewLists != null)
                                {
                                    foreach (var item in resp.registrationViewLists)
                                    {
                                        result.Add(new RegistrationViewList
                                        {
                                            listVisitorTypeId = item.listVisitorTypeId,
                                            listLocationName = item.listLocationName,
                                            listIdTypeName = item.listIdTypeName,
                                            listNricOrPassport = item.listNricOrPassport,
                                            listVisitorName = item.listVisitorName,
                                            listVisitorEmail = item.listVisitorEmail,
                                            listVehicleNo = item.listVehicleNo,
                                            listContactNum = item.listContactNum,
                                            listUnitNo = item.listUnitNo,
                                            listCompanyName = item.listCompanyName,
                                            duplicateNricOrPassport = item.duplicateNricOrPassport
                                        });
                                    }
                                }
                            }

                            model.RegistrationViewLists = result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error");
                }
            }
            else
            {
                jsonResp.resultDescription = "can not find file";
                _logger.LogInformation("Can not found file");
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> SaveBatchExcelFiles([FromBody] RegistrationViewModel datalist)
        {
            JsonResp jsonResp = new();
            try
            {
                _logger.LogInformation("SaveExcel Data");
                APIRequest req = new()
                {
                    RequestType = "SaveExcel Data",
                    RequestString = User?.Identity?.Name ?? string.Empty,
                    Model = datalist
                };

                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/SaveBatchExcelFiles/", req);
                if (resp != null)
                {
                    if (resp.Code == 0)
                    {
                        jsonResp.resultCode = 300;
                        jsonResp.resultDescription = "Failed. Visitor details details are invalid please check and add again..";
                        _logger.LogInformation("Failed. Visitor details details are invalid please check and add again..");
                    }
                    else
                    {
                        jsonResp.resultCode = 200;
                        jsonResp.resultDescription = string.Format("Excel files are successfully created.");
                        _logger.LogInformation("Excel files are successfully created.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Save excel file is getting error");
            }
            return Json(jsonResp);
        }

        #endregion

        #region ManualCheckIn

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ManualCheckIn()
        {
            _logger.LogInformation("Get manual Registers");
            APIRequest req = new()
            {
                RequestType = "GetmanualRegisters",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            RegistrationViewModel result = new();

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetManualCheckInList/", req);

            if (response != null && response.registrationViewLists != null)
            {
                result.RegistrationViewLists = response.registrationViewLists;
                _logger.LogInformation("Getting manual checkin register detsils");
            }
            else
            {
                result = new RegistrationViewModel();
                _logger.LogInformation("Not Getting manual checkin register detsils");
            }
            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> ManualCheckInSave([FromBody] RegistrationViewModel datalist)
        {
            JsonResp jsonResp = new();
            try
            {
                _logger.LogInformation("Manual check in save");
                APIRequest req = new()
                {
                    RequestType = "Manual CheckIn",
                    RequestString = User?.Identity?.Name ?? string.Empty,
                    Model = datalist
                };

                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/ManualCheckInSave/", req);
                if (resp != null)
                {

                    if (resp.Code == 0)
                    {
                        jsonResp.resultCode = 300;
                        jsonResp.resultDescription = "Manual CheckIn Failed..";
                        _logger.LogInformation("Manual CheckIn Failed..");
                    }
                    else
                    {
                        jsonResp.resultCode = 200;
                        jsonResp.resultDescription = string.Format("Manual CheckIn Done.");
                        _logger.LogInformation("Manual CheckIn Done.");
                    }
                }
            }
            catch (Exception ex)
            {
                // ErrorMsg = ExceptionTools.GetInnerExceptionMessage(ex);
                _logger.LogError(ex, "Manual CheckIn is getting error.");
            }
            return Json(jsonResp);
        }

        #endregion

        #region Blacklisting

        public async Task<ActionResult> BlackList()
        {
            _logger.LogInformation("Getting black list visitor details");
            APIRequest req = new()
            {
                RequestType = "ListBlacklist",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };

            RegistrationViewModel result = new();

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetBlackList/", req);

            if (response != null && response.registrationViewLists != null)
            {
                result.RegistrationViewLists = response.registrationViewLists;
                _logger.LogInformation("Getting black list visitor details");
            }
            else
            {
                result = new RegistrationViewModel();
                _logger.LogInformation("not Getting black list visitor details");
            }
            result.Modulename = HttpContext.Session.GetString("ModuleName");
            return View(result);
        }

        // Blacklist trigger
        public async Task<JsonResult> Blacklist_Trigger([FromBody] RegistrationViewModel datalist)
        {
            _logger.LogInformation("getting blacklist_Trigger");
            JsonResp jsonResp = new();
            try
            {
                APIRequest req = new()
                {
                    RequestType = "Update Trigger",
                    RequestString = User?.Identity?.Name ?? string.Empty,
                    Model = datalist
                };

                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/blacklist_Trigger/", req);
                if (resp != null)
                {
                    if (resp.Code == 0)
                    {
                        jsonResp.resultCode = 300;
                        jsonResp.resultDescription = "Visitor Blacklisting Failed";
                        _logger.LogInformation("Visitor Blacklisting Failed");
                    }
                    else
                    {
                        jsonResp.resultCode = 200;
                        jsonResp.resultDescription = string.Format("Visitor Blacklisted Successfully");
                        _logger.LogInformation("Visitor Blacklisted Successfully");
                    }
                }
                else
                {
                    jsonResp.resultCode = 300;
                    jsonResp.resultDescription = "Visitor Blacklisting Failed";
                    _logger.LogInformation("Visitor Blacklisting Failed");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visitor Blacklisted is getting error");
            }
            return Json(jsonResp);
        }

        //blacklist excel export
        public async Task<ActionResult> ExportblacklistVisitor()
        {
            _logger.LogInformation("Export to excel BlackList visitor details");
            byte[]? result = null;
            List<RegistrationViewList> blacklistlist = new();
            var Attenlist2 = new List<RegistrationViewList>();

            ReportViewModel repModel = new();
            try
            {
                APIRequest request = new()
                {
                    RequestType = "AttendanceList",
                    UserName = User.Identity.Name,
                    Message = string.Empty,
                    Model = repModel,
                    //  RequestString = locationId.ToString()
                };
                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/ExportblacklistVisitor/", request);
                if (resp.registrationViewLists.Count == 0)
                {
                    _logger.LogInformation("Export to excel BlackList_Jurong Fishery Port getting No Data...!");
                }
                else
                {
                    blacklistlist = resp.registrationViewLists;
                    foreach (var record in blacklistlist)
                    {
                        Attenlist2.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Export to excel BlackList_Jurong Fishery getting error");
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
                    var sheets = document.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

                    var sheet = new Sheet()
                    {
                        Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Blacklist Visitor Details"
                    };
                    sheets.AppendChild(sheet);
                    // Add Title Group
                    UInt32 rowIdex = 0;
                    var row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Visitor Management System", 2));

                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "Report Type:  Blacklist Visitor Details", 2));
                    rowIdex += 1; //empty row
                    var cellIdex = 0;
                    string h1 = "Sl.No,Visitor Name,Visitor Type,NRIC/ Passport,Location,BlackListed DateTime,Contact Number";

                    List<string> headerList = h1.Split(',').ToList();
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    foreach (var header in headerList)
                    {
                        row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(cellIdex++), rowIdex, header, 4));
                    }

                    if (Attenlist2 == null || Attenlist2.Count == 0)
                    {
                        row = new Row { RowIndex = ++rowIdex };
                        sheetData.AppendChild(row);
                        var cell = CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "No data available", 2);
                        cell.StyleIndex = 3;
                        row.AppendChild(cell);

                        var fontId = 1;
                        var cellFormat = new CellFormat
                        {
                            FontId = Convert.ToUInt32(fontId),
                            FillId = 0,
                            BorderId = 0,
                            ApplyFont = true
                        };

                        var cellFormats = stylePart.Stylesheet.CellFormats ?? new CellFormats();
                        cellFormats.Append(cellFormat);
                        stylePart.Stylesheet.CellFormats = cellFormats;
                        stylePart.Stylesheet.Save();

                        MergeCells mergeCells;
                        if (worksheetPart.Worksheet.Elements<MergeCells>().Any())
                        {
                            mergeCells = worksheetPart.Worksheet.Elements<MergeCells>().First();
                        }
                        else
                        {
                            mergeCells = new MergeCells();
                            worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
                        }
                        //mergeCells.Append(new MergeCell() { Reference = new StringValue("A5:F5") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"A{rowIdex}:B{rowIdex}") });
                        mergeCells.Append(new MergeCell() { Reference = new StringValue($"C{rowIdex}:E{rowIdex}") });

                        worksheetPart.Worksheet.Save();
                    }
                    else
                    {
                        int serialNumber = 1;
                        foreach (RegistrationViewList item in Attenlist2)
                        {
                            row = new Row { RowIndex = ++rowIdex };
                            sheetData.AppendChild(row);

                            row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, serialNumber.ToString(), 5));
                            serialNumber++;

                            if (item.listVisitorName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(1), rowIdex, item.listVisitorName, 5));
                            }
                            if (item.listVisitorTypeName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, item.listVisitorTypeName, 10));
                            }
                            if (item.listNricOrPassport != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(3), rowIdex, item.listNricOrPassport.ToString(), 10));
                            }


                            if (item.listLocationName != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(4), rowIdex, item.listLocationName, 10));
                            }

                            if (item.listblacklistdate != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, item.listblacklistdate, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(5), rowIdex, _blank, 10));
                            }
                            if (item.listContactNum != null)
                            {
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, item.listContactNum, 5));
                            }
                            else
                            {
                                string _blank = string.Empty;
                                row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(6), rowIdex, _blank, 10));
                            }

                        }
                    }


                    rowIdex += 1;
                    row = new Row { RowIndex = ++rowIdex };
                    sheetData.AppendChild(row);
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(0), rowIdex, "BlackList Visitor Details", 2));
                    row.AppendChild(CustomDocumentHelper.CreateTextCell(CustomDocumentHelper.ColumnLetter(2), rowIdex, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt"), 0));

                    workbookpart.Workbook.Save();
                    document.Dispose();  // Use Dispose instead of Close

                    result = stream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in exporting report");
            }
            // return File(result, "application/vnd.ms-excel", string.Format($"Overstayer_{locationName}.xlsx:", DateTime.Now.ToString("ddMMyyyy")));
            //return File(result, "application/vnd.ms-excel", $"BlackList_{locationName}.xlsx");

            return File(result, "application/vnd.ms-excel", string.Format("BlackList Visitor Details.xlsx", DateTime.Now.ToString("ddMMyyyy")));

        }



        #endregion

        #endregion

        #region Rakesh
        public async Task<ActionResult> SingleRegisterModal()
        {
            _logger.LogInformation("SingleRegisterModal");
            RegistrationViewModel result = new();
            APIRequest req = new()
            {
                RequestType = "ListRegisters",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = string.Empty,
                Model = string.Empty
            };
            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetRegisterList/", req);
            if (response != null)
            {
                if (response.UnitsDetailLists != null)
                    result.UnitsDetailLists = response.UnitsDetailLists;
            }
            else
            {
                result.UnitsDetailLists = new List<UnitsDetailList>();
            }
            return PartialView("_AddSingleRegistration", result);
        }
        public async Task<ActionResult> SingleregLoadLocation()
        {
            _logger.LogInformation("SingleregLoadLocation");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "NricLoadLocation",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetsingleregLocationList/", req);

            List<RegistrationViewList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<RegistrationViewList>();
                    result = response.registrationViewLists;
                    _logger.LogInformation("getting SingleregLoadLocation");
                }
                else
                {
                    result = new List<RegistrationViewList>();
                    _logger.LogInformation("not getting SingleregLoadLocation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleregLoadLocation is getting error");
            }
            return Json(result);
        }

        public async Task<ActionResult> SingleLoadVisitorType()
        {
            _logger.LogInformation("loading the SingleLoadVisitorTypes dropdown");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "NricLoadVisitorType",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetSingleLoadVisitorType/", req);

            List<RegistrationViewList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<RegistrationViewList>();
                    result = response.registrationViewLists;
                    _logger.LogInformation("Getting the SingleLoadVisitorTypes dropdown");
                }
                else
                {
                    result = new List<RegistrationViewList>();
                    _logger.LogInformation("not Getting the SingleLoadVisitorTypes dropdown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "SingleLoadVisitorTypes is getting error");
            }
            return Json(result);
        }
        public async Task<ActionResult> SingleLoadVstIdentityType()
        {
            _logger.LogInformation("loading the SingleLoadVstIdentityType dropdown");
            APIRequest req = new()
            {
                RequestString = User?.Identity?.Name ?? string.Empty,
                RequestType = "NricLoadVisitorType",
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/SingleLoadVstIdentityType/", req);

            List<RegistrationViewList>? result = null;
            try
            {
                if (response != null && response.Succeeded == true)
                {
                    result = new List<RegistrationViewList>();
                    result = response.registrationViewLists;
                    _logger.LogInformation("Getting the SingleLoadVstIdentityType dropdown");
                }
                else
                {
                    result = new List<RegistrationViewList>();
                    _logger.LogInformation("not Getting the SingleLoadVstIdentityType dropdown");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "SingleLoadVstIdentityType is getting error");
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> Singleregsave(RegistrationViewModel model)
        {
            dynamic showMessageString = string.Empty;
            _logger.LogInformation("Single registration save");
            APIRequest req = new()
            {
                RequestType = "",
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = model
            };
            var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetSingleregsave/", req);
            try
            {
                if (resp != null)
                {
                    if (resp.Code == 1)
                    {
                        string err = resp != null ? resp.Message : string.Empty;
                        showMessageString = new
                        {
                            ResultCode = 200,
                            Message = "Passport/NRIC/FIN is successfully registered.\n" + err,
                            modalType = "Add"
                        };
                        _logger.LogInformation("Passport/NRIC/FIN is successfully registered.\n");
                    }
                    else if (resp.Code == -1)
                    {
                        showMessageString = new
                        {
                            ResultCode = 300,
                            Message = string.Format("Passport/NRIC/FIN is already registered.", resp.Code),
                            modalType = "Add"
                        };
                        _logger.LogInformation("Passport/NRIC/FIN is already registered.");
                    }
                    else if (resp.Code == 0)
                    {
                        showMessageString = new
                        {
                            ResultCode = 400,
                            Message = string.Format("Passport/NRIC/FIN is not registered.", resp.Code),
                            modalType = "Add"
                        };
                        _logger.LogInformation("Passport/NRIC/FIN is not registered.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Single registration save is getting error");

            }
            return Json(showMessageString);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteVisitor(string rowId)
        {
            _logger.LogInformation("Delete Visitor details");
            dynamic showMessageString = string.Empty;
            try
            {
                APIRequest req = new()
                {
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = "Delete Visitor",
                    RequestString = rowId,
                    RequestType = "Delete",
                };


                var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/Deletevstreg/", req);
                if (response != null && response.Succeeded)
                {
                    showMessageString = new
                    {
                        resultCode = 200,
                        Message = string.Format("Visitor registration is successfully deleted"),
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("Visitor registration is successfully deleted");
                }
                else
                {
                    showMessageString = new
                    {
                        resultCode = 300,
                        Message = "Failed! Visitor is not updated.",
                        ModalType = "Delete",
                    };
                    _logger.LogInformation("Failed! Visitor is not updated.");
                }
                return Json(showMessageString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete visitor is getting error");
                return Json(showMessageString);
            }
        }

        [HttpGet]
        public async Task<ActionResult> EditRegModel(string id)
        {
            _logger.LogInformation("Edit visitor details");

            APIRequest req = new()
            {
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = "Get Visitor Registartion for editing by id",
                RequestString = id,
                RequestType = "Get single visitor registartion for editing",
            };
            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetEditRegModel/", req);

            if (response != null)
            {
                RegistrationViewModel? model = new();
                model = response.rmodel;
                _logger.LogInformation("Edit visitor details popup is showing");
                return PartialView("_EditVisitorRegister", model);
            }
            else
            {
                _logger.LogInformation("Invalid input");
                string errmsg = (response != null && !string.IsNullOrEmpty(response.Message)) ? response.Message : "Invalid input";
                return PartialView(errmsg, null);
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateVisitor(RegistrationViewModel pmodel)
        {
            dynamic showMessageString = string.Empty;
            _logger.LogInformation("Update visitor");
            try
            {
                APIRequest request = new()
                {
                    Model = pmodel,
                    UserName = User?.Identity?.Name ?? string.Empty,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/UpdateVisitorDetails/", request);

                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        ResultCode = 300,
                        Message = "Failed. Visitor is not Updated.\n" + err,
                        modalType = "Update",
                    };
                    _logger.LogInformation("Failed. Visitor is not Updated.\n");
                }
                else
                {
                    showMessageString = new
                    {
                        ResultCode = 200,
                        Message = string.Format("Visitor is sucessfully updated ", resp.Code),
                        modalType = "Update",
                        Id = pmodel.Id

                    };
                    _logger.LogInformation("Visitor is sucessfully updated ");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update visitor is getting error");
            }
            return Json(showMessageString);
        }

        [HttpPost]
        public async Task<JsonResult> GetLocationUnitIDS(RegistrationViewModel datalist)
        {
            _logger.LogInformation("Get location unit ids list");
            JsonResp jsonResp = new();
            try
            {
                RegistrationViewModel result = new();
                APIRequest req = new()
                {
                    RequestType = "GetLocationUnitIDS",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = datalist
                };
                var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetLocationUnitIDS/", req);
                if (response != null)
                {
                    jsonResp.UnitsDetailLists = response.UnitsDetailLists;
                }
                else
                {
                    jsonResp.UnitsDetailLists = null;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "load location unit ids is getting error");
            }
            return Json(jsonResp);
        }

        [HttpPost]
        public async Task<JsonResult> ValidateUnitID(RegistrationViewModel datalist)
        {
            dynamic showMessageString = string.Empty;
            _logger.LogInformation("Validation unit id");
            try
            {
                RegistrationViewModel result = new();
                APIRequest req = new()
                {
                    RequestType = "ValidateUnitID",
                    UserName = User?.Identity?.Name ?? string.Empty,
                    Message = string.Empty,
                    Model = datalist
                };
                var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/ValidateUnitID/", req);
                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Unit ID is not Valid.\n" + err,
                    };
                    _logger.LogInformation("Unit ID is not Valid.\n");
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("Unit ID is Valid ", resp.Code),
                    };
                    _logger.LogInformation("Unit ID is Valid ");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation unit id is getting error");
            }
            return Json(showMessageString);
        }

        #endregion



        //ViewAllDetails
        [HttpGet]
        public async Task<ActionResult> ViewAllDetails(string id)
        {
            _logger.LogInformation("Get visitor full details");
            RegistrationViewModel? model = new RegistrationViewModel();
            APIRequest req = new()
            {
                UserName = User?.Identity?.Name ?? string.Empty,
                Message = "Get Visitors details",
                RequestString = id,
            };
            var response = await WebAPIHelper.AppRequestAsync("/AdminPortal/ViewAllDetails/", req);

            if (response != null)
            {
                model = response.rmodel;

            }
            //else
            //{
            //    _logger.LogInformation("Invalid input");
            //    string errmsg = (response != null && !string.IsNullOrEmpty(response.Message)) ? response.Message : "Invalid input";
            //    return PartialView(errmsg, null);
            //}
            return PartialView("_ViewVisitorsFullDetails", model);
        }

        //sandesh
        [HttpPost]
        public async Task<ActionResult> UpdateNRICPassport(RegistrationViewModel model)
        {
            dynamic showMessageString = string.Empty;
            _logger.LogInformation("Update NRIC/FIN/Passport ");

            APIRequest req = new()
            {
                RequestType = "Renewal registration",
                UserName = User?.Identity?.Name ?? string.Empty,
                Model = model
            };
            var resp = await WebAPIHelper.AppRequestAsync("/AdminPortal/GetUpdateNRICPassport/", req);
            try
            {
                if (resp != null)
                {
                    if (resp.Code == 1)
                    {
                        string err = resp != null ? resp.Message : string.Empty;
                        showMessageString = new
                        {
                            Code = 200,
                            Message = "Passport/NRIC/FIN is Registration Renewed Successfully." + err,
                        };
                        _logger.LogInformation("Passport/NRIC/FIN is Registration Renewed");
                    }
                    else if (resp.Code == 0)
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = string.Format("Passport/NRIC/FIN is Registration Renewed Failed", resp.Code),
                        };
                        _logger.LogInformation("Passport/NRIC/FIN is Registration Renewed Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Updating Failed");
            }
            return Json(showMessageString);
        }
    }
}
