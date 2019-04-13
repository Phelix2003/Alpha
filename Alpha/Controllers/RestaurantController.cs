using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Alpha.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Configuration;
using Newtonsoft.Json;
using Alpha.Helpers.Images;
using System.Text;
using Alpha.Helpers.Common;
using System.IO;
using System.Text.RegularExpressions;

namespace Alpha.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RestaurantController : Controller
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


        // GET: Restaurant
        public async Task<ActionResult> Index()
        {
            return View(await GetAllRestaurants());
        }

        //Get 
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            ViewBag.UserList = await UserManager.Users.ToListAsync();
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateRestoViewModel createViewModel)
        {
            if(ModelState.IsValid)
            {
                if(createViewModel.Image == null)
                {
                    ViewBag.Message = "Introduisez une image";
                    return View();
                }

                ApplicationUser user = await UserManager.FindByIdAsync(createViewModel.UserId);              
                Resto resto = await CreateRestaurant(createViewModel.Name, createViewModel.PhoneNumber, user, null, createViewModel.Address, createViewModel.Image, createViewModel.Description);
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        //Get 
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
       {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var resto = await DbManager.Restos.Include("Menu").FirstAsync(r => r.Id == id);
            if (resto == null)
            {
                return HttpNotFound();
            }

            var adminList = resto.Administrators.ToList();
            var chefList = resto.Chefs.ToList();

            return View(new EditRestoViewModel()
            {
                Id = resto.Id,
                Name = resto.Name,
                PhoneNumber = resto.PhoneNumber,
                ChefsList = chefList,
                AdministratorsList = adminList,
                Address = resto.Address,
                menu = resto.Menu,
                OpeningTimes = resto.OpeningTimes,
                Description = resto.Description                
            });
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,Name, Description,PhoneNumber,Address,SelectedSlotTimeId_1_Start,SelectedSlotTimeId_2_Start,SelectedSlotTimeId_1_Stop,SelectedSlotTimeId_2_Stop")] EditRestoViewModel editResto)
        public async Task<ActionResult> Edit(EditRestoViewModel editResto)

        {
            var resto = await DbManager.Restos.FirstAsync(r => r.Id == editResto.Id);
            if (resto == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                Resto ModifiedResto = await DbManager.Restos.FindAsync(editResto.Id);
                if (ModifiedResto != null)
                {
                    try
                    {
                        ModifiedResto.Owner_DateOfBirth = DateTime.Parse(editResto.PaymentInfo_dob);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "Something failed.");
                        editResto.AdministratorsList = resto.Administrators.ToList();
                        editResto.menu = resto.Menu;
                        editResto.ChefsList = resto.Chefs.ToList();
                        return View(editResto);
                    }

                    ModifiedResto.Name = editResto.Name;
                    ModifiedResto.PhoneNumber = editResto.PhoneNumber;
                    ModifiedResto.Address = editResto.Address;
                    ModifiedResto.Shop_Email = editResto.PaymentInfo_Email;
                    ModifiedResto.Description = editResto.Description;
                    ModifiedResto.Payment_BankAccountId = editResto.PaymentInfo_bankAccountId;
                    ModifiedResto.Payment_BankAdress = editResto.PaymentInfo_bankAddress;
                    ModifiedResto.Payment_BankCity = editResto.PaymentInfo_bankCity;
                    ModifiedResto.Payment_BankId = "";
                    ModifiedResto.Payment_BankName = editResto.PaymentInfo_bankName;
                    ModifiedResto.Payment_BankZip = editResto.PaymentInfo_bankZip;
                    ModifiedResto.Payment_IBAN = editResto.PaymentInfo_bankAccountId;
                    ModifiedResto.Payment_NameOnAccount = editResto.PaymentInfo_lastName;
                    ModifiedResto.Payment_SwiftBIC = "";
                    ModifiedResto.Owner_Address = editResto.PaymentInfo_address;
                    ModifiedResto.Owner_City = editResto.PaymentInfo_city;
                    ModifiedResto.Payment_commissionPercent = int.Parse(editResto.PaymentInfo_commissionPercent);

                    ModifiedResto.Owner_email = editResto.PaymentInfo_Email;
                    ModifiedResto.Owner_FirstName = editResto.PaymentInfo_firstName;
                    ModifiedResto.Owner_LastName = editResto.PaymentInfo_lastName;
                    ModifiedResto.Owner_PassportNumber = editResto.PaymentInfo_driverLicenseNumber;
                    ModifiedResto.Owner_PersonalIDNumber = editResto.PaymentInfo_personalIdentificationNumber;
                    ModifiedResto.Owner_ZipCode = editResto.PaymentInfo_zip;
                    ModifiedResto.Payment_SwiftBIC = editResto.PaymentInfo_bankswiftBic;
                    ModifiedResto.Payment_minimalPayoutAmount = int.Parse(editResto.PaymentInfo_minimalPayoutAmount);

                    if (editResto.Image != null)
                    {
                        ModifiedResto.Image = new ImageHelper().ProcessFileToImage(editResto.Image);
                    }
                    await DbManager.SaveChangesAsync();
                    return RedirectToAction("Index");
                }               
            }

            ModelState.AddModelError("", "Something failed.");
            editResto.AdministratorsList = resto.Administrators.ToList();
            editResto.menu = resto.Menu;
            editResto.ChefsList = resto.Chefs.ToList();
            return View(editResto);
        }

        public async Task<ActionResult> AddOpenTimePeriodToRestaurant(int RestoId, DayOfWeek dayOfWeek)
        {
            // TODO https://www.codeproject.com/Tips/826002/Bootstrap-Modal-Dialog-Loading-Content-from-MVC-Pa
            var resto = await DbManager.Restos.FirstAsync(r => r.Id == RestoId);
            if (resto != null)
            {
                return View(new AddOpenTimePeriodToRestaurantView() {RestoId = RestoId, RestoName = resto.Name, Day = dayOfWeek});
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public async Task<ActionResult> AddOpenTimePeriodToRestaurant(AddOpenTimePeriodToRestaurantView model)
        {
            var resto = await DbManager.Restos.FirstAsync(r => r.Id == model.RestoId);
            if(resto != null)
            {
                if(model.OpenTimeId < model.CloseTimeId)
                {
                    OpenTimePeriod openTimePeriod = new OpenTimePeriod()
                    {
                        DayOfWeek = model.Day,
                        OpenTime = model.OpenTimePeriodList.timeSpanViews.FirstOrDefault(m => m.Id == model.OpenTimeId).TimeSpan,
                        CloseTime = model.OpenTimePeriodList.timeSpanViews.FirstOrDefault(m => m.Id == model.CloseTimeId).TimeSpan,
                        NbAuthorizedOrderPerHour = model.NbrOrdersPerHour                   
                    };

                    resto.OpeningTimes.Add(openTimePeriod);
                    await DbManager.SaveChangesAsync();
                    return RedirectToAction("edit", new { id = model.RestoId });
                }
                else
                {
                    ModelState.AddModelError("OpenTimePeriodList", "L'heure de fermeture doit être après l'heure d'ouverture");
                    return View(model);
                }               
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        
        public async Task<ActionResult> RemoveOpenTimePeriodToRestaurant(int RestoId, int SlotId)
        {
            var resto = await DbManager.Restos.FirstAsync(r => r.Id == RestoId);
            if (resto != null)
            {
                OpenTimePeriod openTimePeriod = resto.OpeningTimes.FirstOrDefault(m => m.OpenTimePeriodId == SlotId);

                if(openTimePeriod != null)
                {
                    DbManager.OpenTimePeriods.Remove(openTimePeriod);
                    await DbManager.SaveChangesAsync();
                    return RedirectToAction("edit", new { id = RestoId });
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }


        [HttpGet]
        public async Task<ActionResult> AddChefToRestaurant(int RestoId, bool ChefOrNotAdmin)
        {
            ICollection<ApplicationUser> UserList = await UserManager.Users.ToListAsync();
            Resto restofound = await DbManager.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);

            if (UserList != null && restofound != null)
            {
                var model = new AddChefToRestaurantViewModel()
                {
                    RestoName = restofound.Name,
                    chefOrNotAdmin = ChefOrNotAdmin
                };

                foreach (var user in UserList)
                {
                    var userViewModel = new SelectUserRestoViewModel()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Selected = true
                    };
                    if (restofound.Chefs.FirstOrDefault(m => m.Id == user.Id) == null && ChefOrNotAdmin)
                    {
                        userViewModel.Selected = false;
                    }
                    if (restofound.Administrators.FirstOrDefault(m => m.Id == user.Id) == null && !ChefOrNotAdmin)
                    {
                        userViewModel.Selected = false;
                    }
                    model.User.Add(userViewModel);
                }
               return View(model);
            }
            return View("Error");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddChefToRestaurant(AddChefToRestaurantViewModel model)
        {
            var selectedIds = model.getSelectedIds();
            Resto restoFound = await DbManager.Restos.FirstOrDefaultAsync(resto => resto.Name == model.RestoName);


            if (restoFound != null)
            {
                var selectedUsers = from x in DbManager.Users
                                    where selectedIds.Contains(x.Id)
                                    select x;
                if (model.chefOrNotAdmin)
                {
                    restoFound.Chefs.Clear();
                }
                else
                {
                    restoFound.Administrators.Clear();
                }

                foreach (var user in selectedUsers)
                {
                    if (model.chefOrNotAdmin)
                    {
                        restoFound.Chefs.Add(user);
                    }
                    else
                    {
                        restoFound.Administrators.Add(user);
                    }
                }
                await DbManager.SaveChangesAsync();

                return RedirectToAction("Edit", new { id = restoFound.Id});
            }
            else
                return View("Error");
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int? RestoId)
        {
            if (RestoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == RestoId);
            if (resto == null)
            {
                return View("Error");
            }
            return View(new DeleteRestoViewModel() { Id = resto.Id, Name = resto.Name });
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(DeleteRestoViewModel restaurant)
        {
            if (ModelState.IsValid)
            {
                if ( restaurant == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == restaurant.Id);
                if (resto == null)
                {
                    return View("Error");
                }
                DbManager.Restos.Remove(resto);
                var result = await DbManager.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }




        public async Task<Resto> CreateRestaurant(string name, string telephone, ApplicationUser Admin, ApplicationUser Chef, string Address, HttpPostedFileBase Image, string Description)
        {
            Resto resto = new Resto
            {
                Name = name,
                PhoneNumber = telephone,
                Chefs = new List<ApplicationUser>(),
                Administrators = new List<ApplicationUser>(),
                Address = Address,
                Image = new ImageHelper().ProcessFileToImage(Image),
                Description = Description,
                Owner_DateOfBirth = null

            };

            if(Admin != null)
                resto.Administrators.Add(Admin);
            if(Chef != null)
                resto.Chefs.Add(Chef);

            DbManager.Restos.Add(resto);
            await DbManager.SaveChangesAsync();
            return resto;
        }

        public async Task<List<Resto>> GetAllRestaurants()
        {
            return await DbManager.Restos.ToListAsync();
        }

        [AllowAnonymous]
        public async Task<ActionResult> RenderRestoPhoto(int RestoId)
        {
            Resto resto = await DbManager.Restos.FindAsync(RestoId);
            if( resto!= null)
            {
                if(resto.Image != null)
                {
                    byte[] photo = resto.Image;
                    return File(photo, "image/jpeg");
                }
            }
            return null;
        }

        // Verify the payment has been executed. 
        public async Task<ActionResult> CreatePaymentAccount(int Id)
        {
            Resto resto = await DbManager.Restos.FirstOrDefaultAsync(r => r.Id == Id);

            if (resto != null)
            {
                string Uri = ConfigurationManager.AppSettings["BlueSnapGatewayBaseURL"] + "/vendors";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Uri);
                httpWebRequest.ContentType = "application/Json";
                httpWebRequest.Method = "POST";

                // Configure basic Authentication
                string username = ConfigurationManager.AppSettings["BlueSnapUserName"];
                string password = ConfigurationManager.AppSettings["BlueSnapPassword"];

                string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

                httpWebRequest.Headers.Add("Authorization", "Basic " + svcCredentials);


                // Creates JSon for creating a new vendor
                string json = new BlueSnapPayment().BlueSnapAddVendorJson(resto);

                

                if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                {
                    System.Diagnostics.Debug.WriteLine("Payment request sent to SaferPay (JSon):");
                    System.Diagnostics.Debug.WriteLine(json);
                }
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                    streamWriter.Close();
                }

                // Send request to SaferPay server
                WebResponse httpWebResponse;
                try
                {
                    httpWebResponse = await httpWebRequest.GetResponseAsync();
                    string location = httpWebResponse.Headers["Location"]; // get the headers with the location ID in the request
                    if(location != null)
                    {
                        // the ID of the vendor created is respecting the following format: https://sandbox.bluesnap.com/services/2/vendors/19575974
                        // Collect the last num digits of the string; 
                        var result = Regex.Match(location, @"\d+$").Value;
                        if(result.Length < 6)
                        {// Received ID sound not to be in a good format.
                            System.Diagnostics.Debug.WriteLine("Payment - Vendor ID received is not in a good format");
                            return RedirectToAction(
                                "Notification",
                                "Home",
                                new
                                {
                                    Message = "Erreur lors de la création du compte de payement. Format d'ID retourné incorrecte",
                                    Url = this.Url.Action("Index", "Restaurant", null)
                                });

                        }
                        resto.Payment_BlueSnapPayoutVendorId = result;
                        System.Diagnostics.Debug.WriteLine("Payment - Vendor created on ID " + result);
                    }else
                    {
                        System.Diagnostics.Debug.WriteLine("Payment - Vendor could not be created");
                    }
                    return RedirectToAction(
                        "Notification",
                        "Home",
                        new
                        {
                            Message = "Le compte de payement du restaurant a été créé avec success",
                            Url = this.Url.Action("Index", "Restaurant", null)
                        });
                }
                catch (WebException we)
                {
                    // The rpayment request has been refused by SaferPay for technical reasons. 
                    httpWebResponse = we.Response as HttpWebResponse;
                    SaferPayErrorMessage errorMessage;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = await streamReader.ReadToEndAsync();
                        errorMessage = JsonConvert.DeserializeObject<SaferPayErrorMessage>(result);
                        if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                        {
                            System.Diagnostics.Debug.WriteLine("Payment request response from SaferPay (JSon) - ERROR Message:");
                            System.Diagnostics.Debug.WriteLine(errorMessage);
                        }
                    }
                }
            }
            return RedirectToAction(
                "Notification",
                "Home",
                new
                {
                    Message = "Erreur lors de la création du compte de payement. Erreur de communication vers le serveur hôte",
                    Url = this.Url.Action("Index", "Restaurant", null)
                });
        }



        /*
        /// <summary>  
        /// This method is used to get the place list  
        /// </summary>  
        /// <param name="SearchText"></param>  
        /// <returns></returns>  
        [HttpGet, ActionName("GetEventVenuesList")]
        public JsonResult GetEventVenuesList(string SearchText)
        {
            string placeApiUrl = ConfigurationManager.AppSettings["GooglePlaceAPIUrl"];

            try
            {
                placeApiUrl = placeApiUrl.Replace("{0}", SearchText);
                placeApiUrl = placeApiUrl.Replace("{1}", ConfigurationManager.AppSettings["GoogleApiSecret"]);

                var result = new System.Net.WebClient().DownloadString(placeApiUrl);
                var Jsonobject = JsonConvert.DeserializeObject<RootObject>(result);

                List<Prediction> list = Jsonobject.predictions;

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        */

    }
}