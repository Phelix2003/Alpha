using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alpha.Utility;

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

        public bool CanSelectSalt { get; set; }
        public bool SelectedSalt { get; set; }
        public bool CanSelectHotCold { get; set; }
        public bool SelcedtHotNotCold { get; set; }
        public bool CanSelectSize { get; set; }
        public int? SelectedSizeId { get; set; }
        public List<SizedMeal> ListOfSizesView { get; set; }
        public bool CanSelectMeat { get; set; }
        public int? SelectedMeatId { get; set; }
        public List<Item> ListOfMeatsView {get; set;}
        public bool CanSlectSauce { get; set; }
        public int? SelectedSauceId { get; set; }
        public List<Item> ListofSauceView { get; set; }
    }

    public class PickASlotTimeView
    {
        public PickASlotTimeView()
        {
            ListOfPossibleTimes = new List<PossibleTime>();
        }

        /*
        public PickASlotTimeView(TimeSpan StartTime, TimeSpan StopTime)
        {
            //Creates a list of Times per 15 minutes steps. Starting from StartTime up to StopTime.
            for (TimeSpan i=StartTime; i < StopTime; i = i + TimeSpan.FromMinutes(15))
            {
                ListOfPossibleTimes.Add(new PossibleTime { Available = false, Id = i.Ticks.ToString(), TimeFrom = i });
            }
        }*/

        public void Add(OrderSlot SlotToAdd)
        {
            // The view should display the available slot times by step of 15 minutes. The slots should be grouped that way. 
            DateTime roundedTime = RoundDown(SlotToAdd.OrderSlotTime, TimeSpan.FromMinutes(15));
            if(ListOfPossibleTimes.FirstOrDefault(r => r.TimeFrom == roundedTime) == null)
            {
                ListOfPossibleTimes.Add(new PossibleTime
                {
                    TimeFrom = roundedTime,
                    TimeTo = roundedTime.AddMinutes(15),
                    Available = true,
                    Id = roundedTime.Ticks.ToString(),
                    SlotGroup = SlotToAdd.SlotGroup});
            }            
        }
        // Round dt to the nearest d velue DOWN
        private DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            return new DateTime((dt.Ticks) / d.Ticks * d.Ticks, dt.Kind);
        }

        public List<PossibleTime> ListOfPossibleTimes { get; set; }
        public int OrderId { get; set; }
    }

    public class PossibleTime
    {
        public string Id { get; set; }

        public int SlotGroup { get; set; }

        public DateTime TimeFrom { get; set; }
        public string TimeFromText()    {   return TimeFrom.ToString("T");  }
        public DateTime TimeTo { get; set; }
        public string TimeToText() { return TimeTo.ToString("T"); }
        public bool Available { get; set; }
    }
}