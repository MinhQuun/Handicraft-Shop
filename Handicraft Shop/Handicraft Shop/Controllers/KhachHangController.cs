using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using BCrypt.Net;

namespace Handicraft_Shop.Controllers
{
    public class KhachHangController : Controller
    {
        // GET: KhachHang
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexKhachHang(int page = 1, int pageSize = 12)
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

            // Gợi ý sản phẩm dựa trên lịch sử đơn hàng
            var recommendedProducts = GetRecommendedProducts(); // Gọi phương thức gợi ý sản phẩm
            ViewBag.RecommendedProducts = recommendedProducts; // Truyền sản phẩm gợi ý vào ViewBag

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
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
        public ActionResult KhachHangHuyDonHang(int id)
        {
            var donHang = data.DONHANGs.SingleOrDefault(dh => dh.MADONHANG == id);
            if (donHang != null && donHang.TRANGTHAI == "Đang xử lý")
            {
                donHang.TRANGTHAI = "Đã hủy";  // Cập nhật trạng thái đơn hàng thành "Đã hủy"
                data.SubmitChanges();
            }
            return RedirectToAction("KhachHangLichSuDonHang");  // Hoặc chuyển về trang đơn hàng đang xử lý
        }
        public ActionResult KhachHangChiTietDonHang(int id)
        {
            var donHang = data.DONHANGs.SingleOrDefault(dh => dh.MADONHANG == id);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            var chiTiet = data.CHITIETDONHANGs
                .Where(ct => ct.MADONHANG == donHang.MADONHANG)
                .Select(ct => new ChiTietSanPhamViewModel
                {
                    MaSanPham = ct.MASANPHAM,
                    TenSanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).TENSANPHAM,
                    HinhAnh = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).HINHANH,
                    SoLuong = ct.SOLUONG ?? 0,
                    DonGia = ct.DONGIA ?? 0
                }).ToList();

            var model = new DonHangViewModel
            {
                DonHang = donHang,
                ChiTietSanPhams = chiTiet
            };

            return View(model);  // Hiển thị chi tiết đơn hàng
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
        public ActionResult KhachHangSearch(string searchString, int page = 1, int pageSize = 12)
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
        public ActionResult KhachHangThemMatHangAjax(string MASANPHAM, string TENSANPHAM = null, string HINHANH = null, double? GIABAN = null, int SoLuong = 1)
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
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            GioHang gh = Session["GioHang"] as GioHang;

