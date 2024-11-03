using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class HomeController : Controller
    {
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult Index(int page = 1, int pageSize = 12)
        {
            // Tổng số sản phẩm
            int totalProducts = data.SANPHAMs.Count();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Lấy sản phẩm cho trang hiện tại
            var products = data.SANPHAMs
                              .OrderBy(p => p.MASANPHAM)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }


        [HttpGet]
        public ActionResult Login()
        {
            return View("LogIn_SignUp");
        }

        ////[HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Kiểm tra thông tin đăng nhập
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.TAIKHOAN == username && u.MATKHAU == password);

            if (user != null)
            {
                // Lưu thông tin người dùng vào session
                Session["User"] = user;
                Session["UserName"] = user.TENNGUOIDUNG;

                // Lấy quyền người dùng
                var userRole = (from quyenNguoiDung in data.QUYEN_NGUOIDUNGs
                                join quyen in data.QUYENs on quyenNguoiDung.MAQUYEN equals quyen.MAQUYEN
                                where quyenNguoiDung.MANGUOIDUNG == user.MANGUOIDUNG
                                select quyen.TENQUYEN).FirstOrDefault();

                if (userRole != null)
                {
                    // Lưu vai trò vào session
                    Session["UserRole"] = userRole;

                    // Thiết lập cookie đăng nhập với FormsAuthentication
                    FormsAuthentication.SetAuthCookie(user.TAIKHOAN, false); // `false` nghĩa là cookie không vĩnh viễn

                    // Điều hướng dựa trên vai trò
                    switch (userRole)
                    {
                        case "Admin":
                            return RedirectToAction("IndexAdmin", "Admin");
                        case "NhanVien":
                            return RedirectToAction("IndexNhanVien", "NhanVien");
                        case "KhachHang":
                            return RedirectToAction("IndexKhachHang", "KhachHang");
                        default:
                            ViewBag.Message = "Không có quyền truy cập hợp lệ.";
                            return View();
                    }
                }
            }

            // Nếu thông tin đăng nhập không chính xác
            ModelState.AddModelError("Password", "Tên đăng nhập hoặc mật khẩu không chính xác");
            return View("LogIn_SignUp");
        }


        [HttpGet]
        public ActionResult SignUp()
        {
            return View("LogIn_SignUp");
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
        public ActionResult Search(string searchString, int page = 1, int pageSize = 12)
        {
            var sp = data.SANPHAMs.Where(s => s.TENSANPHAM.Contains(searchString));

            int totalProducts = sp.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            sp = sp.OrderBy(s => s.MASANPHAM)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

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
        public ActionResult ThemMatHangAjax(string MASANPHAM, string TENSANPHAM = null, string HINHANH = null, double? GIABAN = null, int SoLuong = 1)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }

            // Kiểm tra nếu các tham số bổ sung không được truyền (khi gọi từ Index.cshtml)
            if (string.IsNullOrEmpty(TENSANPHAM) || string.IsNullOrEmpty(HINHANH) || !GIABAN.HasValue)
            {
                // Lấy dữ liệu sản phẩm từ cơ sở dữ liệu dựa trên MASANPHAM
                var sp = data.SANPHAMs.SingleOrDefault(p => p.MASANPHAM == MASANPHAM);
                if (sp != null)
                {
                    TENSANPHAM = sp.TENSANPHAM;
                    HINHANH = sp.HINHANH;
                    GIABAN = (double)sp.GIABAN;
                }
                else
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }
            }

            // Thêm sản phẩm vào giỏ hàng với thông tin đầy đủ
            gh.Them(MASANPHAM, SoLuong);
            Session["GioHang"] = gh;

            return Json(new
            {
                success = true,
                cartCount = gh.SoMatHang() // Trả về số lượng sản phẩm trong giỏ hàng
            }, JsonRequestBehavior.AllowGet);
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