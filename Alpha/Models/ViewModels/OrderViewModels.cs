using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alpha.Models.ViewModels
{
    public class OrderViewModels
    {
        public string RestoId { get; set; }
        public string Resto_Name { get; set; }
        public string Resto_Description { get; set; }

        public ICollection<Item> ListOfProposedItems { get; set; }

    }
}