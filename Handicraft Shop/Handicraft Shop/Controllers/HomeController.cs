using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class HomeController : Controller
    {
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Tìm người dùng có tài khoản và mật khẩu trùng khớp
            var user = data.NGUOIDUNGs.FirstOrDefault(u =>
                            u.TAIKHOAN == username && u.MATKHAU == password);

            if (user != null)
            {
                // Lấy quyền của người dùng từ bảng QUYEN_NGUOIDUNG
                var userRole = (from quyenNguoiDung in data.QUYEN_NGUOIDUNGs
                                join quyen in data.QUYENs on quyenNguoiDung.MAQUYEN equals quyen.MAQUYEN
                                where quyenNguoiDung.MANGUOIDUNG == user.MANGUOIDUNG
                                select quyen.TENQUYEN).FirstOrDefault();

                if (userRole != null)
                {
                    // Lưu thông tin đăng nhập vào Session
                    Session["UserName"] = user.TENNGUOIDUNG;
                    Session["UserRole"] = userRole;

                    // Điều hướng người dùng đến Controller tương ứng theo quyền
                    if (userRole == "Admin")
                        return RedirectToAction("Index", "Admin");
                    else if (userRole == "NhanVien")
                        return RedirectToAction("Index", "NhanVien");
                    else if (userRole == "KhachHang")
                        return RedirectToAction("Index", "KhachHang");
                }
            }

            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }
        public ActionResult Logout()
        {
            // Xóa Session và điều hướng về trang đăng nhập
            Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}