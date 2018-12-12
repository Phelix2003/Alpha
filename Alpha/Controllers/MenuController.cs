using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
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

        [HttpPost]
        public async Task<ActionResult> ItemUploadImage(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                // for more details see here https://stackoverflow.com/questions/1171696/how-do-you-convert-a-httppostedfilebase-to-an-image
                // To do ajouter la gestion d'exceptions; 
                Image image = Image.FromStream(file.InputStream, true, true);
                Item item = new Item
                {
                    Name = "Picture test",
                    Image = imageToByteArray(image)
                };
                DbManager.Items.Add(item);
                await DbManager.SaveChangesAsync();



                ViewBag.Message = "File uploaded successfully";
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return RedirectToAction("CreateItem");
        }

        private byte[] imageToByteArray(Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        private Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

    }
}