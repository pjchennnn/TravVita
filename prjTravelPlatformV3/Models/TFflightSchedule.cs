﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjTravelPlatformV3.Models;

public partial class TFflightSchedule
{
    public int FScheduleId { get; set; }

    public int? FAirlineId { get; set; }

    public string FFlightName { get; set; }

    public DateTime? FDepartureTime { get; set; }

    public DateTime? FArrivalTime { get; set; }

    public int? FDepartureId { get; set; }

    public int? FDestinationId { get; set; }

    public decimal? FTicketPrice { get; set; }

    public int? FClassId { get; set; }

    public int? FQty { get; set; }

    public virtual TCcompanyInfo FAirline { get; set; }

    public virtual TFclass FClass { get; set; }

    public virtual TFairportInfo FDeparture { get; set; }

    public virtual TFairportInfo FDestination { get; set; }

    public virtual ICollection<TForderDetail> TForderDetails { get; set; } = new List<TForderDetail>();
}