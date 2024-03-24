using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using prjTravelPlatform_release.Areas.Customer.ViewModel.Customers;
using prjTravelPlatformV3.Models;
using System.Security.Claims;

namespace prjTravelPlatform_release.Areas.Customer.Controllers.Customer
{
    [Area("Customer")]
    public class UserprofileController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly dbTravalPlatformContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserprofileController(dbTravalPlatformContext context, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index(string prop)
        {
            
            ViewBag.UserName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            ViewBag.UserEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            ViewBag.UserId= _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.keyword = prop;
            ViewBag.UserImage = _context.TCustomers.Where
                (a => a.FCustomerId == Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)))
                .Select(a => a.FImagePath).FirstOrDefault();

            return View();
        }
        public IActionResult VisaOrderPartial() 
        { 
            return View(); 
        }
        [HttpGet]
        public IActionResult GetUserProfile()
        {
            // 從 EF 中檢索用戶檔案資料
            var user = _context.TCustomers.FirstOrDefault(u => u.FCustomerId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 將用戶資料傳遞給部分視圖
            return Json(user);
        }
        [HttpPost]
        public IActionResult SaveUserData([FromBody] UserprofileViewModel userData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // 保存数据到数据库
            // 这里假设您有一个数据库上下文，可以使用它来保存数据
            // 下面的示例假设您的数据库上下文为 DbContext
            TCustomer u = _context.TCustomers.FirstOrDefault(x => x.FCustomerId == userData.FCustomerId);
            if (u != null)
            {
                u.FName = userData.FName;
                u.FEmail = userData.FEmail;
                u.FGender = userData.FGender;
                u.FPhone = userData.FPhone;
                u.FAddress = userData.FAddress;
                u.FBirth = userData.FBirth;
            }
            else
            {
                // 如果未找到对应的用户，则进行适当的处理
                return NotFound();
            }

            _context.TCustomers.Update(u);
            _context.SaveChanges();

            return Ok(true);
        }
        
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordViewModel pwd)
        {
            // 確認新密碼與確認新密碼相同
            if (pwd.usernewpassword != pwd.usernewpasswordcheck)
            {
                return BadRequest();
            }

            // 從資料庫中取得使用者
            var user = await _context.TCustomers.FirstOrDefaultAsync(u => u.FPassword == pwd.userpassword);

            if (user == null)
            {
                return BadRequest("密碼錯誤");
            }

            // 檢查新密碼格式
            if (!IsValidPassword(pwd.usernewpassword))
            {
                return BadRequest();
            }

            //  // 更新密碼
            user.FPassword = pwd.usernewpassword;
            _context.TCustomers.Update(user);
            await _context.SaveChangesAsync();

            return Ok();

        }

        private bool IsValidPassword(string password)
        {
            // 密碼格式驗證：由一個大寫英文、一個小寫英文和一個數字組成，且總長度不超過16
            return password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && password.Length <= 16;
        }
        [HttpPost]
        public IActionResult UploadImage(IFormFile avatar)
        {
            
            try
            {
                // 检查是否有文件被上传
                if (avatar != null && avatar.Length > 0)
                {
                    // 指定图片存储的路径
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img", "uploads");
                    string uniqueFileName = avatar.FileName;
                    string imagePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // 将图片保存到指定路径
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        avatar.CopyTo(stream);
                    }

                    // 更新当前用户的图片路径
                    // 假设您已经从会话中获取了当前用户的信息，比如用户的 ID
                    int currentUserId = GetCurrentUserId(); // 获取当前用户 ID 的方法，这里假设为 GetCurrentUserId()

                    var user = _context.TCustomers.FirstOrDefault(u => u.FCustomerId == currentUserId);
                    if (user != null)
                    {
                        // 將反斜線 \ 改為正斜線 /
                        user.FImagePath = Path.Combine("img/uploads", uniqueFileName).Replace("\\", "/");
                        _context.SaveChanges();
                    }

                    // 创建完整的图片 URL
                    string imageUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{user.FImagePath}";

                    return Ok(new { imageUrl });
                }
                else
                {
                    return BadRequest("No file uploaded.");
                }
            }
            catch (Exception ex)
            {
                // 记录异常信息
                Console.WriteLine("Exception occurred: " + ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private int GetCurrentUserId()
        {
            // 从 HttpContext 中获取用户的 ID
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 转换为整数并返回
            return int.Parse(userId);
        }




        public IActionResult GetPartial(int id)
        {
            switch (id)
            {
                case 1:
                    return PartialView("_UserPartial");
                case 2:
                    return PartialView("_OrderPartial");
                case 3:
                    return PartialView("_OrderPartial");
                case 4:
                    return PartialView("_CommentPartial");
                case 5:
                    return PartialView("_CollectionPartial");
                default:
                    return Content(""); // 处理其他情况
            }
        }
        [HttpGet]
        public IActionResult GetHotelOrder()
        {
            var hotelOrders = _context.THorders
                                .Where(h => h.FCustomerId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier) && h.FPaymentStatusId == 2)
                                .OrderBy(h => h.FHotelOrderId)
                                .Select(x => new
                                {
                                    x.FOrderDate,
                                    x.FHotelOrderId,
                                    x.FHotelId,
                                    x.FRoomTypeId,
                                    x.FRoomCount,
                                    x.FTotalPrice,
                                    x.FCheckInDate,
                                    x.FCheckOutDate,
                                    HotelName = _context.THotels
                                                    .Where(a => a.FHotelId == x.FHotelId)
                                                    .Select(a => a.FHotelName)
                                                    .FirstOrDefault(),
                                    RoomTypeName = _context.THroomTypes
                                                        .Where(a => a.FRoomTypeId == x.FRoomTypeId)
                                                        .Select(a => a.FRoomTypeName)
                                                        .FirstOrDefault(),
                                    BedType= _context.THroomTypes
                                                        .Where(a => a.FRoomTypeId == x.FRoomTypeId)
                                                        .Select(a => a.FBedType)
                                                        .FirstOrDefault(),
                                })
                                .ToList();

            var formattedOrders = hotelOrders.Select(order => new
            {
                HorderDate = order.FOrderDate,
                HotelName = order.HotelName,
                CheckInDate = $"Check in: {order.FCheckInDate?.ToString("yyyy/MM/dd")}<br>Check out: {order.FCheckOutDate?.ToString("yyyy/MM/dd")}",
                RoomType = order.RoomTypeName,
                RoomCount = $"{order.BedType}:{order.FRoomCount}",
                TotalAmount = (order.FTotalPrice ?? 0).ToString("C0")
        });

            return Json(formattedOrders);
        }
        [HttpGet]
        public IActionResult GetDestinationOrder()
        {
            var destinationOrders = _context.TdestinationOrders
                                .Where(h => h.FmemberId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier) && h.ForderState == true)
                                .OrderBy(h => h.ForderDate)
                                .Select(x => new
                                {
                                    x.ForderDate,
                                    x.Fqty,
                                    x.FdestinationId,
                                    x.Fprice,
                                    DestinationName = _context.Tdestinations
                                                    .Where(a => a.FdestinationId == x.FdestinationId)
                                                    .Select(a => a.FdestinationName)
                                                    .FirstOrDefault(),
                                })
                                .ToList();

            var DformattedOrders = destinationOrders.Select(order => new
            {
                DestinationDate = order.ForderDate?.ToString("yyyy/dd/MM"),
                DestinationName = order.DestinationName,
                DestinationQty = order.Fqty,
                DestinationPrice = (order.Fprice ?? 0).ToString("C0")
            });

            return Json(DformattedOrders);
        }

        [HttpGet]
        public IActionResult GetFreeOrder()
        {
            var freeOrders = _context.TfreeOrders
                                .Where(h => h.FmemberId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier) && h.ForderSate == true)
                                .OrderBy(h => h.ForderDate)
                                .Select(x => new
                                {
                                    x.ForderDate,
                                    x.Fqty,
                                    x.Fprice
                                    
                                })
                                .ToList();

            var FreeformattedOrders = freeOrders.Select(order => new
            {
                FreeDate = order.ForderDate?.ToString("yyyy/dd/MM"),
                FreeQty = order.Fqty,
                FreePrice = (order.Fprice ?? 0).ToString("C0")
            });

            return Json(FreeformattedOrders);
        }

        [HttpGet]
        public IActionResult GetVisaOrder()
        {
            var visaOrders = _context.VVorderViews
                    .Where(h => h.客戶名稱 == User.FindFirstValue(ClaimTypes.Name))
                    .ToList();

            var VisaformattedOrders = visaOrders.Select(order => new
            {
                Vorderdate = order.訂單日期,
                Vordername = order.商品名稱,
                Vordercount = order.數量,
                Vorderprice = (order.總價 ?? 0).ToString("C0"),
                Vorderstatus = order.訂單狀態
            });
            return Json(VisaformattedOrders);
        }

        #region Flight使用
        [HttpGet]
        public IActionResult GetFlightOrder()
        {
            var flightOrders = _context.TForders
                                .Where(f => f.FMemberId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier))
                                .OrderBy(f => f.FOrderDate)
                                .Select(x => new
                                {
                                    x.FId,
                                    x.FOrderDate,
                                    x.FOrderId,
                                    x.FOrderStatus,
                                    x.FPayment,
                                    x.FPaymentStatus,
                                    x.FTotal,
                                    x.FComment
                                })
                                .ToList();

            var formattedOrders = flightOrders.Select(order => new
            {
                fid = order.FId,
                orderDate = order.FOrderDate?.ToString("yyyy/MM/dd"),
                orderId = order.FOrderId,
                orderStatus = order.FOrderStatus,
                payment = order.FPayment,
                orderPaymentStatus = order.FPaymentStatus,
                total = (order.FTotal ?? 0).ToString("C0"),
                comment = order.FComment ?? ""
            });

            return Json(formattedOrders);
        }

        [HttpGet]
        public IActionResult getCommentPartial(int id)
        {
            var comment = _context.TForders
                .Where(c => c.FId == id)
                .Select(x => new
                {
                    orderId = x.FOrderId,
                    orderStatus = x.FOrderStatus,
                    paymentSatatus = x.FPaymentStatus,
                    comment = x.FComment,
                })
                .ToList();

            return Json(comment);
        }

        [HttpPost]
        public async Task<IActionResult> SaveComment([FromBody] CommentData data)
        {
            try
            {
                // 可以從 data 中獲取 id 和 comment
                var order = _context.TForders.Where(o => o.FId == data.Id).FirstOrDefault();
                if (order != null)
                {
                    // 如果找到了訂單，設置評論並保存到資料庫
                    order.FComment = data.Comment;
                    await _context.SaveChangesAsync();

                    // 返回成功的回應給前端
                    return Ok();
                }
                else
                {
                    // 如果找不到對應的訂單，返回相應的錯誤
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult getOrderDetailPartial(string orderid)
        {
            var orderdetail = _context.TForderDetails
                .Include(c=>c.FSchedule)
                .Where(c => c.FOrderId == orderid)
                .Select(x => new
                {
                    x.FSchedule.FFlightName,
                    x.FTicketType,
                    x.FPsgrName,
                    x.FNationalId,
                    x.FGender,
                    x.FBirth,
                    x.FPhone,
                    x.FEmail,
                    x.FPrice
                })
                .ToList();

            var formattedOrderdetail = orderdetail.Select(order => new
            {
                FlightName = order.FFlightName.ToString(),
                TicketType = order.FTicketType,
                NationalId = order.FNationalId,
                PsgrName = order.FPsgrName,
                Gender = (bool)(order.FGender) ? "男性" : "女性",
                Birth = order.FBirth?.ToString("yyyy/MM/dd"),
                Phone = order.FPhone,                
                Email= order.FEmail,
                Price = (order.FPrice ?? 0).ToString("C0")
            });

            return Json(formattedOrderdetail);
        }

        public class CommentData
        {
            public int Id { get; set; }
            public string Comment { get; set; }
        } 
        #endregion
    }


}
