﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Alpha.Models.APIModels;
using Alpha.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Configuration;
using System.Web;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;



namespace Alpha.Controllers.API
{
    [Authorize]
    public class RestaurantController : ApiController
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }

        }

        private ApplicationDbContext _dbManager;
        public ApplicationDbContext DbManager
        {
            get
            {
                return _dbManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _dbManager = value;
            }
        }


        [ResponseType(typeof(ListRestoAPIModel))]
        public async Task<IHttpActionResult> Get(int id)
        {

            Resto resto = await db.Restos.FirstOrDefaultAsync(v => v.Id == id);
            if (resto == null)
            {
                return NotFound();
            }

            var userName = User.Identity.GetUserName();
            var user = await UserManager.FindByNameAsync(userName);
            if (user == null)
            {
                return BadRequest("User Authentication failed");
            }

            // Preparing the order context. 
            if(user.PlacedOrder == null)
            {
                // No order is prepared yet for this user. 
                // Initiate a new order 
                Order order = new Order
                {
                    IsOrderCompleted = false,
                    OrderOpenTime = DateTime.Now,
                    OrderOwner = user
                };
                DbManager.Orders.Add(order);
                await DbManager.SaveChangesAsync();
            }
            else
            {
                // On order is already existing with this customer 

                // On order is already completed with this user... 
                if (user.PlacedOrder.IsOrderCompleted)
                {
                    return BadRequest("An order is already completed for this user: OrderId :" + user.PlacedOrder.Id);
                }
                else
                {
                    // This user has started an order previously but did not add anything in it.
                    // in case of an order has been created but not startd (no article in the list) then the order is recreated with a fresh new one;
                    if(user.PlacedOrder.OrderSlot == null)
                    {
                        user.PlacedOrder.OrderOpenTime = DateTime.Now;
                        await DbManager.SaveChangesAsync();

                        //Continue 
                    }
                    else
                    {                    
                        // This user has started an order. Items are already in the basket.                         
                        // If the restaurant is the same of the order ... Continue progressing in the order
                        if(user.PlacedOrder.OrderSlot.Resto.Id == id)
                        {
                            // Continue
                        }
                        else
                        {
                            return BadRequest("An order has already been started in another restaurant: OrderId -" + user.PlacedOrder.Id);
                        }
                    }

                }
            }

            ListRestoAPIModel restoAPI = new ListRestoAPIModel
            {
                ResponseHeader = new ResponseHeaderAPIModel { SpecVersion = ConfigurationManager.AppSettings["CurrentAPIVersion"] },
                Restos = new List<RestoAPIModel>
                {
                    new RestoAPIModel
                    {
                        Address = resto.Address,
                        Description = resto.Description,
                        Id = resto.Id,
                        Image = resto.Image,
                        Name = resto.Name,
                        PhoneNumber = resto.PhoneNumber,
                        Menu = new MenuAPIModel
                        {
                            MenuId = resto.Menu.MenuId,
                            Name = resto.Menu.Name,
                            ItemList = new List<ItemAPIModel>()
                        }
                        
                    }
                }
            };
            foreach(var item in resto.Menu.ItemList)
            {
                if(item.DeletedOn == null)
                {
                    restoAPI.Restos[0].Menu.ItemList.Add(new ItemAPIModel
                    {
                        Name = item.Name,
                        Brand = item.Brand,
                        UnitPrice = item.UnitPrice,
                        Description = item.Description,
                        HasSize = item.HasSize,
                        ItemId = item.ItemId,
                        CanBeHotNotCold = item.CanBeHotNotCold,
                        CanBeSalt = item.CanBeSalt,
                        CanHaveMeat = item.CanHaveMeat,
                        CanHaveSauce = item.CanHaveSauce,
                        TypeOfFood = item.TypeOfFood
                    });
                }
            }            
            return Ok(restoAPI);
        }

    }
}
