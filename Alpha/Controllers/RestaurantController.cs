using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alpha.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace Alpha.Controllers
{
    public class RestaurantController : Controller
    {
        
        private IDal dal;        
        public RestaurantController() : this(new Dal())
        {
        }

        public RestaurantController(IDal dalIoc)
        {
            dal = dalIoc;
            HttpContext.GetOwinContext();
        }
        


        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        // GET: Restaurant
        public async Task<ActionResult> Index()
        {
            return View(await dal.GetAllRestaurants());
        }

        //Get 
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewBag.UserList = await UserManager.Users.ToListAsync();
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel createViewModel)
        {
            if(ModelState.IsValid)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(createViewModel.UserId);              
                Resto resto = await dal.CreateRestaurant(createViewModel.Name, createViewModel.PhoneNumber, user);
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        public async Task<bool> AddChefToRestaurant(int RestoId, ApplicationUser Chef)
        {
            //Resto restoFound = await bdd.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);
            using (var appContext = new ApplicationDbContext())
            {
                Resto restoFound = await appContext.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);

                if (restoFound != null)
                {
                    restoFound.Chefs.Add(Chef);
                    await appContext.SaveChangesAsync();
                    return true;
                }
                else
                    return false;
            }
        }

    }
}