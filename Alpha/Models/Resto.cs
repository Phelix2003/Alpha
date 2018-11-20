using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Alpha.Models;

namespace Alpha.Models
{
    public class Resto
    {
        public Resto()
        {
            this.Chefs = new HashSet<ApplicationUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public virtual ICollection<ApplicationUser> Chefs { get; set; }
    }
}