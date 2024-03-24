using Microsoft.AspNetCore.Mvc;
using prjTravelPlatformV3.Areas.Employee.ViewModels.Discount;
using prjTravelPlatformV3.Areas.Employee.ViewModels.Hotel;
using prjTravelPlatformV3.Models;


namespace prjTravelPlatformV3.Areas.Employee.Controllers.Discount
{
    [Route("/api/Discounts/{action}/{id?}")]
    public class DApiCController : Controller
    {

        private readonly dbTravalPlatformContext _context;

        public DApiCController(dbTravalPlatformContext context)
        {
            _context = context;
        }
        #region
        public IActionResult GetData()
        {
            var coupon = from c in _context.VDcouponViews
                         select new
                         {
                             fCouponId = c.優惠卷編號,
                             fCouponCode = c.折扣碼,
                             fCouponName = c.名稱,
                             //fAmount = "$" + Convert.ToInt32(c.使用金額),
                             fDiscount = "$" + Convert.ToInt32(c.折扣值),
                             fStartDate = Convert.ToDateTime(c.啟用日期).ToString("d"),
                             fEndDate = Convert.ToDateTime(c.截止日期).ToString("d"),
                             fProductType = c.商品類型,
                             fRule = c.使用規則,
                             fNote = c.備註,
                             fEnable = (bool)c.啟用狀態 ? "已啟用" : "未啟用"
                         };
            //foreach(var c in coupon)
            //{
            //    c.使用金額 =Convert.ToDecimal ("$"+c.使用金額.ToString()) ;

            //}
            return Json(coupon);
        }

        public IActionResult GetById(int id)
        {
            var coupons = _context.TDcCouponLists.FirstOrDefault(h => h.FCouponId == id);
            return Json(coupons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponViewModel coupon)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                TDcCouponList tCoupon = new TDcCouponList
                {
                    FCouponId = coupon.CouponId,
                    FCouponCode = coupon.CouponCode,
                    FCouponName = coupon.CouponName,
                    FAmount = coupon.Amount,
                    FDiscount = coupon.Discount,
                    FStartDate = coupon.StartDate,
                    FEndDate = coupon.EndDate,
                    FProductType = coupon.ProductType,
                    FRule = coupon.Rule,
                    //FNote = coupon.Note,
                    FEnable = coupon.Enable,
                };
                _context.Add(tCoupon);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "資料新增成功" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = $"資料新增失敗：{e.Message}" });
            }
            //}
            ////驗證沒過            
            //var errors = ModelState.ToDictionary(
            //    kvp => kvp.Key,
            //    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            //);
            ////var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            //return Json(new
            //{
            //    success = false,
            //    message = "資料驗證失敗",
            //    errors
            //});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponViewModel coupon)
        {
            //if (ModelState.IsValid)
            //{
            try
            {
                TDcCouponList tCoupon = new TDcCouponList
                {
                    FCouponId = coupon.CouponId,
                    FCouponCode = coupon.CouponCode,
                    FCouponName = coupon.CouponName,
                    FAmount = coupon.Amount,
                    FDiscount = coupon.Discount,
                    FStartDate = coupon.StartDate,
                    FEndDate = coupon.EndDate,
                    FProductType = coupon.ProductType,
                    FRule = coupon.Rule,
                    //FNote = coupon.Note,
                    FEnable = coupon.Enable,
                };
                _context.Update(tCoupon);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "資料修改成功" });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = $"資料修改失敗：{e.Message}" });
            }
            //}
            ////驗證沒過            
            //var errors = ModelState.ToDictionary(
            //    kvp => kvp.Key,
            //    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            //);
            ////var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            //return Json(new
            //{
            //    success = false,
            //    message = "資料驗證失敗",
            //    errors
            //});
        }
        #endregion
    }
}
