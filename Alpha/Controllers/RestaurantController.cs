﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Alpha.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace Alpha.Controllers
{
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
                Resto resto = await CreateRestaurant(createViewModel.Name, createViewModel.PhoneNumber, user, null);
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
                AdministratorsList = adminList
            });
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,PhoneNumber")] EditRestoViewModel editResto)
        {
            if (ModelState.IsValid)
            {
                
                Resto ModifiedResto = await DbManager.Restos.FindAsync(editResto.Id);

                if (ModifiedResto != null)
                {
                    ModifiedResto.Name = editResto.Name;
                    ModifiedResto.PhoneNumber = editResto.PhoneNumber;

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
            return View(new AddChefToRestaurantViewModel()
            {
                RestoId = RestoId,
                UserList = UserList
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddChefToRestaurant()
        {
            using (var appContext = new ApplicationDbContext())
            {
                Resto restoFound = await appContext.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);

                if (restoFound != null)
                {
                    restoFound.Chefs.Add(Chef);
                    await appContext.SaveChangesAsync();
                    return View();
                }
                else
                    return View();
            }
        }

        public async Task<Resto> CreateRestaurant(string name, string telephone, ApplicationUser Admin, ApplicationUser Chef)

        {

            Resto resto = new Resto
            {
                Name = name,
                PhoneNumber = telephone,
                Chefs = new List<ApplicationUser>(),
                Administrators = new List<ApplicationUser>()
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

    }
}