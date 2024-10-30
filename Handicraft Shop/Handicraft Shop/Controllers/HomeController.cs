using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;
using System.IO;
namespace Handicraft_Shop.Controllers
{
    public class HomeController : Controller
    {
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult Index()
        {
            List<SANPHAM> sp = data.SANPHAMs.ToList();
            return View(sp);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Tìm người dùng có tài khoản và mật khẩu trùng khớp
            var user = data.NGUOIDUNGs.FirstOrDefault(u =>
                            u.TAIKHOAN == username && u.MATKHAU == password);

            if (user != null)
            {
                // Lấy quyền của người dùng từ bảng QUYEN_NGUOIDUNG
                var userRole = (from quyenNguoiDung in data.QUYEN_NGUOIDUNGs
                                join quyen in data.QUYENs on quyenNguoiDung.MAQUYEN equals quyen.MAQUYEN
                                where quyenNguoiDung.MANGUOIDUNG == user.MANGUOIDUNG
                                select quyen.TENQUYEN).FirstOrDefault();

                if (userRole != null)
                {
                    // Lưu thông tin đăng nhập vào Session
                    Session["UserName"] = user.TENNGUOIDUNG;
                    Session["UserRole"] = userRole;

                    // Điều hướng người dùng đến Controller tương ứng theo quyền
                    if (userRole == "Admin")
                        return RedirectToAction("IndexAdmin", "Admin");
                    else if (userRole == "NhanVien")
                        return RedirectToAction("Index", "NhanVien");
                    else if (userRole == "KhachHang")
                        return RedirectToAction("Index", "KhachHang");
                }
            }

            ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
            return View();
        }
        public ActionResult Logout()
        {
            // Xóa Session và điều hướng về trang đăng nhập
            Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Details(string id)
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
        public ActionResult Menuc1()
        {
            List<DANHMUCSANPHAM> dmsp = data.DANHMUCSANPHAMs.ToList();

            return PartialView(dmsp);
        }
        public ActionResult MenuCap2(int madm)
        {
            List<LOAI> dsloai = data.LOAIs.Where(t => t.MADANHMUC == madm).ToList();
            return PartialView(dsloai);
        }
        //public ActionResult LocDL_Theoloai(string mdm)
        //{
        //    List<SANPHAM> ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
        //    return View("Index", ds);
        //}
        public ActionResult LocDL_Theoloai(string mdm)
        {
            System.Diagnostics.Debug.WriteLine("Mã loại nhận được: " + mdm);

            var ds = data.SANPHAMs.Where(t => t.MALOAI == mdm).ToList();
            System.Diagnostics.Debug.WriteLine("Số sản phẩm lọc được: " + ds.Count);

            if (ds.Count == 0)
            {
                TempData["ErrorMessage"] = "Không có sản phẩm nào thuộc loại này.";
            }

            return View("Index", ds);
        }

        public ActionResult Search(string searchString, int[] categoryIds)
        {
            var sp = from a in data.SANPHAMs select a;
            if (!string.IsNullOrEmpty(searchString))
            {
                sp = sp.Where(s => s.TENSANPHAM.Contains(searchString));
            }
            return View("Search", sp.ToList());
        }

        // them
        [HttpGet]
        public ActionResult AddItem()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddItem(SANPHAM item, HttpPostedFileBase HINHANH)
        {
            var existingProduct = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == item.MASANPHAM);
            if (existingProduct != null)
            {
                ModelState.AddModelError("MASANPHAM", "Mã sản phẩm đã tồn tại. Vui lòng nhập mã khác.");
                return View(item);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (HINHANH != null && HINHANH.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(HINHANH.FileName)
                                          + "_" + DateTime.Now.Ticks + Path.GetExtension(HINHANH.FileName);
                        string folderPath = Server.MapPath("~/HinhAnhSP/MUC HINH ANH CHUNG/");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        string filePath = Path.Combine(folderPath, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            ModelState.AddModelError("", "Tệp đã tồn tại. Vui lòng chọn tệp khác.");
                            return View(item);
                        }
                        HINHANH.SaveAs(filePath);
                        item.HINHANH = "" + fileName;
                    }
                    data.SANPHAMs.InsertOnSubmit(item);
                    data.SubmitChanges();

                    TempData["SuccessMessage"] = "Sản phẩm đã được thêm thành công.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại.");
                }
            }

