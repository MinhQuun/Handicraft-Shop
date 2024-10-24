using System;
using Handicraft_Shop.Models;
public class CartItem
{
    public SANPHAM Product { get; set; }
    public int Quantity { get; set; }

    public decimal TotalPrice => (decimal)(Product.GIABAN * Quantity);
}
