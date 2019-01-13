using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alpha.Models;
using Alpha.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Alpha.Controllers
{
    [Authorize]
    public class OrderController : Controller
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

        // GET: Order
        public async Task<ActionResult> Index(int RestoId)
        {
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);

            if (User.Identity.IsAuthenticated && resto != null)
            {
                string UserId = User.Identity.GetUserId();
                ApplicationUser user = await UserManager.FindByIdAsync(UserId);
                Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.OrderOwner.Id == user.Id);

                if (order == null)
                {
                    // This user has no order started so far.
                    // Starting new order process from start
                    order = new Order
                    {
                        IsOrderCompleted = false,
                        OrderOpenTime = DateTime.Now,
                        OrderOwner = user,
                        Resto = resto
                    };
                    DbManager.Orders.Add(order);
                    await DbManager.SaveChangesAsync();

                    return View(new OrderViewModels {
                        ListOfProposedItems = order.Resto.Menu.ItemList,
                        RestoId = order.Resto.Id,
                        Resto_Description = order.Resto.Description,
                        Resto_Name = order.Resto.Name,
                        OrderId = order.Id

                    });
                }
                else
                {     
                    if(order.IsOrderCompleted)
                    {
                        // This user has already an ongoing order
                        // Redirect on the edition of this order
                        return RedirectToAction("ViewOngoinOrder", new { id = order.Id });
                    }
                    else
                    {
                        // This user has started an order previously but did add anithing in it.
                        // in case of an order has been created but not startd (no article in the list) then the order is recreated with a fresh new one;
                        if (order.OrderItemList.Count == 0 ||  order.OrderItemList == null)
                        {
                            order.IsOrderCompleted = false;
                            order.OrderOpenTime = DateTime.Now;
                            order.OrderOwner = user;
                            order.Resto = resto;

                            await DbManager.SaveChangesAsync();
                            return View(new OrderViewModels
                            {
                                ListOfProposedItems = order.Resto.Menu.ItemList,
                                RestoId = order.Resto.Id,
                                Resto_Description = order.Resto.Description,
                                Resto_Name = order.Resto.Name,
                                OrderId = order.Id

                            });
                        }
                        else
                        {
                            // This user has started an order. Items are already in the basket. 
                            // If the restaurant is the same of the order ... Continue progressing in the order
                            if (order.Resto.Id == RestoId)
                            {
                                return View(new OrderViewModels
                                {
                                    ListOfProposedItems = order.Resto.Menu.ItemList,
                                    RestoId = order.Resto.Id,
                                    Resto_Description = order.Resto.Description,
                                    Resto_Name = order.Resto.Name,
                                    OrderId = order.Id

                                });
                            }
                            else
                            // Otherwise propose to the user to cancel ongoin order to start  a new one.
                            {
                                return RedirectToAction("DeleteExestingOrder", new { id = order.Id });
                            }

                        }
                        
                    }
                }       

            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public async Task<ActionResult> AddItemToOrder (int ItemId, int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(m => m.Id == OrderId);
            Item item = await DbManager.Items.FirstOrDefaultAsync(m => m.ItemId == ItemId);

            if (order != null && item != null)
            {
                order.OrderItemList.Add(item);
                await DbManager.SaveChangesAsync();

                return RedirectToAction("Index", new { RestoId = order.Resto.Id });
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public async Task<ActionResult> ViewOngoinOrder(int OrderId)
        {
            return View();
        }



    }
}