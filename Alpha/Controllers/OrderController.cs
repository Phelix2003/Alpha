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
                if(user == null)
                {
                    return RedirectToAction("LogOff","Account");
                }
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

                    ICollection<ItemView> itemsView = CreateItemsViewFromOrder(order);
                    return View(new OrderViewModels {
                        ListOfProposedItems = itemsView,
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
                        if (order.OrderedItems.Count == 0 ||  order.OrderedItems == null)
                        {
                            order.IsOrderCompleted = false;
                            order.OrderOpenTime = DateTime.Now;
                            order.OrderOwner = user;
                            order.Resto = resto;

                            await DbManager.SaveChangesAsync();
                            ICollection<ItemView> itemsView = CreateItemsViewFromOrder(order);

                            return View(new OrderViewModels
                            {
                                ListOfProposedItems = itemsView,
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
                                ICollection<ItemView> itemsView = CreateItemsViewFromOrder(order);
                                return View(new OrderViewModels
                                {
                                    ListOfProposedItems =itemsView,
                                    RestoId = order.Resto.Id,
                                    Resto_Description = order.Resto.Description,
                                    Resto_Name = order.Resto.Name,
                                    OrderId = order.Id

                                });
                            }
                            else
                            // Otherwise propose to the user to cancel ongoin order to start  a new one.
                            {
                                return RedirectToAction("DeleteExestingOrder", new { OrderId = order.Id });
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
                OrderedItem orderedItem = null;
                // Check if this item is already present in the current order. (with the same configuration)
                if (order.OrderedItems != null)
                {
                    foreach (var element in order.OrderedItems)
                    {
                        if (element.ItemId == ItemId)
                        {
                            orderedItem = element;
                        }
                    }
                }                 

                if (orderedItem != null)
                { 
                    //If the same Items as already been ordered before. add 1 to quantity
                    orderedItem.Quantity++;
                }
                else
                {
                    //If this items has not been ordered before then created a new orderedItem 
                    orderedItem = new OrderedItem
                    {
                        Quantity = 1
                    };

                    orderedItem.Item = item;
                    orderedItem.CurrentOrder = order;                    
                                 
                }

                order.OrderedItems.Add(orderedItem);
                await DbManager.SaveChangesAsync();
                return RedirectToAction("Index", new { RestoId = order.Resto.Id });
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public async Task<ActionResult> ViewOngoinOrder(int OrderId)
        {
            return View();
        }

        public async Task<ActionResult> DeleteExestingOrder (int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(m => m.Id == OrderId);

            if(order != null)
            {
                return View(order);
            }                       
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ICollection<ItemView> CreateItemsViewFromOrder(Order order)
        {
            ICollection<ItemView> itemsView = new List<ItemView>();
            foreach (var item in order.Resto.Menu.ItemList)
            {
                ItemView itemView = new ItemView(item);

                // TODO This part need to be optimized; 
                List<OrderedItem> orderedItems = new List<OrderedItem>();
                foreach(var element in order.OrderedItems)
                {
                    if (item.ItemId == element.ItemId)
                    {
                        orderedItems.Add(element);
                    }

                }
                itemView.Quantity = orderedItems.Select(i => i.Quantity).Sum();
                itemsView.Add(itemView);
            }
            return itemsView;
        }



    }
}