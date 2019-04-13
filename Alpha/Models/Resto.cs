using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using Alpha.Models;

namespace Alpha.Models
{
    [Table("Restos")]
    public class Resto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; } // TODO ajouter l'édition du champ dans les vues 
        public string Address { get; set; }   //TODO Ajouter adresse détaillées 
        public string Address_Street { get; set; }
        public string Address_City { get; set; }
        public string Address_ZIPCode { get; set; }

        public byte[] Image { get; set; }

        public string Shop_VatId { get; set; }
        public string Shop_Email { get; set; }
        //public CountryCode CountryCode { gets; set; } // TODO for payement outside of Belgium 

        public string Owner_FirstName { get; set; }
        public string Owner_LastName { get; set; }
        public string Owner_Address { get; set; }
        public string Owner_City { get; set; }
        public string Owner_ZipCode { get; set; }
        public DateTime? Owner_DateOfBirth { get; set; }
        public string Owner_PersonalIDNumber { get; set; }
        public string Owner_PassportNumber { get; set; }
        public string Owner_email { get; set; }

        public string Payment_BlueSnapPayoutVendorId { get; set; }
        public string Payment_BankName { get; set; }
        public string Payment_BankAdress { get; set; }
        public string Payment_BankCity { get; set; }
        public string Payment_BankZip { get; set; }
        public string Payment_NameOnAccount { get; set; }
        public string Payment_BankId { get; set; }
        public string Payment_BankAccountId { get; set; }
        public string Payment_IBAN { get; set; }
        public string Payment_SwiftBIC { get; set; }
        public int Payment_commissionPercent { get; set; }
        public int Payment_minimalPayoutAmount { get; set; }

        public bool VendoreSuccesfulOnboarding { get; set; }
        public bool RestaurantAvailable { get; set; }

        public virtual ICollection<OpenTimePeriod> OpeningTimes {get; set;}

        public virtual ICollection<ApplicationUser> Administrators { get; set; }
        public virtual ICollection<ApplicationUser> Chefs { get; set; }

        public virtual ICollection<OrderSlot> OrderIntakeSlots { get; set; }

        public virtual Menu Menu {get; set;}
    }

    [Table("RestosOrderSlots")]
    public class OrderSlot
    {
        public int OrderSlotId { get; set; }

        // TODO DateTime is no recommended for use on server. Tobe replaced by UTCDateTime
        public DateTime OrderSlotTime { get; set; }

        // To group the slot by openning time
        public MealTime SlotGroup { get; set; }

        // Restaurant associated to this slot time
        //public int RestoId { get; set; }
        public virtual Resto Resto { get; set; }

        // Order associated to this slot time
        //public int? OrderId { get; set; }
        public virtual Order Order { get; set; }
    }

    [Table("Menu")]
    public class Menu
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfModification { get; set; }

        public virtual Resto resto { get; set; }

        public virtual ICollection<Item> ItemList { get; set; }

    }

    [Table("MenuItems")]
    public class Item
    {
        public Item()
        {
            DeletedOn = null;
        }

        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? DeletedOn { get; set; }

        public decimal UnitPrice { get; set; }
        public string Description { get; set; }
        public TypeOfFood TypeOfFood { get; set; }
        public string Brand { get; set; }
        // public Brand Brand {get; set; } // TODO to be implemented with link to supplier table


        public bool HasSize { get; set; }
        public virtual List<SizedMeal> AvailableSizes { get; set; }
        public bool CanBeSalt { get; set; }
        public bool CanBeHotNotCold { get; set; }
        public bool CanHaveMeat { get; set; }
        public bool CanHaveSauce { get; set; }



        public byte[] Image { get; set; }
        //To do: add drag and drop feature on the front end --> https://www.dropzonejs.com/

        //one to one relation by convention
        public int MenuId { get; set; }

        public virtual Menu Menu { get; set; }

        // Relation with OrderedItems
        public virtual ICollection<OrderedItem> OrderedItemList { get; set; }
        public virtual ICollection<OrderedItem> OrderedItemSauceList { get; set; }
        public virtual ICollection<OrderedItem> OrderedItemMeatList { get; set; }
    }


    [Table("MenuSizedMeal")]
    public class SizedMeal
    {
        
        public int Id { get; set; }
        public MealSize MealSize { get; set; }
        public decimal Price { get; set; }

        public int ItemId { get; set; }
        public virtual Item Item { get; set; }
    }

    public enum TypeOfFood
    {
        Frites = 0,
        Sauce = 1,
        Snack = 2,
        Meal = 3,
        Menu = 4,
        Boisson = 5
    }

    public enum MealSize
    {
        S = 0,
        M = 1,
        L = 2,
        XL = 3,
        XXL = 4
    }

    public enum MealTime
    {
        Breakfast = 0,
        Lunch = 1,
        Diner = 2        
    }


    [Table("RestoOpenTimePeriods")]
    public class OpenTimePeriod
    {
        public List<TimeSpan> GetListOfOrderSlots()
        {
            List<TimeSpan> TimeSpanList = new List<TimeSpan>();
            if (NbAuthorizedOrderPerHour > 0)
            {
                TimeSpan deltaTime = new TimeSpan(TimeSpan.FromMinutes(60).Ticks / NbAuthorizedOrderPerHour);
                TimeSpan lastTimeSpan = OpenTime;
                TimeSpanList.Add(OpenTime);
                do
                {
                    lastTimeSpan = lastTimeSpan + deltaTime;
                    TimeSpanList.Add(lastTimeSpan);
                } while (lastTimeSpan <= CloseTime);
                return TimeSpanList;
            }
            else
            {
                return null;
            }   
        }

        public int OpenTimePeriodId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public int NbAuthorizedOrderPerHour { get; set; }




        public int RestoId { get; set; }
        public virtual Resto Resto { get; set; }
    }
}