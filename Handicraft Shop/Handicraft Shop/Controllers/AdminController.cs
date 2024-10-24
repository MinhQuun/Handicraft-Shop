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
        public ActionResult Index()
        {
            return View();
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
    }
}