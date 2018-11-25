using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Alpha.Models
{
    public class RestaurantViewModels
    {

    }

    public class CreateViewModel
    {
        [Required(ErrorMessage = "Enter a valide restaurant name")]
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; }

        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Please enter a valid phone number")]
        [Display(Name ="Phone Number")]
        public string PhoneNumber { get; set; }

        public string UserId { get; set; }
    }

    public class EditRestoViewModel
    {
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }


        public ICollection<ApplicationUser> AdministratorsList { get; set; }
        public ICollection<ApplicationUser> ChefsList { get; set; }


    }
}