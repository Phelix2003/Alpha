﻿using System;
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

        // ---- Start of Ordered Item creation STEP by STEP
        public async Task<ActionResult> AddItemToOrder (int ItemId, int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(m => m.Id == OrderId);
            Item item = await DbManager.Items.FirstOrDefaultAsync(m => m.ItemId == ItemId);



            if (order != null && item != null)
            {                
                int MenuId = item.Menu.MenuId;
                Menu menu = await DbManager.Menus.FirstOrDefaultAsync(m => m.MenuId == MenuId);
                OrderedItemView.Current.CanSelectHotCold = item.CanBeHotNotCold;
                OrderedItemView.Current.ItemId = item.ItemId;
                OrderedItemView.Current.ItemName = item.Name;
                OrderedItemView.Current.OrderId = order.Id;
                OrderedItemView.Current.TypeOfFood = item.TypeOfFood;
                OrderedItemView.Current.CanSelectSalt = item.CanBeSalt;
                OrderedItemView.Current.SelectedSalt = true;
                OrderedItemView.Current.SelcedtHotNotCold = true;
                OrderedItemView.Current.CanSelectMeat = item.CanHaveMeat;
                OrderedItemView.Current.ListOfMeatsView = menu.ItemList.Where(r => r.TypeOfFood == TypeOfFood.Snack).Where(r => r.DeletedOn == null).ToList();
                OrderedItemView.Current.CanSlectSauce = item.CanHaveSauce;
                OrderedItemView.Current.ListofSauceView = menu.ItemList.Where(r => r.TypeOfFood == TypeOfFood.Sauce).Where(r => r.DeletedOn == null).ToList();
                OrderedItemView.Current.ListofSauceView.Add(new Item { Name = "Pas de Sauce", ItemId = 0, Description="" });
                OrderedItemView.Current.CanSelectSize = item.HasSize;
                OrderedItemView.Current.ListOfSizesView = item.AvailableSizes.ToList();
                OrderedItemView.Current.SelectedSizeId = null;
                OrderedItemView.Current.SelectedSauceId = null;
                OrderedItemView.Current.SelectedMeatId = null;
                
                //= new OrderedItemView
                return RedirectToAction("AddItemStep1aSize");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // ---- STEP 1a ---- Review Size
        public ActionResult AddItemStep1aSize()
        {
            if (OrderedItemView.Current.CanSelectSize)
            {
                if (OrderedItemView.Current.ListOfSizesView.Count() != 0)
                {
                    return View(OrderedItemView.Current);
                }
                else
                {
                    // TODO to integrate log in azure
                    System.Diagnostics.Debug.WriteLine("ERROR in Order-AddItemStepXSize - for ItemId " + OrderedItemView.Current.ItemId + " - No Size are defined for this Item");
                    OrderedItemView.Current.SelectedSizeId = null;
                    return RedirectToAction("AddItemStep1Salt");
                };
            }
            else
            {
                return RedirectToAction("AddItemStep1Salt");
            }
        }


        [HttpPost]
        [ActionName("AddItemStep1aSize")]
        public ActionResult AddItemStep1aSize(int Button)
        {
            OrderedItemView.Current.SelectedSizeId = Button;
            return RedirectToAction("AddItemStep1Salt");
        }

        // ---- STEP 1 ---- Review Salt
        public ActionResult AddItemStep1Salt()
        {
            if (OrderedItemView.Current.CanSelectSalt)
            {
                return View(OrderedItemView.Current);
            }
            else
            {
                return RedirectToAction("AddItemStep2HotCold");
            }
        }


        [HttpPost]
        [ActionName("AddItemStep1Salt")]
        public ActionResult AddItemStep1SaltPost(string Button)
        {
            if(Button == "Salt")
            {
                OrderedItemView.Current.SelectedSalt = true;
            }
            else
            {
                OrderedItemView.Current.SelectedSalt = false;
            }
            
            return RedirectToAction("AddItemStep2HotCold");
        }

        public ActionResult AddItemStep2HotCold()
        {
            if (OrderedItemView.Current.CanSelectHotCold)
            {
                return View(OrderedItemView.Current);
            }
            else
            {
                return RedirectToAction("AddItemStep3Sauce");
            }
        }

        // ---- STEP 2 ---- Review Hot / Cold
        [HttpPost]
        [ActionName("AddItemStep2HotCold")]
        public ActionResult AddItemStep2HotCold(string Button)
        {
            if (Button == "Hot")
            {
                OrderedItemView.Current.SelcedtHotNotCold = true;
            }
            else
            {
                OrderedItemView.Current.SelcedtHotNotCold = false;
            }
            return RedirectToAction("AddItemStep3Sauce");
        }

        // ---- STEP 3 ---- Review Sauce
        public ActionResult AddItemStep3Sauce()
        {
            if (OrderedItemView.Current.CanSlectSauce)
            {
                if(OrderedItemView.Current.ListofSauceView.Count()!=0)
                {
                    return View(OrderedItemView.Current);
                }
                else
                {
                    // TODO to integrate log in azure
                    System.Diagnostics.Debug.WriteLine("ERROR in Order-AddItemStepXSauce - for ItemId " + OrderedItemView.Current.ItemId + " - No Sauce are defined for this Item");
                    OrderedItemView.Current.SelectedSauceId = null;
                    return RedirectToAction("AddItemStep4Meat");
                };
            }
            else
            {
                return RedirectToAction("AddItemStep4Meat");
            }
        }


        [HttpPost]
        [ActionName("AddItemStep3Sauce")]
        public ActionResult AddItemStep3Sauce(int Button)
        {
            OrderedItemView.Current.SelectedSauceId = Button;            
            return RedirectToAction("AddItemStep4Meat");
        }

        // ---- STEP 4 ---- Review Meat
        public ActionResult AddItemStep4Meat()
        {
            if (OrderedItemView.Current.CanSelectMeat)
            {
                if (OrderedItemView.Current.ListOfMeatsView.Count() != 0)
                {
                    return View(OrderedItemView.Current);
                }
                else
                {
                    // TODO to integrate log in azure
                    System.Diagnostics.Debug.WriteLine("ERROR in Order-AddItemStepXMeat - for ItemId " + OrderedItemView.Current.ItemId + " - No Meat are defined for this Item");
                    OrderedItemView.Current.SelectedSauceId = null;
                    return RedirectToAction("AddItemFinalStep");
                };
            }
            else
            {
                return RedirectToAction("AddItemFinalStep");
            }
        }


        [HttpPost]
        [ActionName("AddItemStep4Meat")]
        public ActionResult AddItemStep4Meat(int Button)
        {
            OrderedItemView.Current.SelectedMeatId = Button;
            return RedirectToAction("AddItemFinalStep");
        }

        // ---- FINAL STEP ---- Review of selection and save it to the ongoing order
        public async Task<ActionResult> AddItemFinalStep()
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.Id == OrderedItemView.Current.OrderId);
            Item item = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.ItemId);

            if(order!= null && item != null)
            {
                //Search of equivalent item with same configuratin saved before. 
                OrderedItem orderedItem = new OrderedItem
                {
                    CurrentOrder = order,
                    Item = item,
                    SelectedHotNotCold = OrderedItemView.Current.SelcedtHotNotCold,
                    SelectedSalt = OrderedItemView.Current.SelectedSalt
                };

                if(OrderedItemView.Current.SelectedMeatId != null)
                {
                    orderedItem.SelectedMeat = (await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.SelectedMeatId)).Name;
                }
                else
                {
                    orderedItem.SelectedMeat = "Pas de snack indiqué";
                }

                if (OrderedItemView.Current.SelectedSauceId != 0 && OrderedItemView.Current.SelectedSauceId != null)
                {
                    orderedItem.SelectedSauce = (await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.SelectedSauceId)).Name;
                }
                else
                {
                    orderedItem.SelectedSauce = "Pas de sauce indiquée";
                }

                if (OrderedItemView.Current.SelectedSizeId != null)
                {
                    orderedItem.SelectedSize = item.AvailableSizes.FirstOrDefault(r => r.Id == OrderedItemView.Current.SelectedSizeId).MealSize;
                }
                else
                {
                    orderedItem.SelectedSize = null;
                }


                int? foundOrderedItemId = null;

                // Check if an item with the same configuration is already present in the order list (already ordered)
                foreach(var itemOrder in order.OrderedItems)
                {
                    if (orderedItem.Compare(itemOrder))
                    {
                        foundOrderedItemId = itemOrder.Id;
                    }
                }

                if(foundOrderedItemId != null)
                {
                    // In case of already existing orderedItem then only update quanties. 
                    OrderedItem orderedItemUpdate = order.OrderedItems.FirstOrDefault(r => r.Id == foundOrderedItemId);
                    orderedItemUpdate.Quantity++;
                }else
                {
                    orderedItem.Quantity = 1;
                    order.OrderedItems.Add(orderedItem);
                }
                await DbManager.SaveChangesAsync();
                return RedirectToAction("Index", new { RestoId = order.Resto.Id});
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


        [HttpPost]
        [ActionName("DeleteExestingOrder")]
        public async Task<ActionResult> DeleteExestingOrderPost(Order orderIn)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(m => m.Id == orderIn.Id);
            if (order != null)
            {
                DbManager.Orders.Remove(order);
                await DbManager.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        public ICollection<ItemView> CreateItemsViewFromOrder(Order order)
        {
            ICollection<ItemView> itemsView = new List<ItemView>();
            if(order.Resto.Menu != null && order.Resto.Menu.ItemList != null)
            {
                foreach (var item in order.Resto.Menu.ItemList.Where(r => r.DeletedOn == null))
                {
                    ItemView itemView = new ItemView(item);
                    // TODO This part need to be optimized; 
                    // Check in the list of items with 
                    List<OrderedItem> orderedItems = new List<OrderedItem>();
                    if (order.OrderedItems != null)
                    {
                        foreach (var element in order.OrderedItems)
                        {
                            if (item.ItemId == element.ItemId)
                            {
                                orderedItems.Add(element);
                            }
                        }
                    }
                    itemView.Quantity = orderedItems.Select(i => i.Quantity).Sum();
                    itemsView.Add(itemView);
                }
            }
            return itemsView;
        }
    }
}