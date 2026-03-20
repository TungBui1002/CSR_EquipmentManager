using CSR_EquipmentManager.Data;
using CSR_EquipmentManager.Models;
using OfficeOpenXml;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CSR_EquipmentManager.Controllers
{
    public class DeviceEmailsController : Controller
    {
        private DeviceDbContext db = new DeviceDbContext();

        // GET: DeviceEmails
        public ActionResult Index()
        {
            var deviceEmails = db.DeviceEmails.Include(d => d.Device);
            return View(deviceEmails.ToList());
        }

        // GET: DeviceEmails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeviceEmail deviceEmail = db.DeviceEmails.Find(id);
            if (deviceEmail == null)
            {
                return HttpNotFound();
            }
            return View(deviceEmail);
        }

        // GET: DeviceEmails/Create
        public ActionResult Create()
        {
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "HoSoCode");
            return View();
        }

        // POST: DeviceEmails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DeviceId,Email")] DeviceEmail deviceEmail)
        {
            if (ModelState.IsValid)
            {
                db.DeviceEmails.Add(deviceEmail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "HoSoCode", deviceEmail.DeviceId);
            return View(deviceEmail);
        }

        // GET: DeviceEmails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeviceEmail deviceEmail = db.DeviceEmails.Find(id);
            if (deviceEmail == null)
            {
                return HttpNotFound();
            }
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "HoSoCode", deviceEmail.DeviceId);
            return View(deviceEmail);
        }

        // POST: DeviceEmails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DeviceId,Email")] DeviceEmail deviceEmail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deviceEmail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "HoSoCode", deviceEmail.DeviceId);
            return View(deviceEmail);
        }

        // GET: DeviceEmails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeviceEmail deviceEmail = db.DeviceEmails.Find(id);
            if (deviceEmail == null)
            {
                return HttpNotFound();
            }
            return View(deviceEmail);
        }

        // POST: DeviceEmails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DeviceEmail deviceEmail = db.DeviceEmails.Find(id);
            db.DeviceEmails.Remove(deviceEmail);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Import()
        {
            return View("Import"); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength <= 0)
            {
                ViewBag.ImportMessage = "error|Vui lòng chọn file Excel (.xlsx)";
                return View("Create");
            }

            int successCount = 0;
            int skipCount = 0;
            string errorMsg = "";

            try
            {
                using (var package = new ExcelPackage(excelFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ViewBag.ImportMessage = "error|File Excel không hợp lệ";
                        return View("Create");
                    }

                    int rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount < 2)
                    {
                        ViewBag.ImportMessage = "error|File Excel không có dữ liệu";
                        return View("Create");
                    }

                    for (int row = 2; row <= rowCount; row++)
                    {
                        string hoSoCode = worksheet.Cells[row, 1].Text?.Trim();
                        string email = worksheet.Cells[row, 2].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(hoSoCode) || string.IsNullOrWhiteSpace(email))
                        {
                            skipCount++;
                            continue;
                        }

                        var device = db.Devices.FirstOrDefault(d => d.HoSoCode == hoSoCode);
                        if (device == null)
                        {
                            skipCount++;
                            errorMsg += $"Dòng {row}: Không tìm thấy mã HS '{hoSoCode}'\n";
                            continue;
                        }

                        bool exists = db.DeviceEmails.Any(e => e.DeviceId == device.Id && e.Email == email);
                        if (exists)
                        {
                            skipCount++;
                            errorMsg += $"Dòng {row}: Email '{email}' đã tồn tại cho thiết bị này\n";
                            continue;
                        }

                        db.DeviceEmails.Add(new DeviceEmail
                        {
                            DeviceId = device.Id,
                            Email = email
                        });

                        successCount++;
                    }

                    db.SaveChanges();
                }

                string message = $"Đã thêm thành công {successCount} email.";
                if (skipCount > 0)
                {
                    message += $"\nBỏ qua {skipCount} dòng (không tồn tại hoặc trùng lặp).";
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    message += $"\nChi tiết lỗi:\n{errorMsg}";
                }

                ViewBag.ImportMessage = "success|" + message;
            }
            catch (Exception ex)
            {
                ViewBag.ImportMessage = "error|Lỗi khi import: " + ex.Message;
            }

            // Trả về view Create để hiển thị toast ngay
            ViewBag.DeviceId = new SelectList(db.Devices, "Id", "HoSoCode");
            return View("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
