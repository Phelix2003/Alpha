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

namespace Alpha.Controllers.API
{
    [Authorize]
    public class RestaurantAPIController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [ResponseType(typeof(RestoAPIModel))]
        public async Task<IHttpActionResult> Get(int id)
        {

            Resto resto = await db.Restos.FirstOrDefaultAsync(v => v.Id == id);
            if (resto == null)
            {
                return NotFound();
            }

            RestoAPIModel restoAPI = new RestoAPIModel
            {
                ResponseHeader = new ResponseHeaderAPIModel { SpecVersion = ConfigurationManager.AppSettings["CurrentAPIVersion"] },
                Address = resto.Address,
                Description = resto.Description,
                Id = resto.Id,
                Image = resto.Image,
                Name = resto.Name,
                PhoneNumber = resto.PhoneNumber,
                Menu = new MenuAPIModel
                {
                    MenuId = resto.Menu.MenuId,
                    Name = resto.Menu.Name,
                    ItemList = new List<ItemAPIModel>()
                }
            };
            foreach(var item in resto.Menu.ItemList)
            {
                if(item.DeletedOn == null)
                {
                    restoAPI.Menu.ItemList.Add(new ItemAPIModel
                    {
                        Name = item.Name,
                        Brand = item.Brand,
                        Image = item.Image,
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
            return Ok(restoAPI);
        }


        [ResponseType(typeof(List<RestoAPIModel>))]
        public async Task<IHttpActionResult> Get()
        {

            List<Resto> restos = await db.Restos.ToListAsync();
            if (restos == null)
            {
                return NotFound();
            }

            List<RestoAPIModel> APIRestos = new List<RestoAPIModel>();

            foreach(var resto in restos)
            {
                RestoAPIModel restoAPIModel = new RestoAPIModel
                {
                    ResponseHeader = new ResponseHeaderAPIModel { SpecVersion = ConfigurationManager.AppSettings["CurrentAPIVersion"] },
                    Address = resto.Address,
                    Description = resto.Description,
                    Id = resto.Id,
                    Image = resto.Image,
                    Name = resto.Name,
                    PhoneNumber = resto.PhoneNumber,
                    Menu = new MenuAPIModel
                    {
                        MenuId = resto.Menu.MenuId,
                        Name = resto.Menu.Name,
                        ItemList = new List<ItemAPIModel>()
                    }
                };

                foreach (var item in resto.Menu.ItemList)
                {
                    if (item.DeletedOn == null)
                    {
                        restoAPIModel.Menu.ItemList.Add(new ItemAPIModel
                        {
                            Name = item.Name,
                            Brand = item.Brand,
                            Image = item.Image,
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
                APIRestos.Add(restoAPIModel);
            }
            return Ok(restos);
        }


    }
}
