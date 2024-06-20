using ATTSystems.NetCore.Logger;
using ATTSystems.NetCore.Model.HttpModel;
using ATTSystems.NetCore.Model.ViewModel;
using ATTSystems.SFA.Model.ViewModel;
using ATTSystems.SFA.Web.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Xml.Linq;

namespace ATTSystems.SFA.Web.Controllers
{
    public class OnlinePortalController : Controller
    {

        private readonly IConfiguration config;
        private readonly ILogger<OnlinePortalController> logger;

        public OnlinePortalController(IConfiguration _configuration, ILogger<OnlinePortalController> logger)
        {
            config = _configuration;
            this.logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region Nikitha - Passport
        public ActionResult LocationSelector()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        public async Task<ActionResult> CheckPassportNum(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("CheckPassportNum invoke started");

                if (model != null)
                {
                    APIRequest request = new APIRequest
                    {
                        Model = model,
                    };

                    var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetExistingPassportNo/", request);

                    if (resp != null)
                    {
                        if (resp.Code == 2)
                        {
                            showMessageString = new
                            {
                                Code = 300,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else if (resp.Code == 1)
                        {
                            showMessageString = new
                            {
                                Code = 204,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 3)
                        {
                            showMessageString = new
                            {
                                Code = 300,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else if (resp.Code == 4)
                        {
                            showMessageString = new
                            {
                                Code = 201,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 5)
                        {
                            showMessageString = new
                            {
                                Code = 202,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }

                        else if (resp.Code == 6)
                        {
                            showMessageString = new
                            {
                                Code = 304,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }

                        else if (resp.Code == 7)
                        {
                            showMessageString = new
                            {
                                Code = 305,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = "This Passport has not been Registered before, Please key-in details to Register",
                                ModalType = "Add",
                            };
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "Details Failed",
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
                logger.LogError(ex, "Error in CheckPassportNum method");
                LoggerHelper.Instance.LogError(ex);
            }


            return Json(showMessageString);

        }

        public async Task<ActionResult> LoadLocation()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadLocation",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetLocationList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("LoadLocation invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<OnlinePortalViewList>();
                    result = response.listOnlinePort;
                }
                else
                {
                    result = new List<OnlinePortalViewList>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadLocation method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        public async Task<ActionResult> LoadVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetVisitorTypeList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("LoadVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<OnlinePortalViewList>();
                    result = response.listOnlinePort;
                }
                else
                {
                    result = new List<OnlinePortalViewList>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        public async Task<ActionResult> Registerdtls(int locId)
        {

            OnlinePortalViewModel model = new OnlinePortalViewModel()
            {
                locationid = locId
            };
            try
            {
                logger.LogInformation("Registerdtls invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetUnitIDList/", request);

                ViewBag.LocationId = Convert.ToString(locId);

                if (resp != null && resp.OnlineUnitsDetailLists != null)
                {
                    model.OnlineUnitsDetailLists = resp.OnlineUnitsDetailLists;
                }

            }
            catch (Exception ex)
            {

                logger.LogError(ex, "Error in Registerdtls method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> ValidateUnitID(OnlinePortalViewModel datalist)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("ValidateUnitID invoke started");

                OnlinePortalViewModel result = new OnlinePortalViewModel();
                APIRequest req = new APIRequest
                {
                    RequestType = "ValidateUnitID",
                    Message = string.Empty,
                    Model = datalist
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/ValidateUnitID/", req);
                if (resp == null || !resp.Succeeded)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Unit ID is not Valid",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("Unit ID is Valid "),

                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ValidateUnitID method");
                LoggerHelper.Instance.LogError(ex);

            }
            return Json(showMessageString);
        }

        [HttpPost]
        public async Task<ActionResult> AddVisitorPassportDetails(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("AddVisitorPassportDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/AddNewVisitorPassport/", request);


                if (resp == null || !resp.Succeeded)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = " Passport Registration is Failed.",
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("New Visitor is Added"),
                        ModalType = "Add",
                    };
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddVisitorPassportDetails method");
                LoggerHelper.Instance.LogError(ex);

            }
            return Json(showMessageString);
        }

        public async Task<ActionResult> QRCodeGeneration(string data, string name)
        {
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("QRCodeGeneration invoke started");

                OnlinePortalViewModel model = new OnlinePortalViewModel
                {
                    passportNumber = data,
                };

                APIRequest request = new APIRequest
                {
                    Model = model,
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/UpdateCardNumber/", request);
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                Bitmap QrBitmap = QrCode.GetGraphic(20);
                byte[] BitmapArray = QrBitmap.BitmapToByteArray();

                string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));

                if (resp.Succeeded)
                {
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                    ViewBag.VisitorName = name;
                }
                else
                {
                    QrUri = "Please proceed to counter for assistance" + QrUri;
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;

                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in QRCodeGeneration method");
                LoggerHelper.Instance.LogError(ex);
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePassportVisitorDetails(OnlinePortalViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
            model.Fromdate = fromDate;
            model.Todate = toDate;

            try
            {
                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/UpdatePassportVisitorDetails/", request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdatePassportVisitorDetails method");
            }
            return Json(model.passportNumber);
        }
        #endregion

        #region Chinese Nikitha

        public ActionResult ChinLocationSelector()
        {
            return View();
        }

        public async Task<ActionResult> ChinRegisterDetails(int locId)
        {
            OnlinePortalViewModel model = new OnlinePortalViewModel()
            {
                locationid = locId
            };
            try
            {
                logger.LogInformation("ChinRegisterDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetUnitIDList/", request);

                ViewBag.LocationId = Convert.ToString(locId);

                if (resp != null && resp.OnlineUnitsDetailLists != null)
                {
                    model.OnlineUnitsDetailLists = resp.OnlineUnitsDetailLists;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinRegisterDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View(model);
        }

        public async Task<ActionResult> ChinCheckPassportNum(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("ChinCheckPassportNum invoke started");

                if (model != null)
                {
                    APIRequest request = new APIRequest
                    {
                        Model = model,
                    };

                    var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetExistingPassportNo/", request);

                    if (resp != null)
                    {
                        if (resp.Code == 2)
                        {
                            showMessageString = new
                            {
                                Code = 300,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else if (resp.Code == 1)
                        {
                            showMessageString = new
                            {
                                Code = 204,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 3)
                        {
                            showMessageString = new
                            {
                                Code = 301,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else if (resp.Code == 4)
                        {
                            showMessageString = new
                            {
                                Code = 201,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 5)
                        {
                            showMessageString = new
                            {
                                Code = 202,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }

                        else if (resp.Code == 6)
                        {
                            showMessageString = new
                            {
                                Code = 304,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }

                        else if (resp.Code == 7)
                        {
                            showMessageString = new
                            {
                                Code = 305,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = string.Format("This Passport has not been Registered before, Please key-in details to Register"),
                                ModalType = "Add",
                            };
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "Details Failed",
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
                logger.LogError(ex, "Error in ChinCheckPassportNum method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }

        public async Task<ActionResult> LoadChinVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetVisitorTypeList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("LoadChinVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = response.listOnlinePort;
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


                    result?.ForEach(item =>
                    {
                        if (item.listvisitorType != null && translations.TryGetValue(item.listvisitorType, out var translation))
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
                    result = new List<OnlinePortalViewList>();
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadChinVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> ChinAddVisitorPassportDetails(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("ChinAddVisitorPassportDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/AddNewVisitorPassport/", request);


                if (resp == null || !resp.Succeeded)
                {

                    showMessageString = new
                    {
                        Code = 300,
                        Message = " 详细信息失败",
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("添加了新访客"),
                        ModalType = "Add",
                    };
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinAddVisitorPassportDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);
        }
      
        public async Task<ActionResult> ChinQRCodeGeneration(string data, string name)
        {
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("ChinQRCodeGeneration invoke started");

                OnlinePortalViewModel model = new OnlinePortalViewModel
                {
                    passportNumber = data,
                };

                APIRequest request = new APIRequest
                {
                    Model = model,
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/UpdateCardNumber/", request);
                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                Bitmap QrBitmap = QrCode.GetGraphic(20);
                byte[] BitmapArray = QrBitmap.BitmapToByteArray();

                string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));

                if (resp.Succeeded)
                {
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                    ViewBag.VisitorName = name;
                }
                else
                {
                    QrUri = "Please proceed to counter for assistance" + QrUri;
                    ViewBag.QrCodeUri = QrUri;
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

        public async Task<ActionResult> ChinNricdtlsave(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("ChinNricdtlsave invoke started");

                APIRequest req = new APIRequest
                {

                    Model = model
                };
                string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/OnlinePortal/Nricdtlsaveinfo/", req);

                if (resp != null)
                {
                    if (resp.Code == -1)
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "NRIC is not created",
                        };
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 200,
                            Message = string.Format("NRIC is existed."),
                        };
                    }
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "NRIC is not created",
                    };
                }              
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinNricdtlsave method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }

        public async Task<ActionResult> ChinNricverify(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("ChinNricverify invoke started");

                APIRequest req = new APIRequest
                {
                    Model = model
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/Nricverifyno/", req);

                if (resp != null)
                {
                    if (resp.Code == 2)
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = resp.Message,
                            ModalType = "Add",

                        };
                    }
                    else if (resp.Code == 1)
                    {
                        showMessageString = new
                        {
                            Code = 204,
                            Message = resp.Message,
                            ModalType = "Add",
                            res = resp.listOnlinePort,
                        };
                    }
                    else if (resp.Code == 3)
                    {
                        showMessageString = new
                        {
                            Code = 301,
                            Message = resp.Message,
                            ModalType = "Add",
                        };
                    }
                    else if (resp.Code == 4)
                    {
                        showMessageString = new
                        {
                            Code = 201,
                            Message = resp.Message,
                            ModalType = "Add",
                            res = resp.listOnlinePort,
                        };
                    }
                    else if (resp.Code == 5)
                    {
                        showMessageString = new
                        {
                            Code = 202,
                            Message = resp.Message,
                            ModalType = "Add",
                            res = resp.listOnlinePort,
                        };
                    }

                 
                    else if (resp.Code == 6)
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
                            Message = string.Format("This NRIC/FIN has not been Registered before, Please key-in details to Register"),
                            ModalType = "Add",
                        };
                    }
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "Details Failed",
                        ModalType = "Add",
                    };
                }
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ChinNricverify method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }

        public IActionResult ChinNricconfirm(OnlinePortalViewModel model, string fullnam)
        {
            DateTime fmDateTime = DateTime.Now;
            DateTime toDateTime = fmDateTime.AddYears(+1);
            model.Fromdate = fmDateTime;
            model.Todate = toDateTime;
            model.visitorName = fullnam;

            return View(model);
        }

        #endregion

        #region Malay Nikitha

        public ActionResult MalayLocationSelector()
        {
            return View();
        }

        public async Task<ActionResult> MalayRegisterDetails(int locId)
        {

            OnlinePortalViewModel model = new OnlinePortalViewModel()
            {
                locationid = locId
            };
            try
            {
                logger.LogInformation("MalayRegisterDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetUnitIDList/", request);

                ViewBag.LocationId = Convert.ToString(locId);

                if (resp != null && resp.OnlineUnitsDetailLists != null)
                {
                    model.OnlineUnitsDetailLists = resp.OnlineUnitsDetailLists;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayRegisterDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return View(model);
        }

        public async Task<ActionResult> MalayCheckPassportNum(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("MalayCheckPassportNum invoke started");

                if (model != null)
                {
                    APIRequest request = new APIRequest
                    {
                        Model = model,
                    };

                    var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetExistingPassportNo/", request);

                    if (resp != null)
                    {
                        if (resp.Code == 2)
                        {
                            showMessageString = new
                            {
                                Code = 300,
                                Message = resp.Message,
                                ModalType = "Add",

                            };
                        }
                        else if (resp.Code == 1)
                        {
                            showMessageString = new
                            {
                                Code = 204,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 3)
                        {
                            showMessageString = new
                            {
                                Code = 301,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else if (resp.Code == 4)
                        {
                            showMessageString = new
                            {
                                Code = 201,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }
                        else if (resp.Code == 5)
                        {
                            showMessageString = new
                            {
                                Code = 202,
                                Message = resp.Message,
                                ModalType = "Add",
                                result = resp.listOnlinePort,
                            };
                        }

                        else if (resp.Code == 6)
                        {
                            showMessageString = new
                            {
                                Code = 304,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }

                        else if (resp.Code == 7)
                        {
                            showMessageString = new
                            {
                                Code = 305,
                                Message = resp.Message,
                                ModalType = "Add",
                            };
                        }
                        else
                        {
                            showMessageString = new
                            {
                                Code = 200,
                                Message = string.Format("This Passport has not been Registered before, Please key-in details to Register"),
                                ModalType = "Add",
                            };
                        }
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "Details Failed",
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
                logger.LogError(ex, "Error in MalayCheckPassportNum method");
                LoggerHelper.Instance.LogError(ex);
            }

            return Json(showMessageString);

        }

        public async Task<ActionResult> LoadMalayVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "LoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetVisitorTypeList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("LoadMalayVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = response.listOnlinePort;
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


                    result?.ForEach(item =>
                    {
                        if (item.listvisitorType != null && translations.TryGetValue(item.listvisitorType, out var translation))
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
                    result = new List<OnlinePortalViewList>();
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LoadMalayVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> MalayAddVisitorPassportDetails(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;
            try
            {
                logger.LogInformation("MalayAddVisitorPassportDetails invoke started");

                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/AddNewVisitorPassport/", request);


                if (resp == null || !resp.Succeeded)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = " Butiran Gagal",
                        ModalType = "Add",
                    };
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 200,
                        Message = string.Format("\r\nPelawat baharu ditambah"),
                        ModalType = "Add",
                    };
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayAddVisitorPassportDetails method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);
        }

        public async Task<ActionResult> MalayQRCodeGeneration(string data)
        {
            var QRtodatetime = DateTime.Now.AddDays(1);

            try
            {
                logger.LogInformation("MalayQRCodeGeneration invoke started");

                OnlinePortalViewModel model = new OnlinePortalViewModel
                {
                    passportNumber = data,
                };

                APIRequest request = new APIRequest
                {
                    Model = model,
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/UpdateCardNumber/", request);

                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(resp.Message, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                Bitmap QrBitmap = QrCode.GetGraphic(20);
                byte[] BitmapArray = QrBitmap.BitmapToByteArray();
                string QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(BitmapArray));

                if (resp.Succeeded)
                {
                    ViewBag.QrCodeUri = QrUri;
                    ViewBag.QRCodeDate = QRtodatetime;
                }
                else
                {
                    ViewBag.QrCodeUri = "Please proceed to counter for assistance" + QrUri;
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


        public async Task<ActionResult> MalayNricdtlsave(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("MalayNricdtlsave invoke started");

                APIRequest req = new APIRequest
                {
                    Model = model
                };
                string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/OnlinePortal/Nricdtlsaveinfo/", req);


                if (resp != null)
                {
                    if (resp.Code == -1)
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "NRIC is not created",
                        };
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 200,
                            Message = string.Format("NRIC is existed."),
                        };
                    }
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "NRIC is not created",
                    };
                }
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayNricdtlsave method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }

        public async Task<ActionResult> MalayNricverify(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("MalayNricverify invoke started");

                APIRequest req = new APIRequest
                {

                    Model = model
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/Nricverifyno/", req);
                if (resp.Code == 2)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = resp.Message,
                        ModalType = "Add",

                    };
                }
                else if (resp.Code == 1)
                {
                    showMessageString = new
                    {
                        Code = 204,
                        Message = resp.Message,
                        ModalType = "Add",
                        res = resp.listOnlinePort,
                    };
                }
                else if (resp.Code == 3)
                {
                    showMessageString = new
                    {
                        Code = 301,
                        Message = resp.Message,
                        ModalType = "Add",
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
                        Message = string.Format("This Passport has not been Registered before, Please key-in details to Register"),
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in MalayNricverify method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }

        public IActionResult MalayNricconfirm(OnlinePortalViewModel model)
        {
            DateTime fmDateTime = DateTime.Now;
            DateTime toDateTime = fmDateTime.AddYears(+1);
            model.Fromdate = fmDateTime;
            model.Todate = toDateTime;

            return View(model);
        }

        #endregion


        #region NRIC
        public async Task<ActionResult> Nricverify(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("Nricverify invoke started");

                APIRequest req = new APIRequest
                {
                    RequestType = "",
                    Model = model
                };

                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/Nricverifyno/", req);
                if (resp.Code == 2)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = resp.Message,
                        ModalType = "Add",
                    };
                }
                else if (resp.Code == 1)
                {
                    showMessageString = new
                    {
                        Code = 204,
                        Message = resp.Message,
                        ModalType = "Add",
                        res = resp.listOnlinePort,
                    };
                }
                else if (resp.Code == 3)
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = resp.Message,
                        ModalType = "Add",
                    };
                }
                else if (resp.Code == 4)
                {
                    showMessageString = new
                    {
                        Code = 201,
                        Message = resp.Message,
                        ModalType = "Add",
                        res = resp.listOnlinePort,
                    };
                }
                else if (resp.Code == 5)
                {
                    showMessageString = new
                    {
                        Code = 202,
                        Message = resp.Message,
                        ModalType = "Add",
                        res = resp.listOnlinePort,
                    };
                }

                else if (resp.Code == 6)
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
                        Message = string.Format("This NRIC/FIN has not been Registered before, Please key-in details to Register"),
                        ModalType = "Add",
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Nricverify method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }
        public async Task<ActionResult> NricLoadVisitorType()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "NricLoadVisitorType",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetNricVisitorTypeList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("NricLoadVisitorType invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<OnlinePortalViewList>();
                    result = response.listOnlinePort;
                }
                else
                {
                    result = new List<OnlinePortalViewList>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in NricLoadVisitorType method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        public async Task<ActionResult> NricLoadLocation()
        {
            APIRequest req = new APIRequest
            {
                RequestType = "NricLoadLocation",
                Message = String.Empty
            };

            var response = await WebAPIHelper.AppRequestAsync("/OnlinePortal/GetNricLocationList/", req);

            List<OnlinePortalViewList>? result = null;
            try
            {
                logger.LogInformation("NricLoadLocation invoke started");

                if (response != null && response.Succeeded == true)
                {
                    result = new List<OnlinePortalViewList>();
                    result = response.listOnlinePort;
                }
                else
                {
                    result = new List<OnlinePortalViewList>();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in NricLoadLocation method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(result);
        }
        [HttpPost]
        public async Task<ActionResult> Nricdtlsave(OnlinePortalViewModel model)
        {
            dynamic showMessageString = string.Empty;

            try
            {
                logger.LogInformation("Nricdtlsave invoke started");

                APIRequest req = new APIRequest
                {
                    RequestType = "",

                    Model = model
                };
                string baseuri = config.GetValue<string>("AppSettings:APIBaseUri");
                var resp = await WebAPIHelper.APIRequestAsync(baseuri, "/OnlinePortal/Nricdtlsaveinfo/", req);

                if (resp != null)
                {
                    if (resp.Code == -1)
                    {
                        showMessageString = new
                        {
                            Code = 300,
                            Message = "NRIC Registration is Failed",
                        };
                    }
                    else
                    {
                        showMessageString = new
                        {
                            Code = 200,
                            Message = string.Format("NRIC is existed."),
                        };
                    }
                }
                else
                {
                    showMessageString = new
                    {
                        Code = 300,
                        Message = "NRIC Registration is Failed",
                    };
                }
               
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Nricdtlsave method");
                LoggerHelper.Instance.LogError(ex);
            }
            return Json(showMessageString);

        }
        public IActionResult Nricconfirm(OnlinePortalViewModel model, string fullnam)
        {
            DateTime fmDateTime = DateTime.Now;
            DateTime toDateTime = fmDateTime.AddYears(+1);
            model.Fromdate = fmDateTime;
            model.Todate = toDateTime;
            model.visitorName = fullnam;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateNRICVisitorDetails(OnlinePortalViewModel model)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = fromDate.AddYears(+1);
            model.Fromdate = fromDate;
            model.Todate = toDate;

            try
            {
                APIRequest request = new APIRequest
                {
                    Model = model,
                };
                var resp = await WebAPIHelper.AppRequestAsync("/OnlinePortal/UpdateNRICVisitorDetails/", request);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UpdateNRICVisitorDetails method");
            }
            return Json(model.NRICNumber);
        }

        #endregion
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
