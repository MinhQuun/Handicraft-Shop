using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Handicraft_Shop.Models
{
    public class CartItem
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public string AnhBia { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien
        {
            get { return SoLuong * DonGia; }
        }
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public CartItem(string masp)
        {
            SANPHAM sp = data.SANPHAMs.SingleOrDefault(t => t.MASANPHAM == masp);
            if (sp != null)
            {
                MaSP = masp;
                TenSP = sp.TENSANPHAM;
                AnhBia = sp.HINHANH;
                DonGia = double.Parse(sp.GIABAN.ToString());
                SoLuong = 1;
            }
        }
        public CartItem(string masp, int sl)
        {
            SANPHAM sp = data.SANPHAMs.SingleOrDefault(t => t.MASANPHAM == masp);
            if (sp != null)
            {
                MaSP = masp;
                TenSP = sp.TENSANPHAM;
                AnhBia = sp.HINHANH;
                DonGia = double.Parse(sp.GIABAN.ToString());
                SoLuong = sl;
            }
        }
    }
}