using ATTSystems.SFA.Model.DBModel;
using Microsoft.Extensions.Logging;
using QRCoder;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATTSystems.SFA.DAL
{
    public class Message
    {
        private ILogger _logger;
        public Message(ILogger logger)
        {
            _logger = logger;
        }

        public string GetMessageString(DataContext entity)
        {
            string msg1 = string.Empty;
            var res = entity.AlertMessage.FirstOrDefault(x => x.Id == 1 && x.IsEnabled == false && x.IsDeleted == false);
            if (res != null)
            {
                msg1 = res.MessageString ?? string.Empty;
            }

            return msg1;
        }
        public void InsertMessage(string encDataPass, string email, DataContext entity)
        {
            var QRtodatetime = DateTime.Now.AddHours(24);
            try
            {
                string messageTemplate = GetMessageString(entity);

                QRCodeGenerator QrGenerator = new QRCodeGenerator();
                QRCodeData QrCodeInfo = QrGenerator.CreateQrCode(encDataPass, QRCodeGenerator.ECCLevel.Q);
                QRCode QrCode = new QRCode(QrCodeInfo);
                Bitmap QrBitmap = QrCode.GetGraphic(20);
                byte[] BitmapArray;
                using (MemoryStream ms = new MemoryStream())
                {
                    QrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    BitmapArray = ms.ToArray();
                }
                string base64Image = Convert.ToBase64String(BitmapArray);
                string QrUri = $"data:image/png;base64,{base64Image}";
                int qrCodeWidth = 100;
                int qrCodeHeight = 100;
                string _img = $"<img src='{QrUri}' width='{qrCodeWidth}' height='{qrCodeHeight}'/>";
                string formattedDate = QRtodatetime.ToString("dd/MM/yyyy HH:mm:ss tt");
                string replaceMessage = messageTemplate.Replace("<QR code>", _img).Replace("<DATE>", formattedDate);


                _logger.LogInformation("Logs QR code messages");
                MessageLogs logs = new MessageLogs();
                logs.Message = replaceMessage;
                logs.CardNumber = encDataPass;
                logs.Recipient = email;
                logs.SentStatus = 0;
                logs.CreatedDateTime = DateTime.Now;
                logs.IsPushed = false;
                entity.MessageLogs.Add(logs);
                entity.SaveChanges();
                _logger.LogInformation("QR code message log created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error message logs");
            }
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
