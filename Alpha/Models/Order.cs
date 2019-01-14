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
        public virtual ICollection<OrderedItem> OrderedItems { get; set; }
    }

    [Table("OrderedItems")]
    public class OrderedItem
    {
        public int Id { get; set; }

        //Link to the Item
        public virtual int ItemId { get; set; }
        public virtual Item Item { get; set; }

        // Link to the ORDER
        public virtual int CurrentOrderId { get; set; }
        public virtual Order CurrentOrder { get; set }


        string SelectedSize { get; set; }
        bool SelectedSalt { get; set; }
        bool SeelctedHotNotCold { get; set; }
        string SelectedMeat { get; set; }
        string SelectedSauce { get; set; }
        string SelectedCustom { get; set; }
    }
}