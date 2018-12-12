using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Alpha.Models;
using System.Globalization;

namespace Alpha.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public ActionResult Index()
        {
            return View();
        }

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

        [HttpGet]
        public ActionResult CreateItem()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateItem( CreateItemViewModel itemViewModel  )
        {
            if (ModelState.IsValid)
            {
                Item item = new Item
                {
                    Name = itemViewModel.Name,
                    // To do : add additional control on the convert feature (catch exceptions) 
                    UnitPrice = Convert.ToDecimal(itemViewModel.UnitPrice, new CultureInfo("en-US"))
                };
                DbManager.Items.Add(item);
                await DbManager.SaveChangesAsync();
                
                return RedirectToAction("CreateItem");
            }
            else
            {
                return View();
            }

        }

    }
}