using System;
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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;

namespace Alpha.Controllers.API
{
    [Authorize]
    public class CustomerOrderController : ApiController
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


        // GET: api/CustomerOrder
        public async Task<OrderAPIModel> Get()
        {
            var userName = User.Identity.GetUserName();
            var user = await UserManager.FindByNameAsync(userName);

            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.OrderOwner.Id == user.Id);

            // in case of no order open. Create a new one
            if (order == null)
            {                
                order = new Order
                {
                    IsOrderCompleted = false,
                    OrderOpenTime = DateTime.Now,
                    OrderOwner = user
                };
                DbManager.Orders.Add(order);
                await DbManager.SaveChangesAsync();
            }

            OrderAPIModel orderAPI = new OrderAPIModel
            {
                IsOrderCompleted = order.IsOrderCompleted,
                Id = order.Id,
            };
            if(order.OrderSlot != null)
            {
                orderAPI.OrderSlot = new OrderSlotAPI
                {
                    OrderSlotId = order.OrderSlot.OrderSlotId,
                    OrderSlotTime = order.OrderSlot.OrderSlotTime,
                    SlotGroup = order.OrderSlot.SlotGroup
                };
            }
            if(order.OrderedItems.Count() > 0)
            {
                foreach(var item in order.OrderedItems)
                {
                    orderAPI.OrderedItems.Add(new OrderedItemAPIModel
                    {
                        ItemId = item.Id,
                        Quantity = item.Quantity,
                        SelectedHotNotCold = item.SelectedHotNotCold,
                        SelectedMeatId = item.SelectedMeatId,
                        SelectedSalt = item.SelectedSalt,
                        SelectedSauceId = item.SelectedSauceId,
                        SelectedSize = item.SelectedSize
                    });
                }
            }
            return orderAPI;

        }

        // POST: api/CustomerOrder
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CustomerOrder/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CustomerOrder/5
        public void Delete(int id)
        {
        }
    }
}
