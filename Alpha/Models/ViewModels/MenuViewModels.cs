using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Alpha.Models
{
    public class MenuViewModel
    {
        public int MenuId { get; set; }
        [Required(ErrorMessage = "Introduisez un num de menu valide")]
        [Display(Name = "Nom du menu")]
        public string Name { get; set; }

        [Display(Name = "Date de denière modification du Menu")]
        public DateTime? DateOfModification { get; set; }

        public string restoName { get; set; }
        public int restoId { get; set; }

        public virtual Resto resto { get; set; }

        public virtual ICollection<Item> ItemList { get; set; }
    }

    public class CreateItemViewModel
    {
        public int ItemId { get; set; }
        [Required(ErrorMessage = "Enter a valide restaurant name")]
        [Display(Name = "Article Name")]
        public string Name { get; set; }

        
        [Display(Name = "Article Description")]
        public string Description { get; set; }

        [Display(Name = "Should this article be available?")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Article price")]
        [Required(ErrorMessage = "Expenses is required.")]
        [RegularExpression(@"^[0-9]+(\.[0-9]{1,2})$", ErrorMessage = "Valid Decimal number with maximum 2 decimal places.")]
        public string UnitPrice { get; set; }

        public int MenuId { get; set; }

        [Display(Name = "Article image")]
        public HttpPostedFileBase Image { get; set; }
    }

    public class EditItemViewModel
    {
        public int ItemId { get; set; }
        [Required(ErrorMessage = "Enter a valide restaurant name")]
        [Display(Name = "Article Name")]
        public string Name { get; set; }
        [Display(Name = "Should this article be available?")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Article price")]
        [Required(ErrorMessage = "Expenses is required.")]
        [RegularExpression(@"^[0-9]+(\.[0-9]{1,2})$", ErrorMessage = "Valid Decimal number with maximum 2 decimal places.")]
        public string UnitPrice { get; set; }

        [Display(Name = "Article image")]
        public HttpPostedFileBase Image { get; set; }
    }
}