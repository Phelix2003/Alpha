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

        public decimal? ConfiguredPrice()
        {
            decimal? Price = null;

            if(Item != null)
            {
                if(Item.HasSize)
                {
                    // Select the price for the configured size
                    Price = Item.AvailableSizes.FirstOrDefault(r => r.MealSize == SelectedSize).Price;
                }
                else
                {
                    Price = Item.UnitPrice;
                }
                if (SelectedSauce != null && Item.TypeOfFood != TypeOfFood.Meal && Item.TypeOfFood != TypeOfFood.Menu)
                {
                    //note for "Meal" and "Menu" the price au sauce is embbedded in the initial price. Should not be added here
                    Price = Price + SelectedSauce.UnitPrice;
                }

                if (SelectedMeat != null)
                {
                    Price = Price + SelectedMeat.UnitPrice;
                }

            }
            return Price;
        }


        public int Id { get; set; }

        //Link to the Item (Main Item)
        public virtual int ItemId { get; set; }
        public virtual Item Item { get; set; }

        // Link to the ORDER
        public virtual int CurrentOrderId { get; set; }
        public virtual Order CurrentOrder { get; set; }

        // Configuration of the Item 
        public int Quantity { get; set; }
        public MealSize? SelectedSize { get; set; }
        public bool SelectedSalt { get; set; }
        public bool SelectedHotNotCold { get; set; }

        // The relation with meat and sauce is kept in order to adda analytics functions later. 
        //link to Item (Additional / optional Item)
        public virtual int? SelectedMeatId { get; set; }
        public virtual Item SelectedMeat { get; set; }

        //link to Item (Additional / optional Item
        public virtual int? SelectedSauceId { get; set; }
        public virtual Item SelectedSauce { get; set; }
    }
}