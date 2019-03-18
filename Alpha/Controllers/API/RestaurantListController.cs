using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Http.Description;
using Alpha.Models.APIModels;
using System.Threading.Tasks;
using Alpha.Models;
using System.Data.Entity;

namespace Alpha.Controllers.API
{
    [Authorize]
    public class RestaurantListController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [ResponseType(typeof(ListRestoAPIModel))]
        public async Task<IHttpActionResult> Get()
        {

            List<Resto> restos = await db.Restos.ToListAsync();
            if (restos == null)
            {
                return NotFound();
            }

            ListRestoAPIModel APIRestos =new ListRestoAPIModel();

            APIRestos.ResponseHeader = new ResponseHeaderAPIModel { SpecVersion = ConfigurationManager.AppSettings["CurrentAPIVersion"] };
            APIRestos.Restos = new List<RestoAPIModel>();

            foreach (var resto in restos)
            {
                RestoAPIModel restoAPIModel = new RestoAPIModel
                {
                    Address = resto.Address,
                    Description = resto.Description,
                    Id = resto.Id,
                    Image = resto.Image,
                    Name = resto.Name,
                    PhoneNumber = resto.PhoneNumber,
                    
                };
                if(resto.Menu != null)
                {
                    restoAPIModel.Menu = new MenuAPIModel
                    {
                        MenuId = resto.Menu.MenuId,
                        Name = resto.Menu.Name,
                        ItemList = new List<ItemAPIModel>()
                    };
                    if (resto.Menu.ItemList !=null)
                    {
                        foreach (var item in resto.Menu.ItemList)
                        {
                            if (item.DeletedOn == null)
                            {
                                restoAPIModel.Menu.ItemList.Add(new ItemAPIModel
                                {
                                    Name = item.Name,
                                    Brand = item.Brand,
                                    UnitPrice = item.UnitPrice,
                                    Description = item.Description,
                                    HasSize = item.HasSize,
                                    ItemId = item.ItemId,
                                    CanBeHotNotCold = item.CanBeHotNotCold,
                                    CanBeSalt = item.CanBeSalt,
                                    CanHaveMeat = item.CanHaveMeat,
                                    CanHaveSauce = item.CanHaveSauce,
                                    TypeOfFood = item.TypeOfFood
                                });
                            }
                        }
                    }
                }
                APIRestos.Restos.Add(restoAPIModel);
            }
            return Ok(APIRestos);
        }
    }
}
