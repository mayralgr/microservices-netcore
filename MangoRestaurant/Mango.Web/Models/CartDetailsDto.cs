﻿namespace Mango.Web.Models
{
    public class CartDetailsDto
    {
        public int CartDetailId { get; set; }
        public int CartHeaderId { get; set; }
        public virtual CartHeaderDto? CartHeader { get; set; }
        public int ProductId { get; set; }
        public virtual ProductDto? Product { get; set; }
        public int Count { get; set; } 
    }
}
