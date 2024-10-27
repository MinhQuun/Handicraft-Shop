using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class KhachHangController : Controller
    {
        // GET: KhachHang
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexKhachHang()
        {
            List<SANPHAM> sp = data.SANPHAMs.ToList();
            return View(sp);
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
            if (Session["UserRole"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Home");
                return;
            }

            var role = Session["UserRole"].ToString();

            // Kiểm tra nếu người dùng không có quyền "KhachHang"
            if (role != "KhachHang")
            {
                filterContext.Result = RedirectToAction("Unauthorized", "Home");
                return;
            }

            base.OnActionExecuting(filterContext); // Tiếp tục thực hiện nếu hợp lệ
        }
        public ActionResult KhachHangDetails(string id)
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
        public ActionResult KhachHangMenuCap1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();
            return PartialView(dmsp);
        }
        public ActionResult KhachHangMenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        public ActionResult KhachHangLocDL_Theoloai(string mdm)
        {
            List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            return View("IndexKhachHang", ds);
        }
        public ActionResult KhachHangSearch(string searchString, int[] categoryIds)
        {
            var sp = from a in data.SANPHAMs select a;
            if (!string.IsNullOrEmpty(searchString))
            {
                sp = sp.Where(s => s.TENSANPHAM.Contains(searchString));
            }
            return View("KhachHangSearch", sp.ToList());
        }
        
    }
}