            NGUOIDUNG khach = Session["User"] as NGUOIDUNG;
            ViewBag.k = khach;
            return View(gh);
        }
        [HttpPost]
        public ActionResult KhachHangConfirmOrder(string diachigh, string SelectedRole, string ghiChu)
        {
            // Kiểm tra người dùng đã đăng nhập
            var nguoiDung = Session["User"] as NGUOIDUNG;
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
                TRANGTHAI = "Đang xử lý"
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


        public ActionResult KhachHangOrderConfirmation()
        {
            // Lấy thông tin đơn hàng theo ID
            return View();
        }
        [Authorize]
        public ActionResult KhachHangLichSuDonHang()
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào.";
                return View(new List<DonHangViewModel>());
            }

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào.";
                return View(new List<DonHangViewModel>());
            }

            var donHangs = data.DONHANGs
           .Where(d => d.MAKHACHHANG == khachHang.MAKHACHHANG &&
                    (d.TRANGTHAI == "Đang xử lý" || d.TRANGTHAI == "Đang giao" || d.TRANGTHAI == "Đã hủy"))
           .OrderByDescending(d => d.NGAYDAT)
           .Select(dh => new DonHangViewModel
           {
               DonHang = dh,
               ChiTietSanPhams = data.CHITIETDONHANGs
                   .Where(ct => ct.MADONHANG == dh.MADONHANG)
                   .Select(ct => new ChiTietSanPhamViewModel
                   {
                       MaSanPham = ct.MASANPHAM,
                       SoLuong = ct.SOLUONG.GetValueOrDefault(), 
                       DonGia = ct.DONGIA.GetValueOrDefault(), 
                       TenSanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).TENSANPHAM,
                       HinhAnh = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).HINHANH
                   }).ToList()
           }).ToList();



            if (donHangs.Count == 0)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào.";
            }

            return View(donHangs);
        }

        [Authorize]
        public ActionResult KhachHangLichSuMuaHang()
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào trong lịch sử mua hàng.";
                return View(new List<DonHangViewModel>());
            }

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào trong lịch sử mua hàng.";
                return View(new List<DonHangViewModel>());
            }

            // Lọc chỉ các đơn hàng đã giao
            var donHangs = data.DONHANGs
                .Where(d => d.MAKHACHHANG == khachHang.MAKHACHHANG && d.TRANGTHAI == "Đã giao")
                .OrderByDescending(d => d.NGAYDAT)
                .Select(dh => new DonHangViewModel
                {
                    DonHang = dh,
                    ChiTietSanPhams = data.CHITIETDONHANGs
                        .Where(ct => ct.MADONHANG == dh.MADONHANG)
                        .Select(ct => new ChiTietSanPhamViewModel
                        {
                            MaSanPham = ct.MASANPHAM,
                            SoLuong = ct.SOLUONG.GetValueOrDefault(),
                            DonGia = ct.DONGIA.GetValueOrDefault(),
                            TenSanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).TENSANPHAM,
                            HinhAnh = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).HINHANH
                        }).ToList()
                }).ToList();

            if (donHangs.Count == 0)
            {
                ViewBag.Message = "Bạn chưa có đơn hàng nào trong lịch sử mua hàng.";
            }

            return View(donHangs);
        }


        public ActionResult KhachHangProfile()
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var khachHang = data.KHACHHANGs.SingleOrDefault(kh => kh.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null)
            {
                return HttpNotFound();
            }

            return View(khachHang);
        }

        // Hiển thị form chỉnh sửa thông tin cá nhân
        public ActionResult KhachHangEditProfile()
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var model = new KhachHangEditProfile
            {
                Username = user.TAIKHOAN,
                Email = user.EMAIL,
                SoDienThoai = user.SODIENTHOAI,
                HoTen = user.KHACHHANGs.FirstOrDefault()?.HOTEN
            };

            return View(model);
        }

        
        [HttpPost]
        public ActionResult KhachHangUpdateProfile(KhachHangEditProfile model)
        {
            var userSession = Session["User"] as NGUOIDUNG;
            if (userSession == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var user = data.NGUOIDUNGs.SingleOrDefault(u => u.MANGUOIDUNG == userSession.MANGUOIDUNG);
            if (user == null)
            {
                return HttpNotFound("Không tìm thấy người dùng.");
            }

            if (!ModelState.IsValid)
            {
                return View("KhachHangEditProfile", model);
            }

            // Cập nhật thông tin cá nhân
            user.TENNGUOIDUNG = model.HoTen;
            user.EMAIL = model.Email;
            user.SODIENTHOAI = model.SoDienThoai;

            // Lưu thay đổi vào database
            try
            {
                data.SubmitChanges();
                Session["User"] = user;
                ViewBag.Message = "Cập nhật thông tin cá nhân thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Cập nhật thất bại: " + ex.Message;
            }

            return View("KhachHangEditProfile", model);
        }


        public ActionResult KhachHangDoiMatKhau()
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        
        [HttpPost]
        public ActionResult KhachHangDoiMatKhau(KhachHangEditPass model)
        {
            var userSession = Session["User"] as NGUOIDUNG;
            if (userSession == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var user = data.NGUOIDUNGs.SingleOrDefault(u => u.MANGUOIDUNG == userSession.MANGUOIDUNG);
            if (user == null)
            {
                return HttpNotFound("Không tìm thấy người dùng.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            if (user.MATKHAU != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View(model);
            }

            
            user.MATKHAU = model.NewPassword; 
            data.SubmitChanges();

            
            Session["User"] = user;
            ViewBag.Message = "Đổi mật khẩu thành công!";
            return View("KhachHangDoiMatKhau");
        }
        private List<SANPHAM> GetRecommendedProducts(int top = 5)
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null) return new List<SANPHAM>();

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null) return new List<SANPHAM>();

            // Lấy danh sách sản phẩm đã mua
            var sanPhamDaMua = data.CHITIETDONHANGs
                .Where(ct => data.DONHANGs.Any(dh => dh.MAKHACHHANG == khachHang.MAKHACHHANG && dh.MADONHANG == ct.MADONHANG))
                .Select(ct => ct.MASANPHAM)
                .Distinct()
                .ToList();

            // Tìm các sản phẩm chưa mua và có số lượng tồn
            var sanPhamGoiY = data.SANPHAMs
                .Where(sp => !sanPhamDaMua.Contains(sp.MASANPHAM)) // Loại trừ các sản phẩm đã mua
                .OrderByDescending(sp => sp.SOLUONGTON) // Có thể thay đổi tiêu chí khác
                .Take(top)
                .ToList();

            return sanPhamGoiY;
        }

    }
}