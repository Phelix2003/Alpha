using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Alpha.Models
{
    public class MenuViewModels
    {

    }

    public class CreateItemViewModel
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