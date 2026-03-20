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

        // Action gửi email cảnh báo thủ công (gọi từ nút trong Dashboard)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendExpirationAlerts()
        {
            var now = DateTime.Now.Date;
            const int daysThreshold = 30;

            // Sử dụng DbFunctions để tính khoảng cách ngày (tránh .AddDays trong query)
            var devicesNeedingAttention = db.Devices
                .Include(d => d.Emails)
                .Where(d => d.NextInspectionDate.HasValue &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) <= daysThreshold &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) >= -365)
                .ToList();

            if (!devicesNeedingAttention.Any())
            {
                TempData["EmailMessage"] = "success|Không có thiết bị nào sắp hoặc quá hạn kiểm định.";
                return RedirectToAction("Index");
            }

            // Nhóm theo email
            var emailGroups = new Dictionary<string, List<Device>>();

            foreach (var device in devicesNeedingAttention)
            {
                if (device.Emails == null || !device.Emails.Any()) continue;

                foreach (var emailRec in device.Emails)
                {
                    string emailAddr = emailRec.Email.Trim().ToLower();
                    if (!emailGroups.ContainsKey(emailAddr))
                    {
                        emailGroups[emailAddr] = new List<Device>();
                    }
                    emailGroups[emailAddr].Add(device);
                }
            }

            if (!emailGroups.Any())
            {
                TempData["EmailMessage"] = "warning|Không có email nào được liên kết với các thiết bị sắp/quá hạn.";
                return RedirectToAction("Index");
            }

            int successCount = 0;
            string smtpHost = "twsmtpsrv01.adgroup.com.tw";
            int smtpPort = 25;

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.EnableSsl = false;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                foreach (var group in emailGroups)
                {
                    string recipient = group.Key;
                    var deviceList = group.Value;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("service.app@adgroup.com.tw"),
                        Subject = "Cảnh báo: Thiết bị sắp quá hạn kiểm định",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(recipient);
                    //mailMessage.CC.Add("VNCSR@adgroup.com.tw");

                    var deviceTable = "<table border='1' cellpadding='8' style='border-collapse: collapse; width:100%;'>";
                    deviceTable += "<tr><th>Mã Hồ Sơ</th><th>Mã Thiết Bị</th><th>Tên Thiết Bị</th><th>Ngày kiểm định tiếp theo</th><th>Trạng thái</th></tr>";

                    foreach (var device in deviceList)
                    {
                        int daysLeft = (device.NextInspectionDate.Value - now).Days;
                        string status = daysLeft < 0 ? "Quá hạn" : $"Còn {daysLeft} ngày";
                        string statusColor = daysLeft < 0 ? "red" : "orange";

                        deviceTable += $"<tr>" +
                                       $"<td>{device.HoSoCode}</td>" +
                                       $"<td>{device.DeviceCode}</td>" +
                                       $"<td>{device.DeviceName}</td>" +
                                       $"<td>{device.NextInspectionDate.Value:yyyy/MM/dd}</td>" +
                                       $"<td style='color:{statusColor}; font-weight:bold;'>{status}</td>" +
                                       $"</tr>";
                    }
                    deviceTable += "</table>";

                    mailMessage.Body = $@"
                                    <h3>Thông báo từ CSR Equipment Manager</h3>
                                    <p>Các thiết bị dưới đây sắp quá hạn kiểm định. Vui lòng kiểm tra và xử lý sớm:</p>
                                    {deviceTable}
                                    <p>Trân trọng,<br/>Hệ thống CSR Equipment Manager</p>";

                    try
                    {
                        smtpClient.Send(mailMessage);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi gửi mail tới {recipient}: {ex.Message}");
                    }
                }
            }

            TempData["EmailMessage"] = $"success|Đã gửi cảnh báo thành công đến {successCount} email.";
            return RedirectToAction("Index");
        }
    }
}