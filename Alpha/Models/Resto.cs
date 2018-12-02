using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using Alpha.Models;

namespace Alpha.Models
{

    public class Resto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        // Ajouter adresse


        public virtual ICollection<ApplicationUser> Administrators { get; set; }
        public virtual ICollection<ApplicationUser> Chefs { get; set; }

        public virtual Menu Menu {get; set;}
    }

    public class Menu
    {        
        public int MenuId { get; set; }
        public DateTime? DateOfModification { get; set; }


        public virtual ICollection<Item> ItemList { get; set; }
    }

    public class Item
    {        
        public int ItemId { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }
        public double UnitPrice { get; set; } 

        //Photos to be add
    }


}