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
        public OrderedItem() {}

        public bool Compare(OrderedItem ToCompare)
        {
            if (Item.ItemId == ToCompare.ItemId &&
                SelectedSauce == ToCompare.SelectedSauce &&
                SelectedSalt == ToCompare.SelectedSalt &&
                SelectedMeat == ToCompare.SelectedMeat &&
                SelectedSize == ToCompare.SelectedSize)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public int Id { get; set; }

        //Link to the Item
        public virtual int ItemId { get; set; }
        public virtual Item Item { get; set; }

        // Link to the ORDER
        public virtual int CurrentOrderId { get; set; }
        public virtual Order CurrentOrder { get; set; }

        public int Quantity { get; set; }
        public MealSize? SelectedSize { get; set; }
        public bool SelectedSalt { get; set; }
        public bool SelectedHotNotCold { get; set; }
        public string SelectedMeat { get; set; }
        public string SelectedSauce { get; set; }
        public string SelectedCustom { get; set; }
    }
}