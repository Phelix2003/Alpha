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

        [Display(Name = "Restaurant Address")]
        public string Address { get; set; }

        public string UserId { get; set; }
    }

    public class EditRestoViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Restaurant Name")]
        public string Name { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Restaurant Address")]
        public string Address { get; set; }

        public Menu menu {get; set; }


        public ICollection<ApplicationUser> AdministratorsList { get; set; }
        public ICollection<ApplicationUser> ChefsList { get; set; }


    }

    public class DeleteRestoViewModel
    {
        public int Id { get; set; }
    }

    public class AddChefToRestaurantViewModel
    {
        public String RestoName { get; set; }

        public List<SelectUserRestoViewModel> User { get; set; }
        public AddChefToRestaurantViewModel()
        {
            this.User = new List<SelectUserRestoViewModel>();
        }

        public IEnumerable<string> getSelectedIds()
        {
            return (from p in this.User where p.Selected select p.Id).ToList();
        }        
    }

    public class SelectUserRestoViewModel
    {
        public bool Selected { get; set; }
        public string Id { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; }

    }

    public class Prediction
    {
        public string description { get; set; }
        public string id { get; set; }
        public string place_id { get; set; }
        public string reference { get; set; }
        public List<string> types { get; set; }
    }
    public class RootObject
    {
        public List<Prediction> predictions { get; set; }
        public string status { get; set; }
    }
}