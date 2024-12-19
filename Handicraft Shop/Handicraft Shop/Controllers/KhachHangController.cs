using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using BCrypt.Net;
using QRCoder;
using System.IO;

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



        public ActionResult GeneratePaymentQRCode(int orderId)
        {
            var order = data.DONHANGs.SingleOrDefault(d => d.MADONHANG == orderId);

            if (order == null || order.TONGTHANHTIEN == null)
            {
                return HttpNotFound();
            }

            // Thông tin thanh toán
            string accountNumber = "1051659273";
            string accountName = "TRUONG DOAN";
            string bankName = "Vietcombank";
            string amount = order.TONGTHANHTIEN.ToString(); // Số tiền thanh toán
            string description = $"Thanh toan DH{order.MADONHANG} - {amount} VND"; // Nội dung chi tiết đơn hàng

            // Tạo chuỗi QR theo chuẩn VietQR
            string qrContent =
                $"Giá trị: {amount}\n" +
                $"Nội dung TT: {description}";

            // Tạo mã QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (MemoryStream ms = new MemoryStream())
            {
                using (var qrBitmap = qrCode.GetGraphic(20))
                {
                    qrBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            }
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
                // Tìm các chi tiết đơn hàng
                var chiTietDonHang = data.CHITIETDONHANGs.Where(ct => ct.MADONHANG == id).ToList();

                foreach (var item in chiTietDonHang)
                {
                    // Cập nhật số lượng tồn kho của sản phẩm
                    var sanPham = data.SANPHAMs.SingleOrDefault(sp => sp.MASANPHAM == item.MASANPHAM);
                    if (sanPham != null)
                    {
                        sanPham.SOLUONGTON += item.SOLUONG ?? 0; // Cộng lại số lượng sản phẩm vào kho
                    }
                }

                // Cập nhật trạng thái đơn hàng thành "Đã hủy"
                donHang.TRANGTHAI = "Đã hủy";

                // Lưu thay đổi vào database
                data.SubmitChanges();
            }

            return RedirectToAction("KhachHangLichSuDonHang");
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
            var recommendedProducts = GetRecommendedProducts();
            ViewBag.RecommendedProducts = recommendedProducts;
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
        public ActionResult KhachHangThemMatHangAjax(string MASANPHAM, int SoLuong = 1)
        {
            var sp = data.SANPHAMs.SingleOrDefault(p => p.MASANPHAM == MASANPHAM);

            if (sp == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            // Lấy giỏ hàng từ Session
            GioHang gh = Session["GioHang"] as GioHang ?? new GioHang();

            // Kiểm tra tổng số lượng đã có trong giỏ hàng
            var soLuongDaCo = gh.list.Where(x => x.MaSP == MASANPHAM).Sum(x => x.SoLuong);
            var tongSoLuongMoi = soLuongDaCo + SoLuong;

            if (tongSoLuongMoi > sp.SOLUONGTON)
            {
                return Json(new
                {
                    success = false,
                    message = $"Số lượng tồn chỉ còn {sp.SOLUONGTON}. Hiện bạn đã có {soLuongDaCo} sản phẩm này trong giỏ hàng."
                });
            }

            // Thêm sản phẩm vào giỏ hàng
            gh.Them(MASANPHAM, SoLuong);
            Session["GioHang"] = gh;

            return Json(new { success = true, cartCount = gh.SoMatHang() });
        }

        [HttpPost]
        public ActionResult KiemTraSanPhamHetHang(string MASANPHAM)
        {
            var sp = data.SANPHAMs.SingleOrDefault(p => p.MASANPHAM == MASANPHAM);

            if (sp == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            if (sp.SOLUONGTON <= 0)
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng." });
            }

            return Json(new { success = true });
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
            // Lấy giỏ hàng từ Session
            GioHang gh = Session["GioHang"] as GioHang ?? new GioHang();

            // Lấy thông tin sản phẩm từ database
            var sanPham = data.SANPHAMs.SingleOrDefault(sp => sp.MASANPHAM == id);
            if (sanPham == null || sanPham.SOLUONGTON <= 0)
            {
                return RedirectToAction("KhachHangXemGioHang");
            }

            // Tính tổng số lượng hiện tại
            var gioHangItem = gh.list.FirstOrDefault(p => p.MaSP == id);
            if (gioHangItem != null)
            {
                int tongSoLuong = gioHangItem.SoLuong + 1;

                if (tongSoLuong > sanPham.SOLUONGTON)
                {
                    ViewBag.ErrorMessage = "Số lượng sản phẩm đã đạt giới hạn tồn kho.";
                }
                else
                {
                    gh.Them(id);
                }
            }

            Session["GioHang"] = gh;
            return RedirectToAction("KhachHangXemGioHang");
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
            // Kiểm tra khách hàng đã đăng nhập hay chưa
            // Lấy thông tin khách hàng đang đăng nhập
            NGUOIDUNG khach = Session["User"] as NGUOIDUNG;
            ViewBag.k = khach;

            // Lấy mã khách hàng từ bảng KHACHHANG
            var khachHang = data.KHACHHANGs.FirstOrDefault(kh => kh.MANGUOIDUNG == khach.MANGUOIDUNG);

            if (khachHang != null)
            {
                int maKhachHang = khachHang.MAKHACHHANG;

                // Lấy danh sách địa chỉ gợi ý từ bảng DIACHI_GIAOHANG
                var diaChiGoiY = data.DIACHI_GIAOHANGs
                    .Where(dc => dc.MAKHACHHANG == maKhachHang)
                    .Select(dc => dc.DIACHI)
                    .Distinct()
                    .ToList();

                // Truyền danh sách địa chỉ vào ViewBag
                ViewBag.DiaChiGoiY = diaChiGoiY;
            }
            else
            {
                ViewBag.DiaChiGoiY = new List<string>(); // Nếu không tìm thấy khách hàng
            }

            // Trả về View giỏ hàng để đặt hàng
            GioHang gh = Session["GioHang"] as GioHang;
            return View(gh);

        }


        [HttpPost]
        public ActionResult KhachHangConfirmOrder(string diachigh, string SelectedRole, string ghiChu)
        {
            var nguoiDung = Session["User"] as NGUOIDUNG;
            if (nguoiDung == null)
            {
                return RedirectToAction("Login", "Home");
            }

            string maNguoiDung = nguoiDung.MANGUOIDUNG;

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == maNguoiDung);
            if (khachHang == null)
            {
                khachHang = new KHACHHANG
                {
                    MANGUOIDUNG = maNguoiDung,
                    HOTEN = nguoiDung.TENNGUOIDUNG,
                    SODIENTHOAI = nguoiDung.SODIENTHOAI,
                    EMAIL = nguoiDung.EMAIL
                };
                data.KHACHHANGs.InsertOnSubmit(khachHang);
                data.SubmitChanges();
            }

            var gh = Session["GioHang"] as GioHang;
            if (gh == null || !gh.list.Any())
            {
                ViewBag.ErrorMessage = "Giỏ hàng trống.";
                return RedirectToAction("KhachHangXemGioHang", "KhachHang");
            }

            var diaChiGiaoHang = new DIACHI_GIAOHANG
            {
                MAKHACHHANG = khachHang.MAKHACHHANG,
                DIACHI = diachigh
            };
            data.DIACHI_GIAOHANGs.InsertOnSubmit(diaChiGiaoHang);
            data.SubmitChanges();

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

            // Thêm chi tiết đơn hàng và cập nhật số lượng tồn
            foreach (var item in gh.list)
            {
                var sanPham = data.SANPHAMs.SingleOrDefault(sp => sp.MASANPHAM == item.MaSP);
                if (sanPham != null)
                {
                    // Cập nhật số lượng tồn
                    sanPham.SOLUONGTON -= item.SoLuong;

                    // Đảm bảo số lượng tồn không nhỏ hơn 0
                    if (sanPham.SOLUONGTON < 0)
                    {
                        sanPham.SOLUONGTON = 0;
                    }
                }

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

            // Lấy thông tin người dùng từ bảng NGUOIDUNG
            var nguoiDung = data.NGUOIDUNGs.SingleOrDefault(nd => nd.MANGUOIDUNG == user.MANGUOIDUNG);
            if (nguoiDung == null)
            {
                return HttpNotFound();
            }

            return View(nguoiDung);
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

            // Kiểm tra email trùng lặp
            if (data.NGUOIDUNGs.Any(u => u.EMAIL == model.Email && u.MANGUOIDUNG != userSession.MANGUOIDUNG))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng. Vui lòng chọn email khác.");
                return View("KhachHangEditProfile", model);
            }

            // Kiểm tra số điện thoại trùng lặp
            if (data.NGUOIDUNGs.Any(u => u.SODIENTHOAI == model.SoDienThoai && u.MANGUOIDUNG != userSession.MANGUOIDUNG))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng. Vui lòng chọn số khác.");
                return View("KhachHangEditProfile", model);
            }

            // Cập nhật thông tin
            user.TENNGUOIDUNG = model.HoTen;
            user.EMAIL = model.Email;
            user.SODIENTHOAI = model.SoDienThoai;

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
        private List<SANPHAM> GetRecommendedProducts(int topPerType = 2)
        {
            var user = Session["User"] as NGUOIDUNG;
            if (user == null) return new List<SANPHAM>();

            var khachHang = data.KHACHHANGs.SingleOrDefault(k => k.MANGUOIDUNG == user.MANGUOIDUNG);
            if (khachHang == null) return new List<SANPHAM>();

            // Lấy danh sách sản phẩm khách hàng đã mua
            var sanPhamDaMua = data.CHITIETDONHANGs
                .Where(ct => data.DONHANGs.Any(dh => dh.MAKHACHHANG == khachHang.MAKHACHHANG && dh.MADONHANG == ct.MADONHANG))
                .Select(ct => ct.MASANPHAM)
                .Distinct()
                .ToList();

            // Lấy danh sách loại sản phẩm mà khách hàng đã mua
            var loaiSanPhamDaMua = data.SANPHAMs
                .Where(sp => sanPhamDaMua.Contains(sp.MASANPHAM))
                .Select(sp => sp.MALOAI)
                .Distinct()
                .ToList();

            // Gợi ý sản phẩm: Lấy 2 sản phẩm từ mỗi loại đã mua
            var sanPhamGoiY = new List<SANPHAM>();

            foreach (var loai in loaiSanPhamDaMua)
            {
                var sanPhamLienQuan = data.SANPHAMs
                    .Where(sp => sp.MALOAI == loai && !sanPhamDaMua.Contains(sp.MASANPHAM) && sp.SOLUONGTON > 0)
                    .OrderByDescending(sp => sp.SOLUONGTON)
                    .Take(topPerType)
                    .ToList();

                sanPhamGoiY.AddRange(sanPhamLienQuan);
            }

            return sanPhamGoiY;
        }


    }
}