using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using prjTravelPlatform_release.Areas.Customer.ViewModel.Airline;
using prjTravelPlatform_release.Models;
using prjTravelPlatformV3.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Text.Json;
using static NuGet.Packaging.PackagingConstants;
using System.Security.Claims;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Identity.UI.Services;
using Azure.Core;
using System.Net.Mail;
using System.Net;


namespace prjTravelPlatform_release.Areas.Customer.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightApiController : ControllerBase
    {

        private readonly dbTravalPlatformContext _context;
        private readonly IEmailSender _emailSender;
        public FlightApiController(dbTravalPlatformContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }


        //路徑：~/api/flightapi/airports
        //調出機場選項
        [HttpGet]
        [Route("Airports")]
        public IActionResult Airports()
        {
            var airports = _context.TFairportInfos.Select(a => a.FAirport).Distinct();
            return new JsonResult(airports);
        }

        //路徑：~/api/flightapi/GetFlights
        //調出單程航班資訊
        [HttpGet]
        [Route("GetFlights")]
        public async Task<ActionResult<IEnumerable<FlightSearchDTO>>> GetFlights()
        {
            var flights = await _context.TFflightSchedules
                .Select(f => new FlightSearchDTO
                {
                    fId = Convert.ToInt32(f.FScheduleId),
                    AirlineId = Convert.ToInt32(f.FAirlineId),
                    Departure = f.FDeparture.FAirport,
                    Destination = f.FDestination.FAirport,
                    DepartureTime = Convert.ToDateTime(f.FDepartureTime),
                    ArrivalTime = Convert.ToDateTime(f.FArrivalTime),
                    // 計算飛行時間
                    Duration = f.FArrivalTime != null && f.FDepartureTime != null ? (int)(f.FArrivalTime - f.FDepartureTime).Value.TotalMinutes : 0,
                    TicketPrice = f.FTicketPrice
                })
                .ToListAsync();

            return new JsonResult(flights);
        }

        //顯示搜尋結果
        [HttpGet]
        [Route("FlightsSearch")]
        public async Task<ActionResult> FlightsSearch(string departure, string destination, string departureDate, string returnDate, string Adult, string Children, string Baby)
        {
            try
            {
                DateTime date1 = Convert.ToDateTime(departureDate).Date;
                DateTime date2 = Convert.ToDateTime(returnDate).Date;
                var TicketQty = Convert.ToInt32(Adult) + Convert.ToInt32(Children);
                var outgoingFlights = await _context.TFflightSchedules
                    .Where(f => f.FDeparture.FAirport == departure &&
                                f.FDestination.FAirport == destination &&
                                f.FDepartureTime.Value.Date == date1 &&
                                f.FQty.Value >= TicketQty)
                    .Select(f => new
                    {
                        f.FScheduleId, //航班Id
                        f.FAirlineId,  //航空公司Id
                        f.FAirline.FCompanyName, //航空公司
                        f.FFlightName,　//航班代碼
                        DepartureTime = f.FDepartureTime.Value.ToString("yyyy/MM/dd tt HH:mm"), //起飛時間
                        DepartureAirport = f.FDeparture.FAirport,  //起飛地
                        ArrivalTime = f.FArrivalTime.Value.ToString("yyyy/MM/dd tt HH:mm"),  //降落時間
                        DestinationAirport = f.FDestination.FAirport, //降落地
                        f.FTicketPrice, //票價
                        f.FClass.FClass,  //艙等
                        f.FQty, //數量
                        //飛行時間
                        Duration = f.FArrivalTime != null && f.FDepartureTime != null ? (int)(f.FArrivalTime - f.FDepartureTime).Value.TotalMinutes : 0,
                    })
                    .ToListAsync();

                var returnFlights = await _context.TFflightSchedules
                    .Where(f => f.FDeparture.FAirport == destination &&
                                f.FDestination.FAirport == departure &&
                                f.FDepartureTime.Value.Date == date2 &&
                                f.FDepartureTime.Value.Date > date1 &&
                                f.FQty.Value >= TicketQty)
                    .Select(f => new
                    {
                        f.FScheduleId, //航班Id
                        f.FAirlineId,  //航空公司Id
                        f.FAirline.FCompanyName, //航空公司
                        f.FFlightName,　//航班代碼
                        DepartureTime = f.FDepartureTime.Value.ToString("yyyy/MM/dd tt HH:mm"), //起飛時間
                        DepartureAirport = f.FDeparture.FAirport,  //起飛地
                        ArrivalTime = f.FArrivalTime.Value.ToString("yyyy/MM/dd tt HH:mm"),  //降落時間
                        DestinationAirport = f.FDestination.FAirport, //降落地
                        f.FTicketPrice, //票價
                        f.FClass.FClass,  //艙等
                        f.FQty, //數量
                        //飛行時間
                        Duration = f.FArrivalTime != null && f.FDepartureTime != null ? (int)(f.FArrivalTime - f.FDepartureTime).Value.TotalMinutes : 0,

                    })
                    .ToListAsync();

                var result = new
                {
                    OutgoingFlights = outgoingFlights,
                    ReturnFlights = returnFlights
                };

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        //訂單頁面秀出航班資訊
        [HttpGet]
        [Route("SelectedFlights")]
        public async Task<ActionResult> SelectedFlights(string outgoingScheduleId, string returnScheduleId)
        {
            try
            {
                var outgoingFlight = _context.TFflightSchedules
                    .Where(f => f.FScheduleId == Convert.ToInt32(outgoingScheduleId))
                    .Select(f => new
                    {
                        f.FScheduleId, //航班Id
                        f.FAirlineId,  //航空公司Id
                        f.FAirline.FCompanyName, //航空公司
                        f.FFlightName,　//航班代碼
                        DepartureTime = f.FDepartureTime.Value.ToString("yyyy/MM/dd tt HH:mm"), //起飛時間
                        DepartureAirport = f.FDeparture.FAirport,  //起飛地
                        ArrivalTime = f.FArrivalTime.Value.ToString("yyyy/MM/dd tt HH:mm"),  //降落時間
                        DestinationAirport = f.FDestination.FAirport, //降落地
                        f.FTicketPrice, //票價
                        f.FClass.FClass,  //艙等
                        f.FQty, //數量
                        //飛行時間
                        Duration = f.FArrivalTime != null && f.FDepartureTime != null ? (int)(f.FArrivalTime - f.FDepartureTime).Value.TotalMinutes : 0,
                    })
                    .ToList();

                var returnFlight = _context.TFflightSchedules
                    .Where(f => f.FScheduleId == Convert.ToInt32(returnScheduleId))
                    .Select(f => new
                    {
                        f.FScheduleId, //航班Id
                        f.FAirlineId,  //航空公司Id
                        f.FAirline.FCompanyName, //航空公司
                        f.FFlightName,　//航班代碼
                        DepartureTime = f.FDepartureTime.Value.ToString("yyyy/MM/dd tt HH:mm"), //起飛時間
                        DepartureAirport = f.FDeparture.FAirport,  //起飛地
                        ArrivalTime = f.FArrivalTime.Value.ToString("yyyy/MM/dd tt HH:mm"),  //降落時間
                        DestinationAirport = f.FDestination.FAirport, //降落地
                        f.FTicketPrice, //票價
                        f.FClass.FClass,  //艙等
                        f.FQty, //數量
                        //飛行時間
                        Duration = f.FArrivalTime != null && f.FDepartureTime != null ? (int)(f.FArrivalTime - f.FDepartureTime).Value.TotalMinutes : 0,
                    })
                    .ToList();

                var result = new
                {
                    OutgoingFlights = outgoingFlight,
                    ReturnFlights = returnFlight
                };
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }



        //路徑：~/api/flightapi/RoundTripOrder
        //成立訂單
        [HttpPost]
        [Route("RoundTripOrder")]
        public async Task<ActionResult> RoundTripOrder([FromBody] OrderViewModel model)
        {
            //使用transaction
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    #region 新增訂單
                    // 新建訂單
                    var order = new TForder
                    {
                        FMemberId = model.CustomerId,
                        FOrderDate = DateTime.Now.ToLocalTime(),
                        FOrderStatusId = 2, //已付款
                        FPaymentStatusId = 2, //已付款
                        FPaymentId = 1, //信用卡
                        FTotal = 0  //總額
                    };

                    _context.TForders.Add(order);
                    await _context.SaveChangesAsync();

                    // 從 order 物件中取得訂單編號
                    int orderId = order.FId;
                    #endregion

                    decimal totalAmount = 0; // 訂單總額 


                    // 建立訂單細節
                    foreach (var passenger in model.Passengers)
                    {
                        #region 處理前端資料
                        int ticketTypeId;
                        if (passenger.Type == "adult")
                        {
                            ticketTypeId = 1;
                        }
                        else if (passenger.Type == "baby")
                        {
                            ticketTypeId = 2;
                        }
                        else if (passenger.Type == "children")
                        {
                            ticketTypeId = 3;
                        }
                        else
                        {
                            ticketTypeId = 1;
                        }

                        // 取得ticketRate
                        var ticketType = await _context.TFticketTypes.FindAsync(ticketTypeId);
                        if (ticketType == null)
                        {
                            continue;
                        }

                        // 去程票價
                        var outgoingFlightSchedule = await _context.TFflightSchedules.FindAsync(model.OutgoingScheduleId);
                        if (outgoingFlightSchedule == null)
                        {
                            continue;
                        }
                        // 回程票價
                        var returnFlightSchedule = await _context.TFflightSchedules.FindAsync(model.ReturnScheduleId);
                        if (returnFlightSchedule == null)
                        {
                            continue;
                        }

                        // 計算票價
                        decimal outgoingPrice = (decimal)ticketType.FTicketRate * (decimal)outgoingFlightSchedule.FTicketPrice;
                        decimal returnPrice = (decimal)ticketType.FTicketRate * (decimal)returnFlightSchedule.FTicketPrice;

                        #endregion

                        #region 新增來回訂單細節
                        // 新建去程訂單細節
                        var outgoingOrderDetail = new TForderDetail
                        {
                            FOrderId = order.FOrderId,
                            FScheduleId = model.OutgoingScheduleId,
                            FTicketTypeId = ticketTypeId,
                            FPrice = outgoingPrice,
                            FPsgrName = passenger.LastName + passenger.FirstName,
                            FNationalId = passenger.NationalId,
                            FGender = passenger.Gender,
                            FBirth = passenger.Birth,
                            FEmail = passenger.Email,
                            FPhone = passenger.Phone,
                        };
                        // 新建回程訂單細節
                        var returnOrderDetail = new TForderDetail
                        {
                            FOrderId = order.FOrderId,
                            FScheduleId = model.ReturnScheduleId,
                            FTicketTypeId = ticketTypeId,
                            FPrice = returnPrice,
                            FPsgrName = passenger.LastName + passenger.FirstName,
                            FNationalId = passenger.NationalId,
                            FGender = passenger.Gender,
                            FBirth = passenger.Birth,
                            FEmail = passenger.Email,
                            FPhone = passenger.Phone,
                        };

                        _context.TForderDetails.Add(outgoingOrderDetail);
                        _context.TForderDetails.Add(returnOrderDetail); 
                        #endregion

                        // 更新訂單總金額
                        totalAmount += outgoingPrice + returnPrice;
                    }

                    // 總金額存回資料庫
                    order.FTotal = totalAmount;
                    await _context.SaveChangesAsync();

                    // 訂單成立後扣除航班票數的庫存
                    int passengerCount = model.Passengers.Count;

                    // 去程回程的航班ID
                    var outgoingFlightScheduleId = await _context.TFflightSchedules.FindAsync(model.OutgoingScheduleId);
                    var returnFlightScheduleId = await _context.TFflightSchedules.FindAsync(model.ReturnScheduleId);

                    // 航班的票數扣除乘客數量
                    if (outgoingFlightScheduleId != null && returnFlightScheduleId != null)
                    {
                        outgoingFlightScheduleId.FQty -= passengerCount;
                        returnFlightScheduleId.FQty -= passengerCount;

                        await _context.SaveChangesAsync();
                    }

                    // 提交Commit
                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    #region 寄送訂單確認信

                    //重新從資料庫提取一次建立的order跟去回程(設定同時回傳的關聯屬性)
                    var orderInfo = _context.TForders.Where(o => o.FOrderId == order.FOrderId).Include(o => o.FMember).FirstOrDefault();
                    var outgoingFlightScheduleInfo = _context.TFflightSchedules.Where(o => o.FScheduleId == model.OutgoingScheduleId).Include(o=>o.FDeparture).Include(r=>r.FDestination).FirstOrDefault();
                    var returnFlightScheduleInfo = _context.TFflightSchedules.Where(r => r.FScheduleId == model.ReturnScheduleId).FirstOrDefault();
                    Console.WriteLine(orderInfo.FMember.FName);
                    //處理顯示參數
                    string phoneForEmail = orderInfo.FMember.FPhone;
                    string totalForEmail = ((int)orderInfo.FTotal).ToString("0");

                    // 客戶信箱
                    string customerEmail = model.CustomerEmail;

                    // 設置郵件內容
                    string subject = $"【TravVita】訂單號 {orderInfo.FOrderId} 已成立!";  // 主旨
                    string message = $"<p><span style=\"font-size:14px\">親愛的會員，您的訂單號<span style=\"color:#3498db\"> <strong>{orderInfo.FOrderId}</strong></span>已成立！</span></p>\r\n"; // 內文
                    message += $"<p><span style=\"font-size:14px\">行程如下，請確認：</span></p><hr />";
                    message += $"<h4><strong>【訂購人資料】</strong></h4>\r\n<p>訂購會員姓名：{orderInfo.FMember.FName}</p>\r\n<p>訂單日期：{orderInfo.FOrderDate}</p>\r\n<p>電話：{phoneForEmail}</p>\r\n<p>總金額：{totalForEmail}</p>";
                    message += $"<h4><strong>【行程資料-來回】</strong></h4>";
                    message += $"<p>{outgoingFlightScheduleInfo.FDeparture.FAirport}　直達　{outgoingFlightScheduleInfo.FDestination.FAirport}</p>";
                    message += $"<p>去程</p>\r\n<p>起飛時間：{outgoingFlightScheduleInfo.FDepartureTime}</p>\r\n<p>降落時間：{outgoingFlightScheduleInfo.FArrivalTime}</p>";
                    message += $"<p>回程</p>\r\n<p>起飛時間：{returnFlightScheduleInfo.FDepartureTime} </p>\r\n<p>降落時間：{returnFlightScheduleInfo.FArrivalTime}</p>\r\n";
                    message += $"<hr /><p>感謝您使用TravVita服務，有任何問題歡迎隨時與我們聯繫。</p>";
                    // 使用郵件發送者來發送郵件
                    await _emailSender.SendEmailAsync(customerEmail, subject, message); 
                    #endregion

                    //回傳訂單編號
                    return Ok(new { OrderId = orderId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, "訂單成立失敗，請重新再試");
                }
            }
        }

        //路徑：~/api/flightapi/Confirm
        //訂單確認畫面資訊
        [HttpGet]
        [Route("Confirm")]
        public async Task<ActionResult> Confirm(string outgoingScheduleId, string returnScheduleId, string orderId)
        {
            try
            {
                #region 變數處理
                int OutgoingScheduleId = Convert.ToInt32(outgoingScheduleId);
                int ReturnScheduleId = Convert.ToInt32(returnScheduleId);
                int OrderId = Convert.ToInt32(orderId);
                #endregion

                #region 訂單資訊處理
                // 根據 orderId 查詢相關訂單信息
                var order = await _context.TForders
                    .Where(f => f.FId == OrderId)
                    .Select(o => new
                    {
                        o.FId,
                        o.FOrderId,
                        o.FOrderDate,
                        o.FMember.FName, //會員姓名
                        o.FMember.FPhone, //會員電話
                        o.FMember.FEmail, //會員email
                        o.FTotal, //總金額
                        o.FPayment.FPayment, //付款方式
                        o.TForderDetails, //細節
                    })
                    .FirstOrDefaultAsync();

                // 檢查訂單是否存在
                if (order == null)
                {
                    return NotFound();
                } 
                #endregion

                #region 去程資料處理
                // 根據 outgoingScheduleId 取得去程航班
                var outgoingFlightDetail = _context.TFflightSchedules
                    .Where(d => d.FScheduleId == OutgoingScheduleId)
                    .Select(d => new
                    {
                        d.FAirline,  //航空公司
                        d.FFlightName,//航班代碼
                        d.FDepartureTime,
                        d.FArrivalTime,
                        d.FDeparture,
                        d.FDestination,
                    })
                    .FirstOrDefault();

                var outgoingPsgerDetail = order.TForderDetails
                    .Where(d => d.FScheduleId == OutgoingScheduleId)
                    .Select(d => new
                    {
                        d.FPsgrName,
                        d.FGender,
                    });

                #endregion

                #region 回程航班資料處理
                // 根據 returnScheduleId 取得回程航班
                var returnFlightDetail = _context.TFflightSchedules
                    .Where(d => d.FScheduleId == ReturnScheduleId)
                    .Select(d => new
                    {
                        d.FAirline,  //航空公司
                        d.FFlightName,//航班代碼
                        d.FDepartureTime,
                        d.FArrivalTime,
                        d.FDeparture,
                        d.FDestination,
                    })
                    .FirstOrDefault();

                #endregion

                // 構建回傳的 JSON 對象
                var response = new
                {
                    Order = order,
                    OutgoingFlightDetail = outgoingFlightDetail,
                    OutgoingPsgerDetail = outgoingPsgerDetail,  //乘客資料
                    ReturnFlightDetail = returnFlightDetail,
                };

                // 返回 JSON
                return Ok(response);
            }
            catch (Exception ex)
            {
                // 處理異常
                return StatusCode(500, "Internal server error");
            }
        }


    }
}

