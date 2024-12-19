using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using System.Text.RegularExpressions;

namespace Handicraft_Shop.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexAdmin(int page = 1, int pageSize = 12)
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
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = Session["UserRole"] as string;
            if (role != "Admin")
            {
                filterContext.Result = RedirectToAction("Login", "Home");
            }
            base.OnActionExecuting(filterContext);
        }
        // Hiển thị danh sách người dùng
        public ActionResult ManageUsers()
        {
            var users = data.NGUOIDUNGs.ToList();
            return View(users);
        }

        // [GET] Tạo người dùng mới
        [HttpGet]
        public ActionResult CreateUser()
        {
            return View();
        }


        
        [HttpPost]
        public ActionResult CreateUser(NGUOIDUNG newUser, string SelectedRole)
        {
            if (string.IsNullOrWhiteSpace(newUser.MATKHAU))
            {
                ModelState.AddModelError("MATKHAU", "Mật khẩu không được để trống.");
                return View(newUser);
            }
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }

            // Kiểm tra trùng tên tài khoản
            if (data.NGUOIDUNGs.Any(u => u.TAIKHOAN == newUser.TAIKHOAN))
            {
                ModelState.AddModelError("TAIKHOAN", "Tên tài khoản đã tồn tại. Vui lòng chọn tài khoản khác.");
                return View(newUser);
            }

            // Kiểm tra trùng email
            if (data.NGUOIDUNGs.Any(u => u.EMAIL == newUser.EMAIL))
            {
                ModelState.AddModelError("EMAIL", "Email đã được sử dụng. Vui lòng chọn email khác.");
                return View(newUser);
            }

            // Kiểm tra trùng số điện thoại
            if (data.NGUOIDUNGs.Any(u => u.SODIENTHOAI == newUser.SODIENTHOAI))
            {
                ModelState.AddModelError("SODIENTHOAI", "Số điện thoại đã được sử dụng. Vui lòng chọn số khác.");
                return View(newUser);
            }

            // Kiểm tra định dạng email
            if (!Regex.IsMatch(newUser.EMAIL ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("EMAIL", "Email không đúng định dạng.");
                return View(newUser);
            }

            // Kiểm tra định dạng số điện thoại
            if (!Regex.IsMatch(newUser.SODIENTHOAI ?? "", @"^\d{10}$"))
            {
                ModelState.AddModelError("SODIENTHOAI", "Số điện thoại phải là 10 chữ số.");
                return View(newUser);
            }

            if (ModelState.IsValid)
            {
                // Xử lý kiểm tra tên tài khoản, email, và số điện thoại như trước đó
                //...

                // Sinh mã người dùng mới
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

                // Thêm người dùng vào cơ sở dữ liệu
                data.NGUOIDUNGs.InsertOnSubmit(newUser);
                data.SubmitChanges();

                // Gán quyền cho người dùng
                QUYEN_NGUOIDUNG userRole = new QUYEN_NGUOIDUNG
                {
                    MANGUOIDUNG = newUser.MANGUOIDUNG,
                    MAQUYEN = SelectedRole
                };
                data.QUYEN_NGUOIDUNGs.InsertOnSubmit(userRole);
                data.SubmitChanges();

                return RedirectToAction("ManageUsers");
            }

            return View(newUser);
        }


        // [GET] Sửa thông tin người dùng
        [HttpGet]
        public ActionResult EditUser(string id)
        {
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // [POST] Cập nhật thông tin người dùng
        [HttpPost]
        public ActionResult EditUser(NGUOIDUNG updatedUser, string SelectedRole)
        {
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == updatedUser.MANGUOIDUNG);

            if (user != null)
            {
                // Kiểm tra các trường không được để trống
                if (string.IsNullOrWhiteSpace(updatedUser.TENNGUOIDUNG))
                {
                    ModelState.AddModelError("TENNGUOIDUNG", "Tên người dùng không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(updatedUser.TAIKHOAN))
                {
                    ModelState.AddModelError("TAIKHOAN", "Tài khoản không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(updatedUser.MATKHAU))
                {
                    ModelState.AddModelError("MATKHAU", "Mật khẩu không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(updatedUser.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Email không được để trống.");
                }

                if (string.IsNullOrWhiteSpace(updatedUser.SODIENTHOAI))
                {
                    ModelState.AddModelError("SODIENTHOAI", "Số điện thoại không được để trống.");
                }

                // Kiểm tra định dạng email
                if (!Regex.IsMatch(updatedUser.EMAIL ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    ModelState.AddModelError("EMAIL", "Email không đúng định dạng.");
                }

                // Kiểm tra định dạng số điện thoại
                if (!Regex.IsMatch(updatedUser.SODIENTHOAI ?? "", @"^\d{10}$"))
                {
                    ModelState.AddModelError("SODIENTHOAI", "Số điện thoại phải là 10 chữ số.");
                }

                // Kiểm tra tài khoản trùng
                var existingUsername = data.NGUOIDUNGs.FirstOrDefault(u => u.TAIKHOAN == updatedUser.TAIKHOAN && u.MANGUOIDUNG != updatedUser.MANGUOIDUNG);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("TAIKHOAN", "Tài khoản đã tồn tại. Vui lòng chọn tài khoản khác.");
                }

                // Kiểm tra email trùng
                var existingEmail = data.NGUOIDUNGs.FirstOrDefault(u => u.EMAIL == updatedUser.EMAIL && u.MANGUOIDUNG != updatedUser.MANGUOIDUNG);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("EMAIL", "Email đã tồn tại. Vui lòng chọn email khác.");
                }

                // Kiểm tra số điện thoại trùng
                var existingPhone = data.NGUOIDUNGs.FirstOrDefault(u => u.SODIENTHOAI == updatedUser.SODIENTHOAI && u.MANGUOIDUNG != updatedUser.MANGUOIDUNG);
                if (existingPhone != null)
                {
                    ModelState.AddModelError("SODIENTHOAI", "Số điện thoại đã tồn tại. Vui lòng chọn số khác.");
                }

                // Nếu có lỗi, trả về View với thông báo lỗi
                if (!ModelState.IsValid)
                {
                    return View(updatedUser);
                }

                // Cập nhật thông tin người dùng
                user.TENNGUOIDUNG = updatedUser.TENNGUOIDUNG;
                user.TAIKHOAN = updatedUser.TAIKHOAN;
                user.MATKHAU = updatedUser.MATKHAU;
                user.EMAIL = updatedUser.EMAIL;
                user.SODIENTHOAI = updatedUser.SODIENTHOAI;

                data.SubmitChanges();

                return RedirectToAction("ManageUsers");
            }

            return HttpNotFound();
        }


        // Xóa người dùng (trừ Admin)
        public ActionResult DeleteUser(string id)
        {
            try
            {
                // Kiểm tra người dùng có tồn tại không
                var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("ManageUsers");
                }

                // Kiểm tra quyền người dùng (không xóa Admin)
                var userRole = data.QUYEN_NGUOIDUNGs
                    .FirstOrDefault(q => q.MANGUOIDUNG == id && q.MAQUYEN == "Q01");

                if (userRole != null)
                {
                    TempData["ErrorMessage"] = "Không thể xóa người dùng có quyền Admin.";
                    return RedirectToAction("ManageUsers");
                }

                // Tìm khách hàng liên quan đến người dùng
                var customer = data.KHACHHANGs.FirstOrDefault(kh => kh.MANGUOIDUNG == id);

                if (customer != null)
                {
                    // Xóa chi tiết đơn hàng
                    var orderDetails = data.CHITIETDONHANGs
                        .Where(ctdh => data.DONHANGs
                                        .Where(dh => dh.MAKHACHHANG == customer.MAKHACHHANG)
                                        .Select(dh => dh.MADONHANG)
                                        .Contains(ctdh.MADONHANG))
                        .ToList();
                    if (orderDetails.Any())
                    {
                        data.CHITIETDONHANGs.DeleteAllOnSubmit(orderDetails);
                    }

                    // Xóa đơn hàng
                    var orders = data.DONHANGs.Where(dh => dh.MAKHACHHANG == customer.MAKHACHHANG).ToList();
                    if (orders.Any())
                    {
                        data.DONHANGs.DeleteAllOnSubmit(orders);
                    }

                    // Xóa địa chỉ giao hàng
                    var deliveryAddresses = data.DIACHI_GIAOHANGs.Where(d => d.MAKHACHHANG == customer.MAKHACHHANG).ToList();
                    if (deliveryAddresses.Any())
                    {
                        data.DIACHI_GIAOHANGs.DeleteAllOnSubmit(deliveryAddresses);
                    }

                    // Xóa khách hàng
                    data.KHACHHANGs.DeleteOnSubmit(customer);
                }

                // Xóa quyền người dùng
                var userRoles = data.QUYEN_NGUOIDUNGs.Where(q => q.MANGUOIDUNG == id).ToList();
                if (userRoles.Any())
                {
                    data.QUYEN_NGUOIDUNGs.DeleteAllOnSubmit(userRoles);
                }

                // Xóa người dùng
                data.NGUOIDUNGs.DeleteOnSubmit(user);

                // Lưu thay đổi vào cơ sở dữ liệu
                data.SubmitChanges();

                TempData["SuccessMessage"] = "Tài khoản người dùng đã được xóa thành công.";
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và thông báo lỗi
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa tài khoản: " + ex.Message;
            }

            // Quay lại trang quản lý người dùng
            return RedirectToAction("ManageUsers");
        }




        public ActionResult AdminDetails(string id)
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
        public ActionResult AdminMenuCap1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();

            return PartialView(dmsp);
        }
        public ActionResult AdminMenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        public ActionResult AdminLocDL_Theoloai(string mdm)
        {
            List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            return View("IndexAdmin", ds);
        }
        public ActionResult AdminSearch(string searchString, int page = 1, int pageSize = 12)
        {
            var sp = data.SANPHAMs.Where(s => s.TENSANPHAM.Contains(searchString));

            int totalProducts = sp.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            sp = sp.OrderBy(s => s.MASANPHAM)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString; // Truyền từ khóa vào ViewBag để hiển thị lại

            return View("AdminSearch", sp.ToList());
        }


    }
}