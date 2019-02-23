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

            return Ok(new RestoAPIModel {
            ResponseHeader = new ResponseHeaderAPIModel { SpecVersion = ConfigurationManager.AppSettings["CurrentAPIVersion"]}, 
            Address = resto.Address,
            Description = resto.Description,
            Id = resto.Id,
            Image = resto.Image,
            Name = resto.Name,
            PhoneNumber = resto.PhoneNumber,
            } );
        }

    }
}
