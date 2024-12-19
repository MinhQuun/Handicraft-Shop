using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using System.IO;

namespace Handicraft_Shop.Controllers
{
    public class NhanVienKhoHangController : Controller
    {
        // GET: NhanVienKhoHang
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult IndexNhanVienKhoHang(int page = 1, int pageSize = 12)
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

        
        public ActionResult NhanVienKhoHangDetails(string id)
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
        public ActionResult NhanVienKhoHangMenuCap1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();

            return PartialView(dmsp);
        }
        public ActionResult NhanVienKhoHangMenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        public ActionResult NhanVienKhoHangLocDL_Theoloai(string mdm)
        {
            List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            return View("IndexNhanVienKhoHang", ds);
        }
        public ActionResult NhanVienKhoHangSearch(string searchString, int page = 1, int pageSize = 12)
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

            return View("NhanVienKhoHangSearch", sp.ToList());
        }
        public ActionResult NhanVienKhoHangQuanLyNhaCungCap(int page = 1, int pageSize = 10)
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
                return RedirectToAction("NhanVienKhoHangQuanLyNhaCungCap");
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
                    return RedirectToAction("NhanVienKhoHangQuanLyNhaCungCap");
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

            return RedirectToAction("NhanVienKhoHangQuanLyNhaCungCap");
        }

        public ActionResult NhanVienKhoHangQuanLySanPham(int page = 1, int pageSize = 10, int? minValue = null, int? maxValue = null)
        {
            var query = data.SANPHAMs.AsQueryable();

            // Lọc theo số lượng tồn nếu có giá trị lọc
            if (minValue.HasValue)
            {
                query = query.Where(sp => sp.SOLUONGTON >= minValue.Value);
            }
            if (maxValue.HasValue)
            {
                query = query.Where(sp => sp.SOLUONGTON <= maxValue.Value);
            }

            // Phân trang
            int totalProducts = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var products = query
                            .OrderBy(p => p.MASANPHAM)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Truyền dữ liệu phân trang và lọc vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // Trả về View với danh sách sản phẩm đã lọc và phân trang
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

            return RedirectToAction("NhanVienKhoHangQuanLySanPham");
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
                return RedirectToAction("NhanVienKhoHangQuanLySanPham");
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

            return RedirectToAction("NhanVienKhoHangQuanLySanPham");
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra quyền để đảm bảo là nhân viên kho hàng
            var role = Session["UserRole"] as string;
            if (role != "NhanVienKhoHang")
            {
                filterContext.Result = RedirectToAction("Login", "Home");
            }

            // Tính toán và cập nhật số lượng tồn tự động
            

            base.OnActionExecuting(filterContext);
        }
        public ActionResult NhanVienKhoHangLoc_SoLuongTon(int? minValue, int? maxValue, int page = 1, int pageSize = 10)
        {
            // Lấy danh sách sản phẩm và lọc theo khoảng số lượng tồn
            var sanPhams = data.SANPHAMs.AsQueryable();

            if (minValue.HasValue)
            {
                sanPhams = sanPhams.Where(sp => sp.SOLUONGTON >= minValue.Value);
            }

            if (maxValue.HasValue)
            {
                sanPhams = sanPhams.Where(sp => sp.SOLUONGTON <= maxValue.Value);
            }

            // Phân trang
            int totalProducts = sanPhams.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            sanPhams = sanPhams.OrderBy(sp => sp.MASANPHAM)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View("NhanVienKhoHangQuanLySanPham", sanPhams.ToList());
        }



    }
}