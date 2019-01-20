using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alpha.Models.ViewModels
{
    public class OrderViewModels
    {
        public int RestoId { get; set; }
        public int OrderId { get; set; }
        public string Resto_Name { get; set; }
        public string Resto_Description { get; set; }

        public ICollection<ItemView> ListOfProposedItems { get; set; }
    }

    public class ItemView : Item
    {
        public ItemView()
        {

        }

        public ItemView(Item item)
        {
            this.ItemId = item.ItemId;
            this.Name = item.Name;
            this.IsAvailable = item.IsAvailable;
            this.UnitPrice = item.UnitPrice;
            this.Description = item.Description;
            this.TypeOfFood = item.TypeOfFood;


            this.HasSize = item.HasSize;
            this.AvailableSizes = item.AvailableSizes;
            this.CanBeSalt = item.CanBeSalt;
            this.CanBeHotNotCold = item.CanBeHotNotCold;
            this.CanHaveMeat = item.CanHaveMeat;
            this.CanHaveSauce = item.CanHaveSauce;

            this.Image = item.Image;

            this.MenuId = item.MenuId;
            this.Menu = item.Menu;

            this.OrderedItemList = item.OrderedItemList;
    }

        public int Quantity {get; set;}
    }
}