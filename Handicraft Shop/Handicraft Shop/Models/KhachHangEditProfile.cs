using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Handicraft_Shop.Models
{
    public class KhachHangEditProfile
    {
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 chữ số.")]
        public string SoDienThoai { get; set; }
    }
}
