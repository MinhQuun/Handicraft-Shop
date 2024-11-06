using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Handicraft_Shop.Models
{
    public class SanPhamMuaViewModel
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongMua { get; set; }
        public string HinhAnh { get; set; }
    }
    public class KhachHangMuaNhieuNhat
    {
        public int MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public int TongSoLuongMua { get; set; }
        public int TongSoLuongTatCaSanPham { get; set; } 
        public List<SanPhamMuaViewModel> SanPhamsDaMua { get; set; } // Danh sách sản phẩm đã mua
    }
}