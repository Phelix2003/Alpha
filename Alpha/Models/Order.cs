using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using Alpha.Models;

namespace Alpha.Models
{
    [Table("Orders")]
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderTime { get; set; }
        public bool IsOrderCompleted { get; set; }

        public virtual ApplicationUser OrderOwner { get; set; }
        public virtual Resto Resto { get; set; }
        public virtual ICollection<Item> OrderList { get; set; }
    }
}