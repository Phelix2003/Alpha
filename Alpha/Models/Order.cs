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

        public DateTime OrderOpenTime { get; set; }
        public DateTime? OrderClosedTime { get; set; }
        public bool IsOrderCompleted { get; set; }
        public bool IsInPRogress { get; set; }

        public virtual ApplicationUser OrderOwner { get; set; }
        public virtual Resto Resto { get; set; }

        //A vérifier si dans une collection on peut ajouter plusieurs fois le même élément... 
        public virtual ICollection<Item> OrderItemList { get; set; }
    }
}