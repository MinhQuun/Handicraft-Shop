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
            List<SANPHAM> sp = data.SANPHAMs.ToList();
            return View(sp);
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
                        return RedirectToAction("IndexAdmin", "Admin");
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
        public ActionResult Details(string id)
        {
            // Tìm sản phẩm chi tiết bằng cách sử dụng LINQ to SQL
            var sanPham = data.SANPHAMs.SingleOrDefault(sp => sp.MASANPHAM == id);

            if (sanPham == null)
            {
                return HttpNotFound(); // Xử lý khi sản phẩm không tồn tại
            }

            // Truy vấn các sản phẩm liên quan dựa trên LOAISANPHAM
            var relatedProducts = data.SANPHAMs
                                    .Where(sp => sp.MALOAI == sanPham.MALOAI && sp.MASANPHAM != id)
                                    .ToList();

            // Truyền dữ liệu sang View
            ViewBag.RelatedProducts = relatedProducts;
            return View(sanPham);
        }
        public ActionResult Menuc1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();

            return PartialView(dmsp);
        }
        public ActionResult MenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        public ActionResult LocDL_Theoloai(string mdm)
        {
            List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            return View("Index", ds);
        }
        public ActionResult Search(string searchString, int[] categoryIds)
        {
            var sp = from a in data.SANPHAMs select a;
            if (!string.IsNullOrEmpty(searchString))
            {
                sp = sp.Where(s => s.TENSANPHAM.Contains(searchString));
            }
            return View("Search", sp.ToList());
        }
    }
}