using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Alpha.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Configuration;
using Newtonsoft.Json;

namespace Alpha.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RestaurantController : Controller
    {
        
        private ApplicationDbContext _dbManager;
        public ApplicationDbContext DbManager
        {
            get
            {
                return _dbManager ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _dbManager = value;
            }
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
            return View(await GetAllRestaurants());
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
                Resto resto = await CreateRestaurant(createViewModel.Name, createViewModel.PhoneNumber, user, null, createViewModel.Address);
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        //Get 
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var resto = await DbManager.Restos.FirstAsync(r => r.Id == id);
            if (resto == null)
            {
                return HttpNotFound();
            }

            var adminList = resto.Administrators.ToList();
            var chefList = resto.Chefs.ToList();

            return View(new EditRestoViewModel()
            {
                Id = resto.Id,
                Name = resto.Name,
                PhoneNumber = resto.PhoneNumber,
                ChefsList = chefList,
                AdministratorsList = adminList,
                Address = resto.Address,
                menu = resto.Menu
            });
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,PhoneNumber,Address")] EditRestoViewModel editResto)
        {
            if (ModelState.IsValid)
            {
                
                Resto ModifiedResto = await DbManager.Restos.FindAsync(editResto.Id);

                if (ModifiedResto != null)
                {
                    ModifiedResto.Name = editResto.Name;
                    ModifiedResto.PhoneNumber = editResto.PhoneNumber;
                    ModifiedResto.Address = editResto.Address;

                    await DbManager.SaveChangesAsync();
                    return RedirectToAction("Index");
                }

            }

            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> AddChefToRestaurant(int RestoId)
        {
            ICollection<ApplicationUser> UserList = await UserManager.Users.ToListAsync();
            Resto restofound = await DbManager.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);

            if (UserList != null && restofound != null)
            {
                var model = new AddChefToRestaurantViewModel()
                {
                    RestoName = restofound.Name,
                };

                foreach (var user in UserList)
                {
                    var userViewModel = new SelectUserRestoViewModel()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Selected = true
                    };
                    if (restofound.Chefs.FirstOrDefault(m => m.Id == user.Id) == null)
                    {
                        userViewModel.Selected = false;
                    }
                    model.User.Add(userViewModel);
                }
               return View(model);
            }
            return View("Error");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddChefToRestaurant(AddChefToRestaurantViewModel model)
        {
            var selectedIds = model.getSelectedIds();
            Resto restoFound = await DbManager.Restos.FirstOrDefaultAsync(resto => resto.Name == model.RestoName);


            if (restoFound != null)
            {
                var selectedUsers = from x in DbManager.Users
                                    where selectedIds.Contains(x.Id)
                                    select x;
                foreach (var user in selectedUsers)
                {
                    restoFound.Chefs.Add(user);
                }
                await DbManager.SaveChangesAsync();

                return RedirectToAction("Edit", new { id = restoFound.Id});
            }
            else
                return View("Error");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int? RestoId)
        {
            if (RestoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);
            if (resto == null)
            {
                return View("Error");
            }
            return View(new DeleteRestoViewModel() { Id = resto.Id });
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(DeleteRestoViewModel restaurant)
        {
            if (ModelState.IsValid)
            {
                if ( restaurant == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == restaurant.Id);
                if (resto == null)
                {
                    return View("Error");
                }
                DbManager.Restos.Remove(resto);
                var result = await DbManager.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }




            public async Task<Resto> CreateRestaurant(string name, string telephone, ApplicationUser Admin, ApplicationUser Chef, string Address)
        {

            Resto resto = new Resto
            {
                Name = name,
                PhoneNumber = telephone,
                Chefs = new List<ApplicationUser>(),
                Administrators = new List<ApplicationUser>(),
                Address = Address
                
            };

            if(Admin != null)
                resto.Administrators.Add(Admin);
            if(Chef != null)
                resto.Chefs.Add(Chef);

            DbManager.Restos.Add(resto);
            await DbManager.SaveChangesAsync();
            return resto;
        }

        public async Task<List<Resto>> GetAllRestaurants()
        {
            return await DbManager.Restos.ToListAsync();
        }

        /// <summary>  
        /// This method is used to get the place list  
        /// </summary>  
        /// <param name="SearchText"></param>  
        /// <returns></returns>  
        [HttpGet, ActionName("GetEventVenuesList")]
        public JsonResult GetEventVenuesList(string SearchText)
        {
            string placeApiUrl = ConfigurationManager.AppSettings["GooglePlaceAPIUrl"];

            try
            {
                placeApiUrl = placeApiUrl.Replace("{0}", SearchText);
                placeApiUrl = placeApiUrl.Replace("{1}", ConfigurationManager.AppSettings["GoogleApiSecret"]);

                var result = new System.Net.WebClient().DownloadString(placeApiUrl);
                var Jsonobject = JsonConvert.DeserializeObject<RootObject>(result);

                List<Prediction> list = Jsonobject.predictions;

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

    }
}