using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Utilities;
using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.Web.Helper;
using ATTSystems.SFA.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;

namespace ATTSystems.SFA.Web.Controllers
{
    public class KioskController : Controller
    {
        private readonly IConfiguration config;
        private readonly ILogger<KioskController> logger;

        public KioskController(IConfiguration _configuration, ILogger<KioskController> logger)
        {
            config = _configuration;
            this.logger = logger;
        }
        public IActionResult Index()
        {
           
            return View();
        }

        #region English
        public ActionResult SelectLanguage()
        {
            
            return View();
        }
        public ActionResult TermsandConditions()
        {
            return View();
        }
        public ActionResult ScanNRICPassport(KioskViewModel model)
        {
            return View();
        }

        public async Task<ActionResult> LoadVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };
      
            var response = await WebAPIHelper.AppRequestAsync("/Kiosk/GetVisitorTypeList/", req);

            List<KioskViewList>? result = null;
            try
            {
                logger.LogInformation("LoadVisitorType start time" + DateTime.Now.ToString("hh:mm:ss.sss"));

                if (response != null && response.Succeeded == true)
                {
                    result = new List<KioskViewList>();
                    result = response.listKiosk;
                }
                else
                {
                    result = new List<KioskViewList>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LoadVisitorType End time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }

        public async Task<ActionResult> VisitorDetails(string passportId, string NricPassValue, string visitorName,string locatnId)
        {
            KioskViewModel result = new KioskViewModel();

            try
            {
                logger.LogInformation("VisitorDetails start time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                APIRequest request = new APIRequest
                {
                    RequestType = "VisitorDetails",
                    Message = String.Empty
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/GetUnitIDList/", request);


                result.UnitsDetailLists = resp.UnitsDetailLists;

                ViewBag.NRIC = NricPassValue;
                ViewBag.IdType = passportId;
                ViewBag.Name = visitorName;
                ViewBag.LocationId = locatnId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "VisitorDetails end time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                LoggerHelper.Instance.LogError(ex);
            }
            return View(result);
        }

        [HttpPost]
        public async Task<JsonResult> ValidateUnitID(KioskViewModel datalist)
        {
            dynamic showMessageString = string.Empty;
         
            try
            {
                logger.LogInformation("ValidateUnitID start time" + DateTime.Now.ToString("hh:mm:ss.sss"));

                KioskViewModel result = new KioskViewModel();
                APIRequest req = new APIRequest
                {
                    RequestType = "ValidateUnitID",
                    Message = string.Empty,
                    Model = datalist
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/ValidateUnitID/", req);
                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Unit ID is not Valid.\n" + err,
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("Unit ID is Valid ", resp.Code), 
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ("ValidateUnitID end time" + DateTime.Now.ToString("hh: mm:ss.sss")));
                LoggerHelper.Instance.LogError(ex);

            }
            return Json(showMessageString);
        }

        public async Task<ActionResult> CheckNRICPassportNum(KioskViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("CheckPassportNum start time" + DateTime.Now.ToString("hh:mm:ss.sss"));

                if (model != null)
                {
                    APIRequest request = new APIRequest
                    {
                        Model = model,
                    };

                    var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/GetExistingPassportNo/", request);
                    if (resp != null)
                    {
                        if (resp.Code == 2)
                        {
                          
                            if (resp.ID == 8)
                            {
                                showMessageString = new
                                {
                                    Code = 300,
                                    Message = "This Passport/NRIC/FIN has been already Registered. Please proceed to the counter",
                                    ModalType = 8,
                                };
                            }
                            else
                            {
                                showMessageString = new
                                {
                                    Code = 300,
                                    Message = "This Passport/NRIC/FIN has been already Registered. Please proceed to the counter",
                                    result = resp.listKiosk, 
                                };
                            }
                          
                        }
                        else if (resp.Code == 1)
                        {
                         
                            showMessageString = new
                            {
                                Code = 204,
                                Message = "This Passport/NRIC/FIN has been already Registered",
                                ModalType = "Add",
                                result = resp.listKiosk,
                            };
                        }

                        else if (resp.Code == 4)
                        {
                            showMessageString = new
                            {
                                Code = 304,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }

                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = "This Passport/NRIC/FIN is not registered",
                                ModalType = "Add",
                            };
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "Please scan again", 
                            ModalType = "Add",
                        };
                    }
                }
                else
                {
                    string errmsg = string.Empty;

                    showMessageString = new
                    {
                        Code = 201,
                        Message = !string.IsNullOrEmpty(errmsg) ? errmsg.Substring(1) : errmsg,
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CheckPassportNum end time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                LoggerHelper.Instance.LogError(ex);

            }

            return Json(showMessageString);

        }

        [HttpPost]
        public async Task<ActionResult> AddVisitorDetails(KioskViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("AddVisitorDetails start time" + DateTime.Now.ToString("hh:mm:ss.sss"));


                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/AddKioskVisitorDetails/", request);
                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "This Passport/NRIC/FIN has been already Registered",
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        ModalType = "Add",
                        Message = resp.Message
                    };
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddVisitorDetails end time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                LoggerHelper.Instance.LogError(ex);

            }

            return Json(showMessageString);
        }

        public async Task<ActionResult> QRCodeGeneration(string data,string VisitorName)
        {
            var currDatetime = DateTime.Now;
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("QRCodeGeneration start time" + DateTime.Now.ToString("hh:mm:ss.sss"));

                string locationId = config["AppSettings:LocationId"];
                string kioskID = config["AppSettings:KioskId"];
             
                KioskViewModel model = new KioskViewModel
                {
                    NRICPassport = data,
                    IdType = 1
                };
                APIRequest request = new APIRequest
                {
                    Model = model,
                    RequestString = locationId + kioskID,

                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateCardNumber/", request);
                if(resp.Succeeded == true)
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();                                  
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                    ViewBag.QRData = resp.Message;                  
                    ViewBag.Startdate = currDatetime;
                    ViewBag.VisitorName = VisitorName;
                }
                else
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                    // model.QRdate = QRtodatetime;
                    string formattedDate = QRtodatetime.ToString("dd/MM/yyyy HH:mm:ss");
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = "Please proceed to counter for assistance";
                    ViewBag.QRCodeDate = QRtodatetime;
                  
                }
               

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "QRCodeGeneration end time" + DateTime.Now.ToString("hh:mm:ss.sss"));
            }
            return View();
        }
      
        public IActionResult NRICMessage(string name)
        {
            KioskViewModel model= new KioskViewModel();
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);

            string formattedFromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string formattedToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");

            model.NricFromdate = formattedFromDate;
            model.NricTodate = formattedToDate;
            model.visitorName = name;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateKioskVisitorDetails(KioskViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
            model.Fromdate = fromDate;
            model.Todate = toDate;
          

            APIRequest request = new APIRequest
            {
                Model = model,
            };
            var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateKioskVisitorDetails/", request);

            if (model.IdType == 1)
                return View("QRCodeGeneration", model.NRICPassport);
            else
                return View("NRICMessage", model);
        }

        [HttpPost]
        public async Task<ActionResult> PrintTicket(string startdate, string enddate, string QrCode)
        {
            dynamic showMessageString = string.Empty;
            try
            {

                logger.LogInformation("PrintTicket start time" + DateTime.Now.ToString("hh:mm:ss.sss"));
                IPAddress? ipAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
                string kioskIP = GetKioskTerminalIP(ipAddress.ToString());

                string kioskPrinterApiEndpoint = config["AppSettings:KIOSK_CLIENT_PRINTAPI"];

                string kioskPrintApiUrl = string.Format(kioskPrinterApiEndpoint, kioskIP);

                DateTime dateTime = DateTime.Parse(startdate);
                DateTime dateTime2 = DateTime.Parse(enddate);


                string formattedStartTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string formattedEndTime = dateTime2.ToString("yyyy-MM-dd HH:mm:ss");

                QRTicketModel qRTicketModel = new QRTicketModel();
                qRTicketModel.Model = new PrintTicketRequest
                {
                    StartDate = formattedStartTime,
                    EndDate = formattedEndTime,
                    QRdata = QrCode,
                };

                logger.LogInformation("Kiosk IP Address :  \" + kioskPrintApiUr" + DateTime.Now.ToString("hh:mm:ss.sss"));
                string datReq = JsonConvert.SerializeObject(qRTicketModel);
                logger.LogInformation("Print Request Data :  " + datReq);

                var resp = await WebAPIHelper.PrinterAppRequestAsync(kioskPrintApiUrl, "/interface/Print", qRTicketModel);
                if (resp != null)
                {
                    if (resp.Code == 200)
                    {
                        string err = resp != null ? resp.Message : string.Empty;
                        showMessageString = new
                        {
                            Code = 200,
                            Message = "Print QR Ticket Success. Please collect ticket",
                            ModalType = "Add",
                        };

                    }
                    else if (resp.Code == 99)
                    {
                        string err = resp != null ? resp.Message : string.Empty;
                        showMessageString = new
                        {
                            Code = 204,
                            Message = "Failed To Print QR Ticket",
                            ModalType = "Add",
                        };
                    }

                    else
                    {
                        string err = resp != null ? resp.Message : string.Empty;
                        showMessageString = new
                        {
                            Code = 201,
                            Message = "Failed To Print QR Ticket",
                            ModalType = "Add",
                        };

                    }
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Please scan again", 
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ("PrintTicket" + DateTime.Now.ToString("hh: mm:ss.sss")));

            }

            return Json(showMessageString);
        }
        #endregion

        #region Chinese
        public ActionResult ChineseTermsandConditions()
        {
            return View();
        }

        public ActionResult ChinScanNRICPassport(KioskViewModel model)
        {
            return View();
        }
        public async Task<ActionResult> LoadChinVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Kiosk/GetVisitorTypeList/", req);

            List<KioskViewList>? result = null;
            try
            {
                logger.LogInformation("LoadChinVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<KioskViewList>();
                    result = response.listKiosk;
                    var translations = new Dictionary<string, string>
{
                         { "SFA Staff", "SFA Staff" },
    { "Tenants", "租户" },
    { "Workers", "员工" },
    { "Trade Visitors (contractors, commercial buyers, logistics companies)", "贸易访客 (买家)" },
    { "Public", "公众" },
      { "Other Government Agency Staff", "Other Government Agency Staff" },
 { "Managing Agent and Staff", "Managing Agent and Staff" },
};

                    result.ForEach(item =>
                    {
                        if (translations.TryGetValue(item.listvisitorType, out var translation))
                        {
                            item.listvisitorType = translation;

                        }
                        else
                        {
                            item.listvisitorType = string.Empty;
                        }
                    });

                }
                else
                {
                    result = new List<KioskViewList>();
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadChinVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
     
        public async Task<ActionResult> ChinQRCodeGeneration(string data, string VisitorName)
        {
            var currDatetime = DateTime.Now;
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("ChinQRCodeGeneration invoke started");            
                string locationId = config["AppSettings:LocationId"];
                string kioskID = config["AppSettings:KioskId"];
              
                KioskViewModel model = new KioskViewModel
                {
                    NRICPassport = data,
                    IdType = 1
                };
                APIRequest request = new APIRequest
                {
                    Model = model,
                    RequestString = locationId + kioskID,

                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateCardNumber/", request);
                if (resp.Succeeded == true)
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                  
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));                  
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                    ViewBag.QRData = resp.Message;                
                    ViewBag.Startdate = currDatetime;
                    ViewBag.VisitorName = VisitorName;
                }
                else
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                    string formattedDate = QRtodatetime.ToString("dd/MM/yyyy HH:mm:ss");
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = "\r\n请前往柜台寻求帮助";
                    ViewBag.QRCodeDate = QRtodatetime;
                  
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinQRCodeGeneration method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View();
        }

        public IActionResult ChinNRICMessage(KioskViewModel model, string name)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
           
            string formattedFromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string formattedToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");

            model.NricFromdate = formattedFromDate;
            model.NricTodate = formattedToDate;
            model.visitorName = name;

            return View(model);
        }

        public async Task<ActionResult> ChinVisitorDetails(string passportId, string NricPassValue, string visitorName, string locatnId)
        {
            KioskViewModel result = new KioskViewModel();
            try
            {
                logger.LogInformation("ChinVisitorDetails invoke started");
                APIRequest request = new APIRequest
                {
                    RequestType = "ChinVisitorDetails",
                    Message = string.Empty,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/GetUnitIDList/", request);

                result.UnitsDetailLists = resp.UnitsDetailLists;

                ViewBag.NRIC = NricPassValue;
                ViewBag.IdType = passportId;
                ViewBag.Name = visitorName;
                ViewBag.LocationId = locatnId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View(result);
        }
        [HttpPost]
        public async Task<ActionResult> ChinAddVisitorDetails(KioskViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("ChinAddVisitorDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                    
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/AddKioskVisitorDetails/", request);
                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = " Details is Failed.\n" + err,
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        ModalType = "Add",
                        Message = resp.Message
                    };
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinAddVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
              
            }

            return Json(showMessageString);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateKioskChinVisitorDetails(KioskViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
            model.Fromdate = fromDate;
            model.Todate = toDate;
         
            APIRequest request = new APIRequest
            {
                Model = model,
                
            };
            var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateKioskVisitorDetails/", request);

            if (model.IdType == 1)
                return Json(model.NRICPassport);
            else
                return Json( model);
          
        }
        #endregion

        #region Malay

        public ActionResult MalayTermsandConditions()
        {
            return View();
        }
        public ActionResult MalayScanNRICPassport(KioskViewModel model)
        {
            return View();
        }
        public async Task<ActionResult> LoadMalayVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/Kiosk/GetVisitorTypeList/", req);

            List<KioskViewList>? result = null;
            try
            {
                logger.LogInformation("LoadMalayVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<KioskViewList>();
                    result = response.listKiosk;
                    var translations = new Dictionary<string, string>
                {
                  { "SFA Staff", "Kakitangan SFA" },
                { "Tenants", "Penyewa" },
                 { "Workers", "Pekerja" },
                  { "Trade Visitors (contractors, commercial buyers, logistics companies)", "Pelawat Perdagangan (kontraktor, pembeli komersial, syarikat logistik)" },
                  { "Public", "Awam" },
                        { "Other Government Agency Staff", "Kakitangan Agensi Kerajaan yang lain" },
                  { "Managing Agent and Staff", "Ejen dan Kakitangan Pengurusan" },
                    };

                    result.ForEach(item =>
                    {
                        if (translations.TryGetValue(item.listvisitorType, out var translation))
                        {
                            item.listvisitorType = translation;

                        }
                        else
                        {
                            item.listvisitorType = string.Empty;
                        }
                    });

                }
                else
                {
                    result = new List<KioskViewList>();
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadMalayVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
     
        public async Task<ActionResult> MalayQRCodeGeneration(string data)
        {
            var currDatetime = DateTime.Now;
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("MalayQRCodeGeneration invoke started");
                
                string locationId = config["AppSettings:LocationId"];
                string kioskID = config["AppSettings:KioskId"];
              
                KioskViewModel model = new KioskViewModel
                {
                    NRICPassport = data,
                    IdType = 1
                };
                APIRequest request = new APIRequest
                {
                    Model = model,
                    RequestString = locationId + kioskID,

                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateCardNumber/", request);
                if (resp.Succeeded == true)
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                   
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                    ViewBag.QRData = resp.Message;                  
                    ViewBag.Startdate = currDatetime;
                }
                else
                {
                    QRCodeGenerator QrGenerator = new QRCodeGenerator();
                    QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                    QRCode QrCode = new QRCode(QrCodeInfo);
                    Bitmap QrBitmap = QrCode.GetGraphic(20);
                    byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                    // model.QRdate = QRtodatetime;
                    string formattedDate = QRtodatetime.ToString("dd/MM/yyyy HH:mm:ss");
                    string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));
                    ViewBag.QrCodeUri = "\r\nSila teruskan ke kaunter untuk mendapatkan bantuan";
                     ViewBag.QRCodeDate = QRtodatetime;
                   
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayQRCodeGeneration method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View();
        }
        public IActionResult MalayNRICMessage(KioskViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
          
            string formattedFromDate = fromDate.ToString("dd/MM/yyyy HH:mm:ss");
            string formattedToDate = toDate.ToString("dd/MM/yyyy HH:mm:ss");

            model.NricFromdate = formattedFromDate;
            model.NricTodate = formattedToDate;

            return View(model);
        }

        public async Task<ActionResult> MalayVisitorDetails(string passportId, string NricPassValue, string visitorName)
        {
            KioskViewModel result = new KioskViewModel();
            try
            {
                logger.LogInformation("MalayVisitorDetails invoke started");

                APIRequest request = new APIRequest
                {
                    RequestType = "MalayVisitorDetails",
                    Message = String.Empty
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/GetUnitIDList/", request);

                result.UnitsDetailLists = resp.UnitsDetailLists;

                ViewBag.NRIC = NricPassValue;
                ViewBag.IdType = passportId;
                ViewBag.Name = visitorName;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View(result);
        }

        [HttpPost]
        public async Task<ActionResult> MalayAddVisitorDetails(KioskViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("MalayAddVisitorDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/AddKioskVisitorDetails/", request);
                if (resp == null || !resp.Succeeded)
                {
                    string err = resp != null ? resp.Message : string.Empty;
                    showMessageString = new
                    {
                        Code = 300,
                        Message = " Details is Failed.\n" + err,
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        ModalType = "Add",
                        Message = resp.Message
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayAddVisitorDetails method");
                LoggerHelper.Instance.LogError(ex);
              
            }

            return Json(showMessageString);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateKioskMalayVisitorDetails(KioskViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
            model.Fromdate = fromDate;
            model.Todate = toDate;
           
            APIRequest request = new APIRequest
            {
                Model = model,
            };
            var resp = await WebAPIHelper.AppRequestAsync("/Kiosk/UpdateKioskVisitorDetails/", request);

            if (model.IdType == 1)
                return View("MalayQRCodeGeneration", model.NRICPassport);
            else
                return View("MalayNRICMessage", model);
        }
        #endregion

        public ActionResult ScanNRICID(string NricPass)
        {

            ViewBag.NRIC = NricPass;
            return View();
        }
        public ActionResult ThankyouPage()
        {
            return View();
        }
        public ActionResult SuccessPage()
        {
            return View();
        }
        public ActionResult KioskStart()
        {
            return View();
        }

        private string GetKioskTerminalIP(string hostAddress)
        {
            string clientAddress = "";
            try
            {

                if (hostAddress == "::1")
                {
                    clientAddress = "127.0.0.1";
                }
                else
                {
                    clientAddress = hostAddress;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Instance.TraceLog(string.Format("Level: Warning, Controller: Kiosk, Action: isKioskRegister, Message: {0} ", ex.Message));
            }
            return clientAddress;


        }
    }
    public static class BitmapExtension
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
