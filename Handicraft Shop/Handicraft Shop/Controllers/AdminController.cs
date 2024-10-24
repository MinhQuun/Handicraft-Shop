using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexAdmin()
        {
            List<SANPHAM> sp = data.SANPHAMs.ToList();
            return View(sp);
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

        // [POST] Thêm người dùng mới
        [HttpPost]
        public ActionResult CreateUser(NGUOIDUNG newUser)
        {
            if (ModelState.IsValid)
            {
                data.NGUOIDUNGs.InsertOnSubmit(newUser);
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
        public ActionResult EditUser(NGUOIDUNG updatedUser)
        {
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == updatedUser.MANGUOIDUNG);
            if (user != null && ModelState.IsValid)
            {
                user.TENNGUOIDUNG = updatedUser.TENNGUOIDUNG;
                user.TAIKHOAN = updatedUser.TAIKHOAN;
                user.MATKHAU = updatedUser.MATKHAU;
                user.EMAIL = updatedUser.EMAIL;
                user.SODIENTHOAI = updatedUser.SODIENTHOAI;
                data.SubmitChanges();
                return RedirectToAction("ManageUsers");
            }
            return View(updatedUser);
        }

        // Xóa người dùng
        public ActionResult DeleteUser(string id)
        {
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == id);
            if (user != null)
            {
                data.NGUOIDUNGs.DeleteOnSubmit(user);
                data.SubmitChanges();
            }
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
        public ActionResult AdminSearch(string searchString, int[] categoryIds)
        {
            var sp = from a in data.SANPHAMs select a;
            if (!string.IsNullOrEmpty(searchString))
            {
                sp = sp.Where(s => s.TENSANPHAM.Contains(searchString));
            }
            return View("AdminSearch", sp.ToList());
        }
    }
}