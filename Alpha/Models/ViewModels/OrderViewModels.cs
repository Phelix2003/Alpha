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

        public ICollection<Item> ListOfProposedItems { get; set; }
    }
}