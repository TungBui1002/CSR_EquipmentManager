using CSR_EquipmentManager.Data;
using CSR_EquipmentManager.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CSR_EquipmentManager.Controllers
{
    public class DevicesController : Controller
    {
        private DeviceDbContext db = new DeviceDbContext();

        // GET: Devices
        public ActionResult Index()
        {
            return View(db.Devices.ToList());
        }

        // GET: Devices/ExpiringSoon
        public ActionResult ExpiringSoon()
        {
            var now = DateTime.Now.Date;

            var devices = db.Devices
                .Where(d => d.NextInspectionDate.HasValue &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) >= 0 &&   // >= today
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) <= 30)   // <= 30 ngày nữa
                .ToList();

            ViewBag.Title = "Thiết Bị Sắp Hết Hạn (≤ 30 ngày)";
            ViewBag.Subtitle = "Danh sách thiết bị cần kiểm định sớm";
            ViewBag.PageType = "expiring";

            return View("Index", devices);
        }

        // GET: Devices/Expired
        public ActionResult Expired()
        {
            var now = DateTime.Now.Date;

            var devices = db.Devices
                .Where(d => d.NextInspectionDate.HasValue &&
                            DbFunctions.DiffDays(d.NextInspectionDate.Value, now) > 0)   // quá hạn = Next < now
                .ToList();

            ViewBag.Title = "Thiết Bị Quá Hạn Kiểm Định";
            ViewBag.Subtitle = "Danh sách thiết bị đã quá hạn, cần xử lý ngay";
            ViewBag.PageType = "expired";

            return View("Index", devices);
        }

        // GET: Devices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        // GET: Devices/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Devices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Device device)
        {
            if (ModelState.IsValid)
            {
                db.Devices.Add(device);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(device);
        }

        // GET: Devices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        // POST: Devices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Device device)
        {
            if (ModelState.IsValid)
            {
                db.Entry(device).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(device);
        }

        // GET: Devices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Device device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        // POST: Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Device device = db.Devices.Find(id);
            db.Devices.Remove(device);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Devices/DownloadTemplate
        public ActionResult DownloadTemplate()
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Template");

                // Header (giống file của bạn)
                ws.Cells[1, 1].Value = "Mã hồ sơ";
                ws.Cells[1, 2].Value = "Xưởng";
                ws.Cells[1, 3].Value = "Tòa nhà";
                ws.Cells[1, 4].Value = "Tầng";
                ws.Cells[1, 5].Value = "STT";
                ws.Cells[1, 6].Value = "Mã vị trí";
                ws.Cells[1, 7].Value = "Mã thiết bị";
                ws.Cells[1, 8].Value = "Tên loại TB";
                ws.Cells[1, 9].Value = "Dung tích";
                ws.Cells[1, 10].Value = "Mã hiệu thiết bị";
                ws.Cells[1, 11].Value = "BB kiểm định";
                ws.Cells[1, 12].Value = "NT kiểm định";
                ws.Cells[1, 13].Value = "NT kiểm định tiếp theo";
                ws.Cells[1, 14].Value = "Năm chế tạo";
                ws.Cells[1, 15].Value = "Vị trí thiết bị";

                // Style header
                using (var range = ws.Cells[1, 1, 1, 15])
                {
                    range.Style.Font.Bold = true;
                }

                ws.Cells.AutoFitColumns();

                var stream = new System.IO.MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = "Template_ThietBi.xlsx";

                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }

        // GET: Devices/Import
        public ActionResult Import()
        {
            return View();
        }

        // POST: Devices/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength <= 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel (.xlsx)";
                return RedirectToAction("Index");
            }

            try
            {
                using (var package = new OfficeOpenXml.ExcelPackage(excelFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["Error"] = "File Excel không hợp lệ";
                        return RedirectToAction("Index");
                    }

                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var device = new Device
                        {
                            HoSoCode = worksheet.Cells[row, 1].Text?.Trim(),
                            Factory = worksheet.Cells[row, 2].Text?.Trim(),
                            Building = worksheet.Cells[row, 3].Text?.Trim(),
                            Floor = worksheet.Cells[row, 4].Text?.Trim(),
                            STT = worksheet.Cells[row, 5].Text?.Trim(),
                            PositionCode = worksheet.Cells[row, 6].Text?.Trim(),
                            DeviceCode = worksheet.Cells[row, 7].Text?.Trim(),
                            DeviceName = worksheet.Cells[row, 8].Text?.Trim(),
                            Capacity = worksheet.Cells[row, 9].Text?.Trim(),
                            ModelCode = worksheet.Cells[row, 10].Text?.Trim(),
                            InspectionReport = worksheet.Cells[row, 11].Text?.Trim(),
                            ManufactureYear = worksheet.Cells[row, 14].Text?.Trim(),
                            Location = worksheet.Cells[row, 15].Text?.Trim()
                        };

                        // ❗ BẮT BUỘC: Tên thiết bị
                        if (string.IsNullOrEmpty(device.DeviceName))
                        {
                            continue;
                        }

                        // Parse ngày
                        if (DateTime.TryParseExact(
                            worksheet.Cells[row, 12].Text?.Trim(),
                            "yyyy/MM/dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime insDate))
                        {
                            device.InspectionDate = insDate;
                        }

                        if (DateTime.TryParseExact(
                            worksheet.Cells[row, 13].Text?.Trim(),
                            "yyyy/MM/dd",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime nextDate))
                        {
                            device.NextInspectionDate = nextDate;
                        }

                        db.Devices.Add(device);
                    }

                    db.SaveChanges();
                }

                TempData["Success"] = "Import dữ liệu từ Excel thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi import: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // ====================== EXPORT EXCEL TOÀN BỘ DANH SÁCH ======================
        public ActionResult ExportToExcel()
        {
            var devices = db.Devices.ToList();

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("DanhSachThietBi");

                // Header
                ws.Cells[1, 1].Value = "Mã Hồ Sơ";
                ws.Cells[1, 2].Value = "Tên Thiết Bị";
                ws.Cells[1, 3].Value = "Xưởng";
                ws.Cells[1, 4].Value = "Tòa Nhà";
                ws.Cells[1, 5].Value = "Tầng";
                ws.Cells[1, 6].Value = "STT";
                ws.Cells[1, 7].Value = "Mã Vị Trí";
                ws.Cells[1, 8].Value = "Mã Thiết Bị";
                ws.Cells[1, 9].Value = "Model";
                ws.Cells[1, 10].Value = "Công Suất";
                ws.Cells[1, 11].Value = "Năm Chế Tạo";
                ws.Cells[1, 12].Value = "Vị Trí Chi Tiết";
                ws.Cells[1, 13].Value = "Biên Bản Kiểm Định";
                ws.Cells[1, 14].Value = "Ngày Kiểm Định";
                ws.Cells[1, 15].Value = "Ngày Kiểm Định Tiếp Theo";

                // Style header
                using (var range = ws.Cells[1, 1, 1, 15])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Đổ dữ liệu
                int row = 2;
                foreach (var d in devices)
                {
                    ws.Cells[row, 1].Value = d.HoSoCode;
                    ws.Cells[row, 2].Value = d.DeviceName;
                    ws.Cells[row, 3].Value = d.Factory;
                    ws.Cells[row, 4].Value = d.Building;
                    ws.Cells[row, 5].Value = d.Floor;
                    ws.Cells[row, 6].Value = d.STT;
                    ws.Cells[row, 7].Value = d.PositionCode;
                    ws.Cells[row, 8].Value = d.DeviceCode;
                    ws.Cells[row, 9].Value = d.ModelCode;
                    ws.Cells[row, 10].Value = d.Capacity;
                    ws.Cells[row, 11].Value = d.ManufactureYear;
                    ws.Cells[row, 12].Value = d.Location;
                    ws.Cells[row, 13].Value = d.InspectionReport;
                    ws.Cells[row, 14].Value = d.InspectionDate?.ToString("yyyy/MM/dd");
                    ws.Cells[row, 15].Value = d.NextInspectionDate?.ToString("yyyy/MM/dd");
                    row++;
                }

                ws.Cells.AutoFitColumns();

                var fileName = $"DanhSach_ThietBi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
        }

        // ====================== EXPORT EXCEL - THIẾT BỊ SẮP HẾT HẠN ======================
        public ActionResult ExportExpiringSoonToExcel()
        {
            var now = DateTime.Now.Date;

            var devices = db.Devices
                .Where(d => d.NextInspectionDate.HasValue &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) > 0 &&
                            DbFunctions.DiffDays(now, d.NextInspectionDate.Value) <= 30)
                .ToList();

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("ThietBi_SapHetHan");

                // Header
                ws.Cells[1, 1].Value = "Mã Hồ Sơ";
                ws.Cells[1, 2].Value = "Tên Thiết Bị";
                ws.Cells[1, 3].Value = "Xưởng";
                ws.Cells[1, 4].Value = "Tòa Nhà";
                ws.Cells[1, 5].Value = "Tầng";
                ws.Cells[1, 6].Value = "STT";
                ws.Cells[1, 7].Value = "Mã Vị Trí";
                ws.Cells[1, 8].Value = "Mã Thiết Bị";
                ws.Cells[1, 9].Value = "Model";
                ws.Cells[1, 10].Value = "Công Suất";
                ws.Cells[1, 11].Value = "Năm Chế Tạo";
                ws.Cells[1, 12].Value = "Vị Trí Chi Tiết";
                ws.Cells[1, 13].Value = "Biên Bản Kiểm Định";
                ws.Cells[1, 14].Value = "Ngày Kiểm Định";
                ws.Cells[1, 15].Value = "Ngày Kiểm Định Tiếp Theo";
                ws.Cells[1, 16].Value = "Còn lại (ngày)";

                // Style header
                using (var range = ws.Cells[1, 1, 1, 16])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }

                // Đổ dữ liệu
                int row = 2;
                foreach (var d in devices)
                {
                    int daysLeft = (d.NextInspectionDate.Value - now).Days;

                    ws.Cells[row, 1].Value = d.HoSoCode;
                    ws.Cells[row, 2].Value = d.DeviceName;
                    ws.Cells[row, 3].Value = d.Factory;
                    ws.Cells[row, 4].Value = d.Building;
                    ws.Cells[row, 5].Value = d.Floor;
                    ws.Cells[row, 6].Value = d.STT;
                    ws.Cells[row, 7].Value = d.PositionCode;
                    ws.Cells[row, 8].Value = d.DeviceCode;
                    ws.Cells[row, 9].Value = d.ModelCode;
                    ws.Cells[row, 10].Value = d.Capacity;
                    ws.Cells[row, 11].Value = d.ManufactureYear;
                    ws.Cells[row, 12].Value = d.Location;
                    ws.Cells[row, 13].Value = d.InspectionReport;
                    ws.Cells[row, 14].Value = d.InspectionDate?.ToString("yyyy/MM/dd");
                    ws.Cells[row, 15].Value = d.NextInspectionDate?.ToString("yyyy/MM/dd");
                    ws.Cells[row, 16].Value = daysLeft;

                    // Định dạng cột "Còn lại"
                    if (daysLeft <= 10)
                        ws.Cells[row, 16].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                    else if (daysLeft <= 20)
                        ws.Cells[row, 16].Style.Font.Color.SetColor(System.Drawing.Color.Orange);

                    row++;
                }

                ws.Cells.AutoFitColumns();

                var fileName = $"ThietBi_SapHetHan_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(package.GetAsByteArray(), contentType, fileName);
            }
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
