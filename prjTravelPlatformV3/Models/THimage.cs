﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjTravelPlatformV3.Models;

public partial class THimage
{
    public int FHotelImgId { get; set; }

    public int? FHotelId { get; set; }

    public string FHotelImage { get; set; }

    public virtual THotel FHotel { get; set; }
}