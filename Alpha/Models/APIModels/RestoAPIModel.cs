using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alpha.Models.APIModels;

namespace Alpha.Models.APIModels
{

    public class RestoAPIModel
    {
        public ResponseHeaderAPIModel ResponseHeader { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Description { get; set; } // TODO ajouter l'édition du champ dans les vues 
        public string Address { get; set; }   //TODO Ajouter adresse détaillées 
        public byte[] Image { get; set; } // TODO ajouter dans l

        //public virtual ICollection<OpenTimePeriod> OpeningTimes { get; set; }
        //public virtual Menu Menu { get; set; }
    }



}