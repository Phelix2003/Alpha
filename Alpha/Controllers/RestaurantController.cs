using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alpha.Models;
using System.Threading.Tasks;

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
        }

        // GET: Restaurant
        public async Task<ActionResult> Index()
        {
            return View(await dal.GetAllRestaurants());
        }

        //Get 
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel createViewModel)
        {
            if(ModelState.IsValid)
            {
                await dal.CreateRestaurant(createViewModel.Name, createViewModel.PhoneNumber);
                return RedirectToAction("Index");

            }
            else
            {
                return View();
            }
        }

    }
}