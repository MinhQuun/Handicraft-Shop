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

        
        [HttpPost]
        public ActionResult CreateUser(NGUOIDUNG newUser, string SelectedRole)
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

            if (user != null && ModelState.IsValid)
            {
                user.TENNGUOIDUNG = updatedUser.TENNGUOIDUNG;
                user.TAIKHOAN = updatedUser.TAIKHOAN;
                user.MATKHAU = updatedUser.MATKHAU;
                user.EMAIL = updatedUser.EMAIL;
                user.SODIENTHOAI = updatedUser.SODIENTHOAI;

                var userRole = data.QUYEN_NGUOIDUNGs.FirstOrDefault(q => q.MANGUOIDUNG == updatedUser.MANGUOIDUNG);

                if (userRole != null)
                {
                    if (userRole.MAQUYEN != SelectedRole)
                    {
                        data.QUYEN_NGUOIDUNGs.DeleteOnSubmit(userRole);

                        QUYEN_NGUOIDUNG newRole = new QUYEN_NGUOIDUNG
                        {
                            MANGUOIDUNG = updatedUser.MANGUOIDUNG,
                            MAQUYEN = SelectedRole
                        };
                        data.QUYEN_NGUOIDUNGs.InsertOnSubmit(newRole);
                    }
                }
                else
                {
                    QUYEN_NGUOIDUNG newRole = new QUYEN_NGUOIDUNG
                    {
                        MANGUOIDUNG = updatedUser.MANGUOIDUNG,
                        MAQUYEN = SelectedRole
                    };
                    data.QUYEN_NGUOIDUNGs.InsertOnSubmit(newRole);
                }

                data.SubmitChanges();

                return RedirectToAction("ManageUsers");
            }

            ViewBag.SelectedRole = SelectedRole; 
            return View(updatedUser);
        }



        // Xóa người dùng
        public ActionResult DeleteUser(string id)
        {
            
            var user = data.NGUOIDUNGs.FirstOrDefault(u => u.MANGUOIDUNG == id);

            if (user != null)
            {
               
                var userRoles = data.QUYEN_NGUOIDUNGs.Where(q => q.MANGUOIDUNG == id).ToList();
                data.QUYEN_NGUOIDUNGs.DeleteAllOnSubmit(userRoles);

                
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