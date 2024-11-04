using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Handicraft_Shop.Models
{
    public class DonHangViewModel
    {
        public DONHANG DonHang { get; set; }
        public List<ChiTietSanPhamViewModel> ChiTietSanPhams { get; set; }
    }
    public class ChiTietSanPhamViewModel
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}