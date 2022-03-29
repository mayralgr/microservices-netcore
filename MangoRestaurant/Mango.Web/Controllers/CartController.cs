using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(IProductService productService,ICartService cartService, ICouponService couponService)
        {
            _productService = productService;
             _cartService = cartService; 
            _couponService = couponService;
        }
        public async Task<IActionResult> CartIndex()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            return View(await LoadCartDtoBasedOnLoggedInUser());
        }

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            CartDto cartDto = new();
            var userId = User.Claims
                    .Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            if (userId != null)
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var response = await _cartService.RemoveFromCartAsync<ResponseDto>(cartDetailsId, accessToken);
                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(CartIndex));
                }
            }
            return View();
        }

        private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
        {
            CartDto cartDto = new();
            var userId = User.Claims
                    .Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            if (userId != null)
            {
                var accessToken = await HttpContext.GetTokenAsync("access_token");
                var response = await _cartService.GetCartByUserIdAsync<ResponseDto>(userId, accessToken);
                if (response != null && response.IsSuccess)
                {
                    cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                }
                if (cartDto.CartHeader != null)
                {

                    foreach (var detail in cartDto.CartDetails)
                    {
                        cartDto.CartHeader.OrderTotal += detail.Count * detail.Product.Price;

                    }
                    if (!String.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                    {
                        CouponDto coupon = new();
                        var couponResponse = await _couponService.GetDiscountForCode<ResponseDto>(cartDto.CartHeader.CouponCode, accessToken);
                        if (couponResponse != null && couponResponse.IsSuccess)
                        {
                            coupon = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(couponResponse.Result));
                            cartDto.CartHeader.DiscountTotal = coupon.DiscountAmount;
                        }
                        cartDto.CartHeader.OrderTotal -= cartDto.CartHeader.DiscountTotal;
                    }
                }
            }
            return cartDto;
        }

        [HttpPost]
        [ActionName("ApplyCoupon")]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.ApplyCoupon<ResponseDto>(cartDto, accessToken);
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

        [HttpPost]
        [ActionName("RemoveCoupon")]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var response = await _cartService.RemoveCoupon<ResponseDto>(cartDto.CartHeader.UserId, accessToken); ;
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }
    }
}
