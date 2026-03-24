using CSR_EquipmentManager.Data;
using CSR_EquipmentManager.Models;
using CSR_EquipmentManager.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using System.Data.Entity;

namespace CSR_EquipmentManager.Controllers
{
    public class DashBoardController : Controller
    {
        private DeviceDbContext db = new DeviceDbContext();

        public ActionResult Index()
        {
            var now = DateTime.Now.Date;

            var devices = db.Devices
                .Where(d => d.NextInspectionDate.HasValue)
                .ToList();

            var model = new DashboardVM
            {
                TotalDevices = db.Devices.Count(),

                ExpiredDevices = devices.Count(d => d.NextInspectionDate.Value < now),

                ExpiringSoonDevices = devices.Count(d =>
                    d.NextInspectionDate.Value >= now &&
                    (d.NextInspectionDate.Value - now).Days <= 30),

                ValidDevices = devices.Count(d =>
                    d.NextInspectionDate.Value > now &&
                    (d.NextInspectionDate.Value - now).Days > 30)
            };

            return View(model);
        }

        // ====================== GỬI EMAIL CẢNH BÁO ======================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendExpirationAlerts()
        {
            var result = SendExpirationAlertsInternal();
            return Json(new { success = result.success, message = result.message });
        }

        // Action dùng cho Task Scheduler gọi tự động
        [AllowAnonymous]
        public ActionResult SendExpirationAlertsAuto()
        {
            var result = SendExpirationAlertsInternal();

            string message = result.success
                ? $"[{DateTime.Now}] Gửi email thành công: {result.message}"
                : $"[{DateTime.Now}] Lỗi: {result.message}";

            // Ghi log ra file để dễ kiểm tra
            string logPath = Server.MapPath("~/App_Data/auto_email.log");
            System.IO.File.AppendAllText(logPath, message + Environment.NewLine);

            return Content(message);
        }

        // Logic chính
        private (bool success, string message) SendExpirationAlertsInternal()
        {
            var now = DateTime.Now.Date;
            const int daysThreshold = 30;

            // Lấy thiết bị sắp hết hạn (1 ~ 30 ngày)
            var devices = db.Devices
                .Include(d => d.Emails)
                .Where(d => d.NextInspectionDate.HasValue &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) > 0 &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) <= daysThreshold)
                .ToList();

            if (!devices.Any())
                return (true, "Không có thiết bị nào sắp hết hạn (1-30 ngày).");

            // Tạo danh sách email nhận thông báo (unique)
            var allEmails = new HashSet<string>();

            foreach (var device in devices)
            {
                if (device.Emails == null) continue;
                foreach (var e in device.Emails)
                {
                    if (!string.IsNullOrWhiteSpace(e.Email))
                        allEmails.Add(e.Email.Trim().ToLower());
                }
            }

            if (!allEmails.Any())
                return (false, "Không có email nào được liên kết với thiết bị sắp hết hạn.");

            int successCount = 0;
            string logPath = Server.MapPath("~/App_Data/email_debug.log");
            string smtpHost = "twsmtpsrv01.adgroup.com.tw";
            int smtpPort = 25;

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.EnableSsl = false;
                smtpClient.UseDefaultCredentials = true;

                // Tạo bảng thiết bị một lần
                var deviceTable = "<table border='1' cellpadding='8' style='border-collapse: collapse; width:100%;'>";
                deviceTable += "<tr style='background:#f0f0f0;'><th>Mã HS</th><th>Mã Thiết Bị</th><th>Tên Thiết Bị</th><th>Ngày KT Tiếp Theo</th><th>Còn lại</th></tr>";

                foreach (var device in devices)
                {
                    int daysLeft = (device.NextInspectionDate.Value - now).Days;
                    deviceTable += $"<tr>" +
                                   $"<td>{device.HoSoCode}</td>" +
                                   $"<td>{device.DeviceCode}</td>" +
                                   $"<td>{device.DeviceName}</td>" +
                                   $"<td>{device.NextInspectionDate.Value:yyyy/MM/dd}</td>" +
                                   $"<td style='color:orange; font-weight:bold;'>Còn {daysLeft} ngày</td>" +
                                   $"</tr>";
                }
                deviceTable += "</table>";

                var body = $@"
                    <h3>Thông báo từ CSR Equipment Manager</h3>
                    <p>Các thiết bị dưới đây sắp hết hạn kiểm định. Vui lòng kiểm tra và xử lý sớm:</p>
                    {deviceTable}
                    <p><small>Thời gian kiểm tra: {DateTime.Now:dd/MM/yyyy HH:mm}</small></p>
                    <p>Trân trọng,<br/>Hệ thống CSR Equipment Manager</p>";

                // Gửi 1 email cho tất cả email liên quan
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("service.app@adgroup.com.tw"),
                    Subject = "Cảnh báo: Thiết bị sắp hết hạn kiểm định",
                    IsBodyHtml = true,
                    Body = body
                };

                // Thêm tất cả email vào TO
                foreach (var email in allEmails)
                {
                    mailMessage.To.Add(email);
                }

                //mailMessage.CC.Add("VNCSR@adgroup.com.tw");

                try
                {
                    smtpClient.Send(mailMessage);
                    successCount = allEmails.Count;

                    System.IO.File.AppendAllText(logPath,
                        $"{DateTime.Now}: Gửi thành công {successCount} email (tổng {devices.Count} thiết bị)\n");
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(logPath,
                        $"{DateTime.Now}: LỖI gửi email - {ex.Message}\n");
                    return (false, "Có lỗi xảy ra khi gửi email: " + ex.Message);
                }
            }

            return (true, $"Đã gửi cảnh báo thành công đến {successCount} email (tổng {devices.Count} thiết bị).");
        }
    }
}