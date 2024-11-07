using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Handicraft_Shop.Models
{
    public class KhachHangEditProfile
    {
        public string HoTen { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}