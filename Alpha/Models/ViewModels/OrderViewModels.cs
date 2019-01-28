﻿using System;
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
        public int Quantity { get; set; }
    }

    public class OrderedItemView
    {
        public static OrderedItemView Current
        {
            get
            {
                var viewmodel = HttpContext.Current.Session["OrderedItemView"] as OrderedItemView;
                if (null == viewmodel)
                {
                    viewmodel = new OrderedItemView();
                    HttpContext.Current.Session["OrderedItemView"] = viewmodel;
                }
                return viewmodel;
            }
        }

        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int OrderId { get; set; }
        public TypeOfFood TypeOfFood { get; set; }

        public MealSize SelectedMealSize { get; set; }
        public bool CanSelectSalt { get; set; }
        public bool SelectedSalt { get; set; }
        public bool CanSelectHotCold { get; set; }
        public bool SelcedtHotNotCold { get; set; }
        public bool CanSelectSize { get; set; }
        public int? SelectedSize { get; set; }
        public IEnumerable<MealSize> ListOfSizesView { get; set; }
        public bool CanSelectMeat { get; set; }
        public int? SelectedMeatId { get; set; }
        public IEnumerable<Item> ListOfMeatsView {get; set;}
        public bool CanSlectSauce { get; set; }
        public int? SelectedSauceId { get; set; }
        public IEnumerable<Item> ListofSauceView { get; set; }
    }
}