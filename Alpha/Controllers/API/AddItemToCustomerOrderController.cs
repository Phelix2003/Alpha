using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Alpha.Models.APIModels;
using Alpha.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;

namespace Alpha.Controllers.API
{
    [Authorize]
    public class AddItemToCustomerOrderController : ApiController
    {
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

        // GET: api/AddItemToCustomerOrder
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/AddItemToCustomerOrder/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/AddItemToCustomerOrder
        public async Task<IHttpActionResult> Post([FromBody] OrderedItemAPIModel orderedItemAPI)
        {
            if (orderedItemAPI == null)
                return BadRequest("The input format is not accepted");
            var userName = User.Identity.GetUserName();
            var user = await UserManager.FindByNameAsync(userName);

            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.OrderOwner.Id == user.Id);
            if (order == null)
                return NotFound();



            if (orderedItemAPI == null)
                return BadRequest();

            Item item = await DbManager.Items.FirstOrDefaultAsync(i => i.ItemId == orderedItemAPI.ItemId);
            if (item == null)
                return NotFound();

            if (order != null && item != null)
            {
                //Search of equivalent item with same configuratin saved before. 
                OrderedItem orderedItem = new OrderedItem
                {
                    CurrentOrder = order,
                    Item = item,
                    SelectedHotNotCold = orderedItemAPI.SelectedHotNotCold,
                    SelectedSalt = orderedItemAPI.SelectedSalt
                };

                if (orderedItemAPI.SelectedMeatId != null)
                {
                    orderedItem.SelectedMeat = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == orderedItemAPI.SelectedMeatId);
                }
                else
                {
                    orderedItem.SelectedMeat = null;
                }

                if (orderedItemAPI.SelectedSauceId != 0 && orderedItemAPI.SelectedSauceId != null)
                {
                    orderedItem.SelectedSauce = await DbManager.Items.FirstOrDefaultAsync(r => r.ItemId == orderedItemAPI.SelectedSauceId);
                }
                else
                {
                    orderedItem.SelectedSauce = null;
                }
                
                if (orderedItemAPI.SelectedSize != null && item.AvailableSizes.Count > 0)
                {
                    orderedItem.SelectedSize = item.AvailableSizes.FirstOrDefault(r => r.MealSize == orderedItemAPI.SelectedSize).MealSize;
                }
                else
                {
                    orderedItem.SelectedSize = null;
                }


                int? foundOrderedItemId = null;

                // Check if an item with the same configuration is already present in the order list (already ordered)
                foreach (var itemOrder in order.OrderedItems)
                {
                    if (orderedItem.Compare(itemOrder))
                    {
                        foundOrderedItemId = itemOrder.Id;
                    }
                }

                if (foundOrderedItemId != null)
                {
                    // In case of already existing orderedItem then only update quanties. 
                    OrderedItem orderedItemUpdate = order.OrderedItems.FirstOrDefault(r => r.Id == foundOrderedItemId);
                    orderedItemUpdate.Quantity = orderedItemUpdate.Quantity + orderedItemAPI.Quantity;
                }
                else
                {
                    orderedItem.Quantity = 1;
                    order.OrderedItems.Add(orderedItem);
                }
                await DbManager.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        // PUT: api/AddItemToCustomerOrder/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/AddItemToCustomerOrder/5
        public void Delete(int id)
        {
        }
    }
}
