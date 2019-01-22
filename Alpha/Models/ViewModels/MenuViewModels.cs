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
        public CreateItemViewModel()
        {
            TypeOfFoods = new List<TypeOfFood>();
            foreach (TypeOfFood i in Enum.GetValues(typeof(TypeOfFood)))
            {
                TypeOfFoods.Add(i);
            }

            SelectedSizedMeals = new List<SelectedSizedMealViewModel>();
            foreach (MealSize i in Enum.GetValues(typeof(MealSize)))
            {
                SelectedSizedMeals.Add(new SelectedSizedMealViewModel
                {
                    Selected = false,
                    PriceText = "0",
                    SizedMeal = new SizedMeal()
                    {
                        MealSize = i,
                        Price = 0
                    }                        
                });                             
            }               
        }

        public int ItemId { get; set; }
        [Required(ErrorMessage = "Enter a valide restaurant name")]
        [Display(Name = "Article Name")]
        public string Name { get; set; }


        [Display(Name = "Article Description")]
        public string Description { get; set; }

        [Display(Name = "Rendre disponnible?")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Prix")]
        public string UnitPrice { get; set; }

        [Display(Name = "Sélectionner le type d'article")]
        public TypeOfFood SelectedTypeOfFood { get; set; }
        public List<TypeOfFood> TypeOfFoods { get; set; }

        [Display(Name = "Sélectionner les tailles disponnible ")]
        public List<SelectedSizedMealViewModel> SelectedSizedMeals { get; set; }

        [Display(Name = "Cet article dispose de plusieurs tailles?")]
        public bool HasSize { get; set; }

        public bool CanBeSalt { get; set; }

        [Display(Name = "Peut-il être mangé chaud ou froid?")]
        public bool CanBeHotNotCold { get; set; }

        [Display(Name = "Une viande peut être associée")]
        public bool CanHaveMeat { get; set; }

        [Display(Name = "Une sauce peut être associée")]
        public bool CanHaveSauce { get; set; }

        public int MenuId { get; set; }

        [Display(Name = "Article image")]
        public HttpPostedFileBase Image { get; set; }
    }

    public class SelectedSizedMealViewModel
    {
        public bool Selected { get; set; }
        public string PriceText { get; set; }
        public SizedMeal SizedMeal{ get; set; }
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