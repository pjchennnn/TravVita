﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjTravelPlatformV3.Models;

public partial class TItype
{
    public int FTypeId { get; set; }

    public string FType { get; set; }

    public virtual ICollection<TIproduct> TIproducts { get; set; } = new List<TIproduct>();
}