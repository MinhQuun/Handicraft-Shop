using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class NhanVienController : Controller
    {
        // GET: NhanVien
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexNhanVien()
        {
            List<SANPHAM> sp = data.SANPHAMs.ToList();
            return View(sp);
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = Session["UserRole"] as string;
            if (role != "NhanVien")
            {
                filterContext.Result = RedirectToAction("Login", "Home");
            }
            base.OnActionExecuting(filterContext);
        }
        public ActionResult NhanVienDetails(string id)
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
        public ActionResult NhanVienMenuCap1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();

            return PartialView(dmsp);
        }
        public ActionResult NhanVienMenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        public ActionResult NhanVienLocDL_Theoloai(string mdm)
        {
            List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            return View("IndexNhanVien", ds);
        }
        public ActionResult NhanVienSearch(string searchString, int[] categoryIds)
        {
            var sp = from a in data.SANPHAMs select a;
            if (!string.IsNullOrEmpty(searchString))
            {
                sp = sp.Where(s => s.TENSANPHAM.Contains(searchString));
            }
            return View("NhanVienSearch", sp.ToList());
        }
    }
}