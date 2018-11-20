using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Alpha.Models
{
    public class Dal : IDal
    {
        private BddContext bdd;

        public Dal()
        {
            bdd = new BddContext();
        }

        public async Task CreateRestaurant(string name, string telephone)
        {
            bdd.Restos.Add(new Resto { Name = name, PhoneNumber = telephone });
            await bdd.SaveChangesAsync();
        }

        public async Task<List<Resto>> GetAllRestaurants()
        {             
            return await bdd.Restos.ToListAsync();
        }


        public void Dispose()
        {
            bdd.Dispose();
        }
    }
}