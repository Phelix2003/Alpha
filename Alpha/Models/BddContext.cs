﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Alpha.Models
{
    public class BddContext : DbContext
    {
        public DbSet<Resto> Restos { set; get; }
    }
}