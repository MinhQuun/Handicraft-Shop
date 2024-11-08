using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using System.IO;
namespace Handicraft_Shop.Controllers
{
    public class NhanVienController : Controller
    {
        // GET: NhanVien
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexNhanVien(int page = 1, int pageSize = 12)
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
        public ActionResult NhanVienSearch(string searchString, int page = 1, int pageSize = 12)
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

            return View("NhanVienSearch", sp.ToList());
        }
        public ActionResult NhanVienQuanLyNhaCungCap(int page = 1, int pageSize = 10)
        {
            int totalNhaCungCap = data.NHACUNGCAPs.Count();
            int totalPages = (int)Math.Ceiling((double)totalNhaCungCap / pageSize);

            var nhaCungCapList = data.NHACUNGCAPs
                                     .OrderBy(ncc => ncc.MANHACUNGCAP)
                                     .Skip((page - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(nhaCungCapList);
        }

        // Action GET: Hiển thị form thêm nhà cung cấp
        public ActionResult CreateNhaCungCap()
        {
            return View();
        }

        // Action POST: Xử lý việc thêm nhà cung cấp
        [HttpPost]
        public ActionResult CreateNhaCungCap(NHACUNGCAP nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                data.NHACUNGCAPs.InsertOnSubmit(nhaCungCap);
                data.SubmitChanges();
                return RedirectToAction("NhanVienQuanLyNhaCungCap");
            }
            return View(nhaCungCap);
        }

        // Action GET: Hiển thị form sửa nhà cung cấp
        public ActionResult EditNhaCungCap(int id)
        {
            var nhaCungCap = data.NHACUNGCAPs.SingleOrDefault(ncc => ncc.MANHACUNGCAP == id);
            if (nhaCungCap == null)
            {
                return HttpNotFound();
            }
            return View(nhaCungCap);
        }

        // Action POST: Xử lý việc cập nhật thông tin nhà cung cấp
        [HttpPost]
        public ActionResult EditNhaCungCap(NHACUNGCAP nhaCungCap)
        {
            if (ModelState.IsValid)
            {
                var existingNhaCungCap = data.NHACUNGCAPs.SingleOrDefault(ncc => ncc.MANHACUNGCAP == nhaCungCap.MANHACUNGCAP);
                if (existingNhaCungCap != null)
                {
                    existingNhaCungCap.TENNHACUNGCAP = nhaCungCap.TENNHACUNGCAP;
                    existingNhaCungCap.DTHOAI = nhaCungCap.DTHOAI;
                    existingNhaCungCap.DIACHI = nhaCungCap.DIACHI;
                    data.SubmitChanges();
                    return RedirectToAction("NhanVienQuanLyNhaCungCap");
                }
            }
            return View(nhaCungCap);
        }
        public ActionResult DeleteNhaCungCap(int id)
        {
            // Tìm nhà cung cấp trong bảng NHACUNGCAP dựa trên id
            var nhaCungCap = data.NHACUNGCAPs.FirstOrDefault(ncc => ncc.MANHACUNGCAP == id);

            if (nhaCungCap != null)
            {
                // Tìm tất cả sản phẩm có liên quan đến nhà cung cấp này
                var products = data.SANPHAMs.Where(sp => sp.MANHACUNGCAP == id).ToList();

                // Xóa các sản phẩm liên quan
                data.SANPHAMs.DeleteAllOnSubmit(products);

                // Xóa nhà cung cấp
                data.NHACUNGCAPs.DeleteOnSubmit(nhaCungCap);

                // Lưu thay đổi vào cơ sở dữ liệu
                data.SubmitChanges();
            }

            return RedirectToAction("NhanVienQuanLyNhaCungCap");
        }

        public ActionResult NhanVienQuanLySanPham(int page = 1, int pageSize = 10)
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
        // GET: ThemSanPham - Hiển thị form thêm sản phẩm
        [HttpPost]
        public ActionResult CreateSanPham(SANPHAM sanPham, HttpPostedFileBase uploadHinhAnh)
        {
            // Lấy mã sản phẩm lớn nhất hiện tại
            var maxMaSanPham = data.SANPHAMs.OrderByDescending(sp => sp.MASANPHAM).Select(sp => sp.MASANPHAM).FirstOrDefault();
            int nextNumber = 1; // Mặc định là 1 nếu chưa có sản phẩm nào

            if (!string.IsNullOrEmpty(maxMaSanPham) && maxMaSanPham.Length >= 5)
            {
                // Lấy 2 chữ số cuối từ mã sản phẩm hiện tại và chuyển thành số nguyên
                nextNumber = int.Parse(maxMaSanPham.Substring(maxMaSanPham.Length - 2)) + 1;
            }

            // Tạo mã sản phẩm mới với tiền tố "SP" và số thứ tự tăng dần
            sanPham.MASANPHAM = "SP" + nextNumber.ToString("D3");

            // Xử lý upload hình ảnh nếu có
            if (uploadHinhAnh != null && uploadHinhAnh.ContentLength > 0)
            {
                var fileName = Path.GetFileName(uploadHinhAnh.FileName);
                var path = Path.Combine(Server.MapPath("~/HinhAnhSP/"), fileName);
                uploadHinhAnh.SaveAs(path);
                sanPham.HINHANH = fileName;
            }

            // Lưu sản phẩm vào database
            data.SANPHAMs.InsertOnSubmit(sanPham);
            data.SubmitChanges();

            return RedirectToAction("NhanVienQuanLySanPham");
        }

        public ActionResult CreateSanPham()
        {
            // Lấy danh sách nhà cung cấp và loại sản phẩm vào ViewBag để sử dụng trong dropdown list
            ViewBag.NhaCungCapList = new SelectList(data.NHACUNGCAPs, "MANHACUNGCAP", "TENNHACUNGCAP");
            ViewBag.LoaiSanPhamList = new SelectList(data.LOAIs, "MALOAI", "TENLOAI");
            ViewBag.KhuyenMaiList = new SelectList(data.KHUYENMAIs, "MAKHUYENMAI", "TENKHUYENMAI");

            return View();
        }


        // GET: SuaSanPham - Hiển thị form sửa sản phẩm
        public ActionResult EditSanPham(string id)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            ViewBag.NhaCungCapList = new SelectList(data.NHACUNGCAPs, "MANHACUNGCAP", "TENNHACUNGCAP", sanPham.MANHACUNGCAP);
            ViewBag.LoaiList = new SelectList(data.LOAIs, "MALOAI", "TENLOAI", sanPham.MALOAI);
            return View(sanPham);
        }

        // POST: SuaSanPham - Xử lý sửa sản phẩm
        [HttpPost]
        public ActionResult EditSanPham(SANPHAM sanPham, HttpPostedFileBase file)
        {
            var sp = data.SANPHAMs.FirstOrDefault(x => x.MASANPHAM == sanPham.MASANPHAM);
            if (sp != null && ModelState.IsValid)
            {
                sp.TENSANPHAM = sanPham.TENSANPHAM;
                sp.GIABAN = sanPham.GIABAN;
                sp.SOLUONGTON = sanPham.SOLUONGTON;
                sp.MOTA = sanPham.MOTA;
                sp.MANHACUNGCAP = sanPham.MANHACUNGCAP;
                sp.MALOAI = sanPham.MALOAI;

                if (file != null && file.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(file.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/HinhAnhSP"), fileName);
                    file.SaveAs(path);
                    sp.HINHANH = fileName;
                }

                data.SubmitChanges();
                return RedirectToAction("NhanVienQuanLySanPham");
            }

            ViewBag.NhaCungCapList = new SelectList(data.NHACUNGCAPs, "MANHACUNGCAP", "TENNHACUNGCAP", sanPham.MANHACUNGCAP);
            ViewBag.LoaiList = new SelectList(data.LOAIs, "MALOAI", "TENLOAI", sanPham.MALOAI);
            return View(sanPham);
        }

        // GET: XoaSanPham - Xác nhận xóa sản phẩm
        public ActionResult DeleteSanPham(string id)
        {
            // Tìm sản phẩm dựa vào mã sản phẩm
            var sanPham = data.SANPHAMs.SingleOrDefault(sp => sp.MASANPHAM == id);

            if (sanPham == null)
            {
                // Trả về trang lỗi nếu sản phẩm không tồn tại
                return HttpNotFound();
            }

            // Xóa hình ảnh liên quan trong thư mục nếu có
            if (!string.IsNullOrEmpty(sanPham.HINHANH))
            {
                var path = Server.MapPath("~/HinhAnhSP/" + sanPham.HINHANH);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }

            // Xóa sản phẩm khỏi database
            data.SANPHAMs.DeleteOnSubmit(sanPham);
            data.SubmitChanges();

            return RedirectToAction("NhanVienQuanLySanPham");
        }
        public ActionResult NhanVienQuanLyDonHang(int page = 1, int pageSize = 10)
        {
            // Lấy danh sách tất cả các đơn hàng, sắp xếp theo ngày đặt giảm dần
            var donHangList = data.DONHANGs
                                .OrderByDescending(d => d.NGAYDAT)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Tổng số đơn hàng để tính số trang
            int totalDonHang = data.DONHANGs.Count();
            int totalPages = (int)Math.Ceiling((double)totalDonHang / pageSize);

            // Truyền dữ liệu phân trang vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(donHangList);
        }
        public ActionResult NhanVienDonHangDetails(int id)
        {
            // Lấy đơn hàng dựa vào mã đơn hàng
            var donHang = data.DONHANGs.SingleOrDefault(d => d.MADONHANG == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách chi tiết đơn hàng
            var chiTietDonHang = data.CHITIETDONHANGs.Where(c => c.MADONHANG == id).ToList();

            // Truyền dữ liệu chi tiết vào View
            ViewBag.ChiTietDonHang = chiTietDonHang;

            return View(donHang);
        }
        [HttpPost]
        public ActionResult NhanVienUpdateTrangThaiDonHang(int id, string trangThaiMoi)
        {
            // Tìm đơn hàng
            var donHang = data.DONHANGs.SingleOrDefault(d => d.MADONHANG == id);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            // Cập nhật trạng thái mới cho đơn hàng
            donHang.TRANGTHAI = trangThaiMoi;
            data.SubmitChanges();

            return RedirectToAction("NhanVienQuanLyDonHang");
        }
        public ActionResult NhanVienQuanLyKhachHang()
        {
            var users = data.KHACHHANGs.ToList();
            return View(users);
        }

        public ActionResult NhanVienBaoCaoSanPhamBanChay(int page = 1, int pageSize = 10)
        {
            var topSanPhamBanChay = data.CHITIETDONHANGs
                .GroupBy(ct => new { ct.MASANPHAM, ct.SANPHAM.TENSANPHAM, ct.SANPHAM.HINHANH })
                .Select(g => new TopSanPhamViewModel
                {
                    MaSanPham = g.Key.MASANPHAM,
                    TenSanPham = g.Key.TENSANPHAM,
                    TongSoLuongBan = g.Sum(ct => ct.SOLUONG ?? 0),
                    TongDoanhThu = g.Sum(ct => (ct.SOLUONG ?? 0) * (ct.DONGIA ?? 0)),
                    HinhAnh = g.Key.HINHANH
                })
                .OrderByDescending(x => x.TongSoLuongBan)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalProducts = topSanPhamBanChay.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(topSanPhamBanChay);
        }


        public ActionResult NhanVienBaoCaoTonKho(int page = 1, int pageSize = 10)
        {
            var tonKhoData = data.SANPHAMs
                .Select(sp => new TonKhoViewModel
                {
                    MaSanPham = sp.MASANPHAM,
                    TenSanPham = sp.TENSANPHAM,
                    HinhAnh = sp.HINHANH,
                    SoLuongTon = (sp.SOLUONGTON ?? 0) - data.CHITIETDONHANGs
                                            .Where(ct => ct.MASANPHAM == sp.MASANPHAM)
                                            .Sum(ct => (int?)ct.SOLUONG) ?? 0
                })
                .OrderByDescending(sp => sp.SoLuongTon)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalProducts = data.SANPHAMs.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(tonKhoData);
        }

        public ActionResult NhanVienBaoCaoKhachHang(int page = 1, int pageSize = 10)
        {
            var topKhachHang = data.KHACHHANGs
                .Select(kh => new KhachHangMuaNhieuNhat
                {
                    MaKhachHang = kh.MAKHACHHANG,
                    TenKhachHang = kh.HOTEN,
                    SoDienThoai = kh.SODIENTHOAI,
                    Email = kh.EMAIL,
                    TongSoLuongMua = data.DONHANGs
                        .Where(dh => dh.MAKHACHHANG == kh.MAKHACHHANG)
                        .SelectMany(dh => data.CHITIETDONHANGs.Where(ct => ct.MADONHANG == dh.MADONHANG))
                        .Sum(ct => (int?)ct.SOLUONG) ?? 0,
                    TongSoLuongTatCaSanPham = data.DONHANGs
                        .Where(dh => dh.MAKHACHHANG == kh.MAKHACHHANG)
                        .SelectMany(dh => data.CHITIETDONHANGs.Where(ct => ct.MADONHANG == dh.MADONHANG))
                        .Sum(ct => ct.SOLUONG) ?? 0,
                    SanPhamsDaMua = data.DONHANGs
                        .Where(dh => dh.MAKHACHHANG == kh.MAKHACHHANG)
                        .SelectMany(dh => data.CHITIETDONHANGs.Where(ct => ct.MADONHANG == dh.MADONHANG))
                        .Select(ct => new SanPhamMuaViewModel
                        {
                            MaSanPham = ct.MASANPHAM,
                            TenSanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).TENSANPHAM,
                            SoLuongMua = ct.SOLUONG.GetValueOrDefault(),
                            HinhAnh = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == ct.MASANPHAM).HINHANH
                        })
                        .ToList()
                })
                .OrderByDescending(kh => kh.TongSoLuongMua)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCustomers = data.KHACHHANGs.Count();
            int totalPages = (int)Math.Ceiling((double)totalCustomers / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(topKhachHang);
        }


    }
}