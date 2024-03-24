using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using prjTravelPlatform_release.Areas.Customer.ViewModel.Airline;
using prjTravelPlatform_release.Models;
using prjTravelPlatformV3.Models;
using System.Security.Claims;


namespace prjTravelPlatform_release.Areas.Customer.Controllers.Airline
{
    [Area("Customer")]
    public class FlightController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly dbTravalPlatformContext _context;
        public FlightController(IHttpContextAccessor httpContextAccessor, dbTravalPlatformContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Search()
        {
            ViewBag.WeatherKey = "CWA-69D21D85-3507-4963-BD88-F0FC93735B67";
            return View();
        }

        public IActionResult Map()
        {
            ViewBag.WeatherKey = "CWA-69D21D85-3507-4963-BD88-F0FC93735B67";
            return View();
        }

        public IActionResult Booking()
        {
            var CustomerId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (CustomerId == 0)
            {
                return RedirectToAction("Index", "Login", new { area = "CustomizedIdentity" });
            }
            ViewBag.CustomerId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.Email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            return View();
        }

        public IActionResult OrderConfirm()
        {
            var CustomerId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (CustomerId == 0)
            {
                return RedirectToAction("Index", "Login", new { area = "CustomizedIdentity" });
            }
            ViewBag.CustomerId = Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.Email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            return View();
        }
    }
}
