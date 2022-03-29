namespace Mango.Web.Services.IServices
{
    public interface ICouponService
    {
        Task<T> GetDiscountForCode<T>(string code, string token = null);
    }
}
