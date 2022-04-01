﻿using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public CouponRepository(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _configuration = config;
        }
        public async Task<CouponDto> GetCoupon(string couponName)
        {
            string completeUrl = "api/coupon/" + couponName;
            var response  = await _httpClient.GetAsync(completeUrl);
            var apiContent = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if (resp.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(resp.Result));

            }
            return new CouponDto();
        }
    }
}
