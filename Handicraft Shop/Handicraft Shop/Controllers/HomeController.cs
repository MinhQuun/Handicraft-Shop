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
            var user = data.NGUOIDUNGs.FirstOrDefault(u =>
                            u.TAIKHOAN == username && u.MATKHAU == password);

            if (user != null)
            {
                Session["kh"] = user;
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
                        return RedirectToAction("Index");
                }
            }

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

        public List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        public ActionResult XemGioHang()
        {
            var cart = GetCart();
            return View(cart);
        }

        public ActionResult ThemVaoGio(string id, int quantity = 1)
        {
            var product = data.SANPHAMs.SingleOrDefault(p => p.MASANPHAM == id);
            if (product != null)
            {
                var cart = GetCart();
                var existingItem = cart.FirstOrDefault(c => c.Product.MASANPHAM == id);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity; // Cộng dồn số lượng nếu sản phẩm đã có trong giỏ
                }
                else
                {
                    cart.Add(new CartItem { Product = product, Quantity = quantity });
                }

                Session["Cart"] = cart; // Cập nhật lại Session giỏ hàng
            }

            return RedirectToAction("XemGioHang"); // Điều hướng về giỏ hàng
        }

        public ActionResult XoaKhoiGio(string id)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(c => c.Product.MASANPHAM == id);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                Session["Cart"] = cart;
            }
            return RedirectToAction("XemGioHang");
        }

        [HttpPost]
        public ActionResult CapNhatGio(string id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.MASANPHAM == id);

            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                Session["Cart"] = cart;
            }
            else if (quantity <= 0)
            {
                cart.Remove(item);
                Session["Cart"] = cart;
            }
            return RedirectToAction("XemGioHang");
        }
    }
}