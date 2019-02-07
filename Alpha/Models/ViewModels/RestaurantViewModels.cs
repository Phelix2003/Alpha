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

        public ICollection<OpenTimePeriod> OpeningTimes { get; set; }
    }

    public class AddOpenTimePeriodToRestaurantView
    {

        public AddOpenTimePeriodToRestaurantView()
        {
            OpenTimePeriodList = new OpenTimePeriodList();
            NbrOrdersPerHoursList = new List<int>(new int[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });            
        }

        public int RestoId { get; set; }
        public int OpenTimeId { get; set; }
        public int CloseTimeId { get; set; }

        public string RestoName { get; set; }
        public DayOfWeek Day { get; set; }

        public OpenTimePeriodList OpenTimePeriodList { get;}

        public List<int> NbrOrdersPerHoursList { get; set; }
        public int NbrOrdersPerHour{ get; set; }
    }

        public class OpenTimePeriodList
    {

        public List<TimeSpanView> timeSpanViews = new List<TimeSpanView>
        {
            new TimeSpanView(1, new TimeSpan(11,30,0)),
            new TimeSpanView(2, new TimeSpan(12,00,0)),
            new TimeSpanView(3, new TimeSpan(12,30,0)),
            new TimeSpanView(4, new TimeSpan(13,00,0)),
            new TimeSpanView(5, new TimeSpan(13,30,0)),
            new TimeSpanView(6, new TimeSpan(14,00,0)),
            new TimeSpanView(7, new TimeSpan(14,30,0)),
            new TimeSpanView(8, new TimeSpan(15,00,0)),
            new TimeSpanView(9, new TimeSpan(15,30,0)),
            new TimeSpanView(10, new TimeSpan(16,00,0)),
            new TimeSpanView(11, new TimeSpan(16,30,0)),
            new TimeSpanView(12, new TimeSpan(17,00,0)),
            new TimeSpanView(13, new TimeSpan(17,30,0)),
            new TimeSpanView(14, new TimeSpan(18,00,0)),
            new TimeSpanView(15, new TimeSpan(18,30,0)),
            new TimeSpanView(16, new TimeSpan(19,00,0)),
            new TimeSpanView(17, new TimeSpan(19,30,0)),
            new TimeSpanView(18, new TimeSpan(20,00,0)),
            new TimeSpanView(19, new TimeSpan(20,30,0)),
            new TimeSpanView(20, new TimeSpan(21,00,0)),
            new TimeSpanView(21, new TimeSpan(21,30,0)),
            new TimeSpanView(22, new TimeSpan(22,00,0)),
            new TimeSpanView(23, new TimeSpan(22,30,0)),
            new TimeSpanView(24, new TimeSpan(23,00,0)),
            new TimeSpanView(25, new TimeSpan(23,30,0)),
            new TimeSpanView(26, new TimeSpan(00,00,0))
        };

    }

    public class TimeSpanView
    {
        public TimeSpanView(int id, TimeSpan timeSpan)
        {
            Id = id;
            TimeSpan = timeSpan;           

        }

        public int Id { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string TimeSpanText
        {
            get
            {
                return TimeSpan.ToString(@"hh\:mm");
            }
        }
    }

    public class DeleteRestoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AddChefToRestaurantViewModel
    {
        public AddChefToRestaurantViewModel()
        {
            this.User = new List<SelectUserRestoViewModel>();
        }

        public IEnumerable<string> getSelectedIds()
        {
            return (from p in this.User where p.Selected select p.Id).ToList();
        }

        public String RestoName { get; set; }

        public List<SelectUserRestoViewModel> User { get; set; }

        public bool chefOrNotAdmin { get; set; }       
    }

    public class SelectUserRestoViewModel
    {
        public bool Selected { get; set; }
        public string Id { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}