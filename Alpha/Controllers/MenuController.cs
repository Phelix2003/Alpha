using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Alpha.Models;
using System.Globalization;
using System.Net;
using System.Data.Entity;


namespace Alpha.Controllers
{
    [Authorize]
    public class MenuController : Controller
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

        // GET: Menu
        public async Task<ActionResult> Index(int? Id)
        {
            if(Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Menu menu = await DbManager.Menus.FirstOrDefaultAsync(r => r.MenuId == Id);
                MenuViewModel menuView = new MenuViewModel()
                {
                    Name = menu.Name,
                    DateOfModification = menu.DateOfModification,
                    ItemList = menu.ItemList
                };
                return View(menuView);
            }
        }


        [HttpGet]
        public async Task<ActionResult> CreateMenu(int? Id)
        {
            Resto resto = await DbManager.Restos.FindAsync(Id);
            if ( resto != null)
            {
                MenuViewModel menuView = new MenuViewModel()
                {
                    restoName = resto.Name,
                    restoId = resto.Id,
                    resto = resto
                };
                return View(menuView);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CreateMenu")]
        public async Task<ActionResult> CreateMenu(MenuViewModel menuView)
        {
            if(ModelState.IsValid)
            {
                Resto resto = await DbManager.Restos.FirstOrDefaultAsync(m => m.Id == menuView.restoId);
                if (resto != null)
                {
                    Menu menu = new Menu()
                    {
                        Name = menuView.Name,
                        DateOfModification = DateTime.Now
                        
                    };
                    DbManager.Menus.Add(menu);


                    resto.Menu= menu;

                    await DbManager.SaveChangesAsync();

                    return RedirectToAction("Edit", "Restaurant", new { id = menuView.restoId });                        
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }


            return View(menuView);

        }




        [HttpGet]
        public async Task<ActionResult> CreateItem(int Id)
        {
            Menu menu = await DbManager.Menus.FirstOrDefaultAsync(m => m.MenuId == Id);
            if (menu != null)
            {
                CreateItemViewModel itemViewModel = new CreateItemViewModel()
                {
                    IsAvailable = true,
                    MenuId = Id
                };
                return View(itemViewModel);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CreateItem")]
        public async Task<ActionResult> CreateItemPost(CreateItemViewModel itemViewModel)
        {
            if (ModelState.IsValid)
            {
                Menu menu = await DbManager.Menus.FirstAsync(m => m.MenuId == itemViewModel.MenuId);
                if (menu != null)
                {
                    if (itemViewModel.Image != null)
                    {
                        Item item = new Item()
                        {
                            IsAvailable = itemViewModel.IsAvailable,
                            Name = itemViewModel.Name,
                            Description = itemViewModel.Description,
                            UnitPrice = Convert.ToDecimal(itemViewModel.UnitPrice, new CultureInfo("en-US")),
                            Image = ProcessFileToImage(itemViewModel.Image)
                        };

                        if (item.Image == null)
                        {
                            ViewBag.Message = "Le format de l'image n'est pas correct";
                            return View(itemViewModel);
                        }
                        else
                        {
                            ViewBag.Message = "File uploaded successfully";
                        }

                        DbManager.Items.Add(item);
                        menu.ItemList.Add(item);
                        await DbManager.SaveChangesAsync();
                        ViewBag.Message = "File uploaded successfully";
                        Resto resto = await DbManager.Restos.FirstOrDefaultAsync(m =>m.Menu.MenuId == itemViewModel.MenuId);
                        if (resto != null)
                        {
                            return RedirectToAction("Edit", "Restaurant", new { id = resto.Id });
                        }
                        else
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Introduisez une image";
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            return View(itemViewModel);
        }

        [HttpGet]
        public async Task<ActionResult> EditItem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = await DbManager.Items.FirstAsync(r => r.ItemId == id);
            if (item == null)
            {
                return HttpNotFound();
            }

            return View(new EditItemViewModel()
            {
                ItemId = item.ItemId,
                Name = item.Name,
                IsAvailable = item.IsAvailable,
                UnitPrice = item.UnitPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("EditItem")]
        public async Task<ActionResult> EditItemPost(CreateItemViewModel itemViewModel)
        {
            if (ModelState.IsValid)
            {
                Item item = await DbManager.Items.FirstAsync(r => r.ItemId == itemViewModel.ItemId);

                item.IsAvailable = itemViewModel.IsAvailable;
                item.Name = itemViewModel.Name;
                item.UnitPrice = Convert.ToDecimal(itemViewModel.UnitPrice, new CultureInfo("en-US"));

                if (itemViewModel.Image != null)
                {
                    item.Image = ProcessFileToImage(itemViewModel.Image);
                    if (item.Image == null)
                    {
                        ViewBag.Message = "Error while uploading the file";
                    }
                    else
                    {
                        ViewBag.Message = "File uploaded successfully";
                    }
                }
                await DbManager.SaveChangesAsync();
            }
            return View("EditItem", new { id = itemViewModel.ItemId });
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

        public async Task<ActionResult> RenderItemPhoto(int ItemId)
        {
            Item item = await DbManager.Items.FindAsync(ItemId);
            byte[] photo = item.Image;
            return File(photo, "image/jpeg");
        }

        private byte[] ProcessFileToImage(HttpPostedFileBase dataIn)
        {
            try
            {
                const double FinalImageSize = 150; // in pixels

                int newWidth;
                int newHeight;

                using (Image image = Image.FromStream(dataIn.InputStream, true, true))
                {
                    // Rescaling image size to be done here,  before saving it in the DB. See https://stackoverflow.com/questions/1171696/how-do-you-convert-a-httppostedfilebase-to-an-image
                    //https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp

                    //var destRect = new Rectangle(0, 0, newWidth, newHeight);
                    if (image.Height < image.Width) // Image scaling while conserving the ratio. 
                    {
                        newWidth = (int)FinalImageSize;
                        newHeight = (int)((double)image.Height * (double)FinalImageSize / (double)image.Width);
                    }
                    else
                    {
                        newHeight = (int)FinalImageSize;
                        newWidth = (int)((double)image.Width * (double)FinalImageSize / (double)image.Height);
                    }

                    var destImage = new Bitmap(newWidth, newHeight);
                    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (Graphics g = Graphics.FromImage(destImage))
                    {

                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.CompositingQuality = CompositingQuality.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        using (var wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            g.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                        }
                        return imageToByteArray(destImage);
                    }
                }
            }
            catch
            {
                return null;
            }

        }
    }
}