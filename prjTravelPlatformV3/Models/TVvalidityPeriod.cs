﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace prjTravelPlatformV3.Models;

public partial class TVvalidityPeriod
{
    public int FId { get; set; }

    public string FValidityPeriod { get; set; }

    public virtual ICollection<TVproduct> TVproducts { get; set; } = new List<TVproduct>();
}