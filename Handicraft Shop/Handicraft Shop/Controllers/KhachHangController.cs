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
            var role = Session["UserRole"] as string;
            if (role != "KhachHang")
            {
                filterContext.Result = RedirectToAction("Login", "Home");
            }
            base.OnActionExecuting(filterContext);
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
        //Thêm mặt hàng
        public ActionResult KhachHangThemMatHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            gh.Them(id);
            Session["GioHang"] = gh;
            return RedirectToAction("IndexKhachHang");
        }
        //Them gio hang co SL
        [HttpPost]
        public ActionResult KhachHangThemMatHangAjax(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            gh.Them(id);
            Session["GioHang"] = gh;

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    cartCount = gh.SoMatHang() // Số lượng sản phẩm trong giỏ hàng
                }, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Index");
        }

        //Xem giỏ hàng
        public ActionResult KhachHangXemGioHang()
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            return View(gh);
        }

        //Tăng mặt hàng trong giỏ hàng
        public ActionResult KhachHangTangMatHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            if (gh == null)
            {
                gh = new GioHang();
            }
            gh.Them(id);
            Session["GioHang"] = gh;
            return RedirectToAction("KhachHangXemgiohang");
        }

        //Giảm mặt hàng trong giỏ hàng
        public ActionResult KhachHangGiamMatHang(string id)
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
            return RedirectToAction("KhachHangXemgiohang");
        }
        //Xóa hàng trong giỏ hàng
        public ActionResult KhachHangXoaGioHang(string id)
        {
            GioHang gh = Session["GioHang"] as GioHang;
            gh.XoaSanPham(id);
            return RedirectToAction("KhachHangXemgiohang");
        }
        [HttpPost]
        public ActionResult KhachHangCapNhatGioHang(string maSanPham, int soLuong)
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
        public ActionResult KhachHangDatHang()
        {
            //kiem tra khach hang da dang nhap
            //chua
            if (Session["kh"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            GioHang gh = Session["GioHang"] as GioHang;

            NGUOIDUNG khach = Session["kh"] as NGUOIDUNG;
            ViewBag.k = khach;
            return View(gh);
        }
        [HttpPost]
        public ActionResult KhachHangConfirmOrder(string diachigh, string SelectedRole, string ghiChu)
        {
            // Kiểm tra người dùng đã đăng nhập
            var nguoiDung = Session["kh"] as NGUOIDUNG;
            if (nguoiDung == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Lấy mã người dùng từ bảng NGUOIDUNG (dưới dạng chuỗi)
            string maNguoiDung = nguoiDung.MANGUOIDUNG;

            // Kiểm tra xem người dùng này đã có thông tin trong bảng KHACHHANG chưa
            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == maNguoiDung);
            if (khachHang == null)
            {
                // Nếu chưa có, tạo bản ghi mới trong KHACHHANG
                khachHang = new KHACHHANG
                {
                    MANGUOIDUNG = maNguoiDung, // Liên kết mã người dùng
                    HOTEN = nguoiDung.TENNGUOIDUNG,
                    SODIENTHOAI = nguoiDung.SODIENTHOAI,
                    EMAIL = nguoiDung.EMAIL
                };
                data.KHACHHANGs.InsertOnSubmit(khachHang);
                data.SubmitChanges();
            }

            // Lấy giỏ hàng từ session
            var gh = Session["GioHang"] as GioHang;
            if (gh == null || !gh.list.Any())
            {
                ViewBag.ErrorMessage = "Giỏ hàng trống.";
                return RedirectToAction("KhachHangXemGioHang", "KhachHang");
            }

            // Thêm địa chỉ vào bảng DIACHI_GIAOHANG
            var diaChiGiaoHang = new DIACHI_GIAOHANG
            {
                MAKHACHHANG = khachHang.MAKHACHHANG,
                DIACHI = diachigh
            };
            data.DIACHI_GIAOHANGs.InsertOnSubmit(diaChiGiaoHang);
            data.SubmitChanges();

            // Thêm đơn hàng vào bảng DONHANG
            var donHang = new DONHANG
            {
                MAKHACHHANG = khachHang.MAKHACHHANG,
                MADIACHI = diaChiGiaoHang.MADIACHI,
                NGAYDAT = DateTime.Now,
                NGAYGIAO = null,
                MATT = SelectedRole,
                GHICHU = ghiChu,
                TONGSLHANG = gh.TongSLHang(),
                TONGTHANHTIEN = (decimal)gh.TongThanhTien(),
                TRANGTHAI = "Chờ xử lý"
            };
            data.DONHANGs.InsertOnSubmit(donHang);
            data.SubmitChanges();

            // Thêm từng sản phẩm trong giỏ vào bảng CHITIETDONHANG
            foreach (var item in gh.list)
            {
                var chiTietDonHang = new CHITIETDONHANG
                {
                    MADONHANG = donHang.MADONHANG,
                    MASANPHAM = item.MaSP,
                    SOLUONG = item.SoLuong,
                    DONGIA = (decimal)item.DonGia
                };
                data.CHITIETDONHANGs.InsertOnSubmit(chiTietDonHang);
            }
            data.SubmitChanges();

            // Xóa giỏ hàng sau khi đơn hàng đã được xác nhận
            Session["GioHang"] = null;

            // Chuyển hướng đến trang xác nhận đơn hàng
            return RedirectToAction("KhachHangOrderConfirmation", new { id = donHang.MADONHANG });
        }


        public ActionResult KhachHangOrderConfirmation(  )
        {
            // Lấy thông tin đơn hàng theo ID
            return View();
        }
        public ActionResult KhachHangLichSuDonHang()
        {
            if (Session["UserRole"]?.ToString() != "KhachHang")
            {
                return RedirectToAction("Login", "Home");
            }

            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var donHangs = data.DONHANGs
                .Where(d => d.MAKHACHHANG == khachHang.MAKHACHHANG)
                .OrderByDescending(d => d.NGAYDAT)
                .ToList();

            return View(donHangs);
        }



    }
}