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
                        OrderOwner = user
                    };
                    DbManager.Orders.Add(order);
                    await DbManager.SaveChangesAsync();

                    ICollection<ItemView> itemsView = await CreateItemsViewFromOrderAndRestoId(order, RestoId);
                    return View(new OrderViewModels {
                        ListOfProposedItems = itemsView,
                        RestoId = RestoId,
                        Resto_Description = resto.Description,
                        Resto_Name = resto.Name,
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
                        // This user has started an order previously but did not add anything in it.
                        // in case of an order has been created but not startd (no article in the list) then the order is recreated with a fresh new one;
                        if (order.OrderSlot == null)
                        {
                            order.IsOrderCompleted = false;
                            order.OrderOpenTime = DateTime.Now;
                            order.OrderOwner = user;
                            

                            await DbManager.SaveChangesAsync();
                            ICollection<ItemView> itemsView = await CreateItemsViewFromOrderAndRestoId(order, RestoId);

                            return View(new OrderViewModels
                            {
                                ListOfProposedItems = itemsView,
                                RestoId = resto.Id,
                                Resto_Description = resto.Description,
                                Resto_Name = resto.Name,
                                OrderId = order.Id

                            });
                        }
                        else
                        {
                            // This user has started an order. Items are already in the basket. 
                            // If the restaurant is the same of the order ... Continue progressing in the order
                            if (order.OrderSlot.RestoId == RestoId)
                            {
                                ICollection<ItemView> itemsView = await CreateItemsViewFromOrderAndRestoId(order, RestoId);
                                return View(new OrderViewModels
                                {
                                    ListOfProposedItems =itemsView,
                                    RestoId = RestoId,
                                    Resto_Description = resto.Description,
                                    Resto_Name = resto.Name,
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

                if (order.OrderSlot == null)
                {
                    // No Slottime has been selected yet to this restaurant. 
                    // Or slot time has been released because of timout. Need to find new slot time.
                    return RedirectToAction("PickASlotTimeStep0", new { RestoId = item.Menu.resto.Id});
                }


                return RedirectToAction("AddItemStep1aSize");
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public async Task<ActionResult> PickASlotTimeStep0(int RestoId)
        {
            // Check and creates the list of available slottimes
            // TODO: Current strategy is to create this list on request. See for optimization to schedule this list creation one time a day (off ressources-
            await CreateOrderSlotListForDay(RestoId, DateTime.Today);
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);

            // Collect the available slot times. 
            List<OrderSlot> availableOrderSlots = new List<OrderSlot>();
            availableOrderSlots = resto.OrderIntakeSlots.Where(r => r.OrderSlotTime != null).ToList();

            PickASlotTimeView pickSlotTimeView = new PickASlotTimeView();
            foreach(var item in resto.OrderIntakeSlots.Where(r => r.OrderSlotTime.CompareTo(DateTime.Now) > 0))
            {
                pickSlotTimeView.Add(item);
            }
            return View(pickSlotTimeView);
        }

        [HttpPost]
        public async Task<ActionResult> PickASlotTime(string button)
        {
            DateTime selectedTime = new DateTime(long.Parse(button));
            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.Id == OrderedItemView.Current.OrderId);
            Item item = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.ItemId);
            
            if (selectedTime != null && order != null && item != null)
            {
                // Analyse the comming free slots for the first one in the seleted time range (15 min). 
                order.OrderSlot = item.Menu.resto.OrderIntakeSlots
                    .Where(r => r.OrderSlotTime.CompareTo(selectedTime) >= 0)
                    .Where(r => r.OrderSlotTime.CompareTo(DateTime.Now) > 0)
                    .Where(r => r.OrderSlotTime.CompareTo(selectedTime.AddMinutes(15)) <= 0)
                    .FirstOrDefault(r => r.Order == null); // Select only free slots           
                await DbManager.SaveChangesAsync();
                return RedirectToAction("AddItemToOrder", new { ItemId = item.ItemId, OrderId = order.Id}); // Now a slot time should be associated to that order. 
                //In cas no slot could be associated because of concurent access  OrderSlot = null then process start again to find a new one. 
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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
                    OrderedItemView.Current.SelectedMeatId = null;
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
                    orderedItem.SelectedMeat = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.SelectedMeatId);
                }
                else
                {
                    orderedItem.SelectedMeat = null;
                }

                if (OrderedItemView.Current.SelectedSauceId != 0 && OrderedItemView.Current.SelectedSauceId != null)
                {
                    orderedItem.SelectedSauce = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == OrderedItemView.Current.SelectedSauceId);
                }
                else
                {
                    orderedItem.SelectedSauce = null;
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
                return RedirectToAction("Index", new { RestoId = item.Menu.resto.Id});
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public async Task<ActionResult> FinalizeOrder (int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.Id == OrderId);

            if(order != null)
            {
                FinalaizeOrderView finalizeOrderView = new FinalaizeOrderView
                {
                    OrderId = OrderId,
                    RestoId = order.OrderSlot.RestoId,
                    TotalOrderPrice = order.OrderedItems.Sum(r => r.ConfiguredPrice()).ToString(),
                    OrderedItems = new List<OrderedItemView>()
                };

                foreach (var item in order.OrderedItems)
                {
                    finalizeOrderView.OrderedItems.Add(new OrderedItemView
                    {
                        ConfiguredName = item.Name(),
                        PriceString = item.ConfiguredPrice().ToString(),
                        QuantityString = item.Quantity.ToString(),
                        TypeOfFood = item.Item.TypeOfFood,
                        ItemId = item.Id
                    });
                }
                return View(finalizeOrderView);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

        }

        public async Task<ActionResult> DeleteOneOrderedItem(int OrderedItemId)
        {
            OrderedItem orderedItem = await DbManager.OrderedItems.FirstOrDefaultAsync(r => r.Id == OrderedItemId);
            if(orderedItem != null)
            {
                int OrderId = orderedItem.CurrentOrder.Id;
                if(orderedItem.Quantity > 1)
                {
                    orderedItem.Quantity--;
                }
                else
                {
                    DbManager.OrderedItems.Remove(orderedItem);
                }
                await DbManager.SaveChangesAsync();
                return RedirectToAction("FinalizeOrder", new { OrderId = OrderId });
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

        // Create the list of Items view for this restaurant and associated to this order (if the order has been started) 
        public async Task<ICollection<ItemView>> CreateItemsViewFromOrderAndRestoId(Order order,int RestoId)
        {
           
            ICollection<ItemView> itemsView = new List<ItemView>();

            // Check if this order has already a slot associated
            if(order.OrderSlot !=null)
            {
                // Create the list of items in this restaurant and associate the number of items listed in the Order
                if (order.OrderSlot.Resto.Menu != null && order.OrderSlot.Resto.Menu.ItemList != null) // Check if items are existing - otherwise the list stay blank
                {
                    foreach (var item in order.OrderSlot.Resto.Menu.ItemList.Where(r => r.DeletedOn == null))
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
            }
            else
            {
                // The order is not started yet. Then create list of items for the restaurant with no qty
                Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);
                if (resto.Menu != null && resto.Menu.ItemList != null)  // Check if items are existing - otherwise the list stay blank
                {
                    foreach (var item in resto.Menu.ItemList.Where(r => r.DeletedOn == null))
                    {
                        ItemView itemView = new ItemView(item);
                        itemsView.Add(itemView);
                    }
                }

            }
            return itemsView;
        }

        public async Task CreateOrderSlotListForDay(int RestoId, DateTime Date)
        {
            // Retrieve the resto to create the slots.
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);

            if(resto != null)
            {
                if(resto.OrderIntakeSlots.FirstOrDefault(r => r.OrderSlotTime.Date == Date.Date) == null) // Check if slots time for that day has already been created 
                {
                    int i = 1;
                    foreach (var times in resto.OpeningTimes.Where(r => r.DayOfWeek == Date.DayOfWeek))
                    {
                        i = i++;                        
                        List<TimeSpan> orderSlotList = times.GetListOfOrderSlots();
                        foreach (var slotToConvert in orderSlotList)
                        {
                            DateTime slotInDateTime = new DateTime(Date.Year, Date.Month, Date.Day, slotToConvert.Hours, slotToConvert.Minutes, slotToConvert.Seconds);
                            resto.OrderIntakeSlots.Add(new OrderSlot { Resto = resto , OrderSlotTime = slotInDateTime, SlotGroup = i});
                        }
                    }
                    await DbManager.SaveChangesAsync();
                }
            }
            else
            {
                throw new Exception("CreateOrderSlotListForDay -- No resto found with RestoId!");
            }
        }
    }
}