using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CartService: BaseService, ICartService
    {
        private readonly IHttpClientFactory _clientFactory;

        public CartService(IHttpClientFactory clientFactory): base(clientFactory)
        {
            _clientFactory = clientFactory; 
        }

        public async Task<T> AddToCartAsync<T>(CartDto cartDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + "/api/cart/AddCart",
                AccessToken = token 
            });
        }

        public async Task<T> ApplyCoupon<T>(CartDto cartDto, string token = null)
        {
            return await SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = cartDto,
                Url = $"{SD.ShoppingCartAPIBase}/api/cart/ApplyCoupon",
                AccessToken = token
            });
        }

        public async Task<T> Checkout<T>(CartHeaderDto cartHeaderDto, string token = null)
        {
            return await SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = cartHeaderDto,
                Url = $"{SD.ShoppingCartAPIBase}/api/cart/Checkout",
                AccessToken = token
            });
        }

        public async Task<T> ClearCart<T>(string userId, string token = null)
        {
            return await SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = userId,
                Url = $"{SD.ShoppingCartAPIBase}/api/cart/clearCart",
                AccessToken = token
            });
        }

        public async Task<T> GetCartByUserIdAsync<T>(string userId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.GET,
                Url = SD.ShoppingCartAPIBase + "/api/cart/GetCart/"+userId,
                AccessToken = token
            });
        }

        public async Task<T> RemoveCoupon<T>(string userId, string token = null)
        {
            return await SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = userId,
                Url = $"{SD.ShoppingCartAPIBase}/api/cart/RemoveCoupon",
                AccessToken = token
            });
        }

        public async Task<T> RemoveFromCartAsync<T>(int cartId, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = cartId,
                Url = SD.ShoppingCartAPIBase + "/api/cart/RemoveCart",
                AccessToken = token
            });
        }

        public async Task<T> UpdateCartAsync<T>(CartDto cartDto, string token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiMethodType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBase + "/api/cart/UpdateCart",
                AccessToken = token
            });
        }
    }
}
