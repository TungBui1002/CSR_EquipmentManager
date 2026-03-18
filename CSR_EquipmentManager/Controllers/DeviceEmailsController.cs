using CSR_EquipmentManager.Data;
using CSR_EquipmentManager.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
