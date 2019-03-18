using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Alpha.Models;

using Newtonsoft.Json;
using Alpha.Models.APIModels;
using Alpha.Helpers.Common;

using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Description;

namespace Alpha.Controllers.API
{
    [Authorize]
    public class SlotTimeController : ApiController
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

        // GET: api/SlotTime
        [ResponseType(typeof(List<OrderSlotAPI>))]
        public async Task<IHttpActionResult> Get(int id)
        {
            OrderSlot orderSlot = await DbManager.OrderSlots.FirstOrDefaultAsync(o => o.OrderSlotId == 57);
            var userName = User.Identity.GetUserName();
            var user = await UserManager.FindByNameAsync(userName);
            

            if(user == null)
            {
                return BadRequest();
            }

            // Check and creates the list of available slottimes
            // TODO: Current strategy is to create this list on request. See for optimization to schedule this list creation one time a day (off ressources-
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == id);
            if (resto == null)
                return BadRequest();
            await new OrderHelper().CreateOrderSlotListForDay(id, DateTime.Today, DbManager);


            // Collect the available slot times. 
            List<OrderSlot> availableOrderSlots = new List<OrderSlot>();

            availableOrderSlots = resto.OrderIntakeSlots.Where(r => r.OrderSlotTime != null).ToList();

            List<OrderSlotAPI> slotTimeAPI = new List<OrderSlotAPI>();
            foreach (var item in resto.OrderIntakeSlots
                .Where(r => r.Order == null)                                // Check this slot time is free
                .Where(r => r.OrderSlotTime.CompareTo(DateTime.Now) > 0))
            {
                slotTimeAPI.Add(new OrderSlotAPI {
                OrderSlotId = item.OrderSlotId,
                OrderSlotTime = item.OrderSlotTime,
                SlotGroup = item.SlotGroup});
            }
            return Ok(slotTimeAPI);
        }

        // PUT: api/SlotTime/5
        
        public async Task<IHttpActionResult> Post(int id)
        {
            var userName = User.Identity.GetUserName();
            var user = await UserManager.FindByNameAsync(userName);
            var orderSlot = await DbManager.OrderSlots.FirstOrDefaultAsync(r => r.OrderSlotId == id);

            if (user == null)            
                return BadRequest("Unknown User");           
            
            if (user.PlacedOrder == null)
                return BadRequest("And order should have been, created before associating a SlotTiem");

            //OrderSlot orderSlot = await DbManager.OrderSlots.FirstOrDefaultAsync(o => o.OrderSlotId == id);        
            if (orderSlot == null)
                return BadRequest("OrderSlot does not exist");

            if (orderSlot.Order != null)
                return BadRequest("This order is already in use");

            orderSlot.Order = await DbManager.Orders.FirstAsync(r => r.Id == user.PlacedOrder.Id); // Necessaire de rechercher le "order" à partir du contexte DbManager
            
            await DbManager.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/SlotTime/5
        
        public void Delete(int id)
        {
        }
    }
}
