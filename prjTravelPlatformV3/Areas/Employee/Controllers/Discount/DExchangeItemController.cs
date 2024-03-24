using CoreMVC_SignalR_Chat.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjTravelPlatformV3.Areas.Employee.ViewModels.Discount;
using prjTravelPlatformV3.Models;

namespace prjTravelPlatformV3.Areas.Employee.Controllers.Discount
{
    [Area("Employee")]
    public class DExchangeItemController : Controller
    {
       
        private readonly dbTravalPlatformContext _context;
        public DExchangeItemController(dbTravalPlatformContext context)
        {
            _context = context;
            
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetPartial(int? id)
        {
            if (id == 0)
            {
                ViewBag.formId = "Create";
                ViewBag.title = "新增兌換商品資料";
                return PartialView("_ModalPartial", new ExcangeViewModel());
            }
            if (_context.TDcExchangeItems == null)
            {
                return NotFound();
            }
            var tExchange = _context.TDcExchangeItems.Find(id);
            if (tExchange == null)
            {
                return NotFound();
            }
            ExcangeViewModel E = new ExcangeViewModel
            {
                fId = tExchange.FProductId,
                fProductName = tExchange.FName,             
                fPoint = tExchange.FPointsRequired,
                fMoney= tExchange.FMoneyRequired,
                fQty = tExchange.FQuantity,
                fType = tExchange.FProductType,
                fImgPath=tExchange.FImagePath,
                fNote = tExchange.FNote,

            };
            ViewBag.formId = "Edit";
            ViewBag.title = "編輯兌換商品資料";
            return PartialView("_ModalPartial", E);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TDcExchangeItems == null)
            {
                return Problem("Entity set 'dbTravalPlatformContext.TExchangeItems'  is null.");
            }
            var tExchange = await _context.TDcExchangeItems.FindAsync(id);
            if (tExchange != null)
            {
                _context.TDcExchangeItems.Remove(tExchange);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TCouponListExists(int id)
        {
            return (_context.TDcExchangeItems?.Any(e => e.FProductId == id)).GetValueOrDefault();
        }


       
    }
}