            return View(item);
        }
        public ActionResult DeleteProductById()
        {
            return View();
        }

        // POST: Nhận mã sản phẩm và hiển thị thông tin sản phẩm
        [HttpPost]
        public ActionResult DeleteProductById(string productId)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == productId);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("DeleteProductById");
            }

            // Nếu tìm thấy sản phẩm, trả về view với thông tin sản phẩm
            return View("DeleteConfirmed", sanPham);
        }

        // POST: Xác nhận xóa sản phẩm
        [HttpPost, ActionName("DeleteConfirmed")]
        public ActionResult DeleteConfirmed(string id)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == id);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa.";
                return RedirectToAction("DeleteProductById");
            }

            // Xóa sản phẩm và lưu thay đổi
            data.SANPHAMs.DeleteOnSubmit(sanPham);
            data.SubmitChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được xóa thành công.";
            return RedirectToAction("DeleteProductById");
        }


        // GET: Hiển thị trang nhập mã sản phẩm
        public ActionResult InputProductId()
        {
            return View();
        }

        // POST: Kiểm tra mã sản phẩm và chuyển đến trang chỉnh sửa nếu hợp lệ
        [HttpPost]
        public ActionResult InputProductId(string productId)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == productId);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Mã sản phẩm không tồn tại.";
                return RedirectToAction("InputProductId");
            }

            // Chuyển đến trang chỉnh sửa với thông tin sản phẩm
            return View("EditProduct", sanPham);
        }

        // POST: Lưu thông tin sản phẩm sau khi chỉnh sửa
        //[HttpPost]
        //public ActionResult EditProduct(SANPHAM updatedProduct)
        //{
        //    var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == updatedProduct.MASANPHAM);
        //    if (sanPham == null)
        //    {
        //        TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
        //        return RedirectToAction("InputProductId");
        //    }

        //    // Cập nhật thông tin sản phẩm
        //    sanPham.TENSANPHAM = updatedProduct.TENSANPHAM;
        //    sanPham.GIABAN = updatedProduct.GIABAN;
        //    sanPham.HINHANH = updatedProduct.HINHANH;
        //    sanPham.MOTA = updatedProduct.MOTA;
        //    sanPham.SOLUONGTON = updatedProduct.SOLUONGTON;
        //    sanPham.MALOAI = updatedProduct.MALOAI;

        //    // Lưu thay đổi vào cơ sở dữ liệu
        //    data.SubmitChanges();

        //    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công.";
        //    return RedirectToAction("InputProductId");
        //}

        [HttpGet]
        public ActionResult EditProduct(string productId)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == productId);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("InputProductId");
            }
            return View(sanPham);
        }

        [HttpPost]
        public ActionResult EditProduct(SANPHAM updatedProduct, HttpPostedFileBase HINHANH)
        {
            var sanPham = data.SANPHAMs.FirstOrDefault(sp => sp.MASANPHAM == updatedProduct.MASANPHAM);
            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("InputProductId");
            }

            try
            {
                if (HINHANH != null && HINHANH.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(HINHANH.FileName)
                                      + "_" + DateTime.Now.Ticks + Path.GetExtension(HINHANH.FileName);
                    string folderPath = Server.MapPath("~/HinhAnhSP/MUC HINH ANH CHUNG/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filePath = Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        ModelState.AddModelError("", "Tệp đã tồn tại. Vui lòng chọn tệp khác.");
                        return View(updatedProduct);
                    }
                    HINHANH.SaveAs(filePath);
                    sanPham.HINHANH = "" + fileName;
                }
                sanPham.TENSANPHAM = updatedProduct.TENSANPHAM;
                sanPham.GIABAN = updatedProduct.GIABAN;
                sanPham.MOTA = updatedProduct.MOTA;
                sanPham.SOLUONGTON = updatedProduct.SOLUONGTON;
                sanPham.MALOAI = updatedProduct.MALOAI;
                data.SubmitChanges();

                TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception and return error message
                System.Diagnostics.Debug.WriteLine("Lỗi: " + ex.Message);
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật sản phẩm. Vui lòng thử lại.");
                return View(updatedProduct);
            }
        }
    }
}
   
   



