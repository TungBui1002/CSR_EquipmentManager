using System;
using System.Web;
using System.Web.Mvc;

namespace CSR_EquipmentManager.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public ActionResult Login()
        {
            // Nếu đã đăng nhập thì chuyển thẳng vào Dashboard
            if (Session["IsLoggedIn"] != null && (bool)Session["IsLoggedIn"] == true)
            {
                return RedirectToAction("Index", "DashBoard");
            }

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // Kiểm tra cứng theo yêu cầu của anh
            if (username?.Trim().ToLower() == "admin" && password == "123456")
            {
                Session["IsLoggedIn"] = true;
                Session["Username"] = "admin";
                Session["FullName"] = "Administrator";

                return RedirectToAction("Index", "DashBoard");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // POST: /Account/ChangeLanguage
        public ActionResult ChangeLanguage(string lang)
        {
            Session["lang"] = lang;

            HttpCookie cookie = new HttpCookie("lang");
            cookie.Value = lang;
            cookie.Expires = DateTime.Now.AddDays(30);

            Response.Cookies.Add(cookie);

            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}