﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjTravelPlatformV3.Models;

public partial class THcollection
{
    public int FCustomerId { get; set; }

    public int FHotelId { get; set; }

    public int FSid { get; set; }

    public virtual TCustomer FCustomer { get; set; }

    public virtual THotel FHotel { get; set; }
}