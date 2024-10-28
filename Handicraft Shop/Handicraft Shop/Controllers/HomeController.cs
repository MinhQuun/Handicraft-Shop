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
            // Kiểm tra thông tin đăng nhập từ CSDL
            var user = data.NGUOIDUNGs.FirstOrDefault(u =>
                            u.TAIKHOAN == username && u.MATKHAU == password);

            if (user != null)
            {
                // Gán người dùng vào session
                Session["kh"] = user;

                // Lấy quyền của người dùng
                var userRole = (from quyenNguoiDung in data.QUYEN_NGUOIDUNGs
                                join quyen in data.QUYENs on quyenNguoiDung.MAQUYEN equals quyen.MAQUYEN
                                where quyenNguoiDung.MANGUOIDUNG == user.MANGUOIDUNG
                                select quyen.TENQUYEN).FirstOrDefault();

                if (userRole != null)
                {
                    // Gán quyền và tên người dùng vào session
                    Session["UserName"] = user.TENNGUOIDUNG;
                    Session["UserRole"] = userRole;

                    // Điều hướng dựa trên quyền
                    switch (userRole)
                    {
                        case "Admin":
                            return RedirectToAction("IndexAdmin", "Admin");
                        case "NhanVien":
                            return RedirectToAction("Index", "NhanVien");
                        case "KhachHang":
                            return RedirectToAction("IndexKhachHang", "KhachHang");
                        default:
                            ViewBag.Message = "Không có quyền truy cập hợp lệ.";
                            return View();
                    }
                }
            }

            // Thông báo nếu thông tin đăng nhập không đúng
            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(NGUOIDUNG newUser)
        {
            if (ModelState.IsValid)
            {
                var lastUser = data.NGUOIDUNGs
                                  .OrderByDescending(u => u.MANGUOIDUNG)
                                  .FirstOrDefault();

                int nextId = 1;
                if (lastUser != null)
                {
                    string lastIdString = lastUser.MANGUOIDUNG.Substring(2); 
                    nextId = int.Parse(lastIdString) + 1; 
                }

                newUser.MANGUOIDUNG = "ND" + nextId.ToString("D2");

                data.NGUOIDUNGs.InsertOnSubmit(newUser);
                data.SubmitChanges();

                QUYEN_NGUOIDUNG userRole = new QUYEN_NGUOIDUNG
                {
                    MANGUOIDUNG = newUser.MANGUOIDUNG,
                    MAQUYEN = "Q03" 
                };
                data.QUYEN_NGUOIDUNGs.InsertOnSubmit(userRole);
                data.SubmitChanges();

                return RedirectToAction("Success");
            }

            return View(newUser);
        }
        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Logout()
        {
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

        //Thêm mặt hàng
        public ActionResult ThemMatHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            gh.Them(id);
            Session["GioHang"] = gh;
            return RedirectToAction("Index");
        }
        //Them gio hang co SL
        [HttpPost]
        public ActionResult ThemMatHang(FormCollection c)
        {
            string ma = c["txtMa"];
            int sl = int.Parse(c["SoLuong"]);

            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }

            gh.Them(ma, sl);
            Session["GioHang"] = gh;
            return RedirectToAction("Index");
        }

        //Xem giỏ hàng
        public ActionResult XemGioHang()
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            return View(gh);
        }

        //Tăng mặt hàng trong giỏ hàng
        public ActionResult TangMatHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            gh.Them(id);
            Session["GioHang"] = gh;
            return RedirectToAction("Xemgiohang");
        }

        //Giảm mặt hàng trong giỏ hàng
        public ActionResult GiamMatHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            int result = gh.Giam(id);
            Session["GioHang"] = gh;
            if (result == -1)
            {
                ViewBag.ErrorMessage = "Mặt hàng không tồn tại trong giỏ hàng.";
            }
            else if (result == -2)
            {
                ViewBag.ErrorMessage = "Số lượng mặt hàng đã là 0 và không thể giảm thêm.";
            }
            return RedirectToAction("Xemgiohang");
        }
        //Xóa hàng trong giỏ hàng
        public ActionResult XoaGioHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            gh.XoaSanPham(id);
            return RedirectToAction("Xemgiohang");
        }
        [HttpPost]
        public ActionResult CapNhatGioHang(string maSanPham, int soLuong)
        {

            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                return Json(new { success = false, message = "Giỏ hàng rỗng" });
            }


            var gioHangItem = gh.list.FirstOrDefault(item => item.MaSP == maSanPham);
            if (gioHangItem != null)
            {

                gioHangItem.SoLuong = soLuong;


                Session["GioHang"] = gh;
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
        }
        public ActionResult Unauthorized()
        {
            return View();
        }

    }
}