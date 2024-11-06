using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Handicraft_Shop.Models
{
    public class TopSanPhamViewModel
    {
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; } 
        public int TongSoLuongBan { get; set; }
        public decimal TongDoanhThu { get; set; }
    }
}