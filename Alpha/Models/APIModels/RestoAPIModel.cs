using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alpha.Models.APIModels;

namespace Alpha.Models.APIModels
{
    public class ListRestoAPIModel
    {
        public ResponseHeaderAPIModel ResponseHeader { get; set; }
        public List<RestoAPIModel> Restos { get; set; }
    }

    public class RestoAPIModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; } // TODO ajouter l'édition du champ dans les vues 
        public string Address { get; set; }   //TODO Ajouter adresse détaillées 
        public byte[] Image { get; set; } // TODO ajouter dans l

        //public virtual ICollection<OpenTimePeriod> OpeningTimes { get; set; }
        public MenuAPIModel Menu { get; set; }
    }

    public class MenuAPIModel
    {
        public int MenuId { get; set; }
        public string Name { get; set; }

        public ICollection<ItemAPIModel> ItemList { get; set; }
    }

    public class ItemAPIModel
    {

        public int ItemId { get; set; }
        public string Name { get; set; }

        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }

        public TypeOfFood TypeOfFood { get; set; }
        public bool HasSize { get; set; }
        public virtual List<SizedMeal> AvailableSizes { get; set; }
        public bool CanBeSalt { get; set; }
        public bool CanBeHotNotCold { get; set; }
        public bool CanHaveMeat { get; set; }
        public bool CanHaveSauce { get; set; }

        public byte[] Image { get; set; }
    }


    public class OrderSlotAPI
    {
   

        public int OrderSlotId { get; set; }

        public DateTime OrderSlotTime { get; set; }

        // To group the slot by openning time
        public MealTime SlotGroup { get; set; }
    }

}