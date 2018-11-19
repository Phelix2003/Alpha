using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Alpha.Models;

namespace Alpha.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        
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


        public async Task<ActionResult> Index()
        {

            if (User.Identity.IsAuthenticated)
            {
                var user = await UserManager.FindByNameAsync(User.Identity.Name);
                if(user != null)
                {
                    user.LastLoginDate = DateTime.Now;
                    await UserManager.UpdateAsync(user);
                }

            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}