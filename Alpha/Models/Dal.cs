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
        private ApplicationDbContext bdd;

        public Dal()
        {
            bdd = new ApplicationDbContext();
        }

        public async Task<Resto> CreateRestaurant(string name, string telephone)

        {
           
            Resto resto = new Resto
            {
                Name = name,
                PhoneNumber = telephone,
                Chefs = new List<ApplicationUser>(),
                Administrators = new List<ApplicationUser>()
            };

            bdd.Restos.Add(resto);
            await bdd.SaveChangesAsync();
            return resto;
        }

        public async Task<Resto> CreateRestaurant(string name, string telephone, ApplicationUser Admin)

        {

            Resto resto = new Resto
            {
                Name = name,
                PhoneNumber = telephone,
                Chefs = new List<ApplicationUser>() ,
                Administrators = new List<ApplicationUser>() 
            };

            resto.Administrators.Add(Admin);

            bdd.Restos.Add(resto);
            await bdd.SaveChangesAsync();
            return resto;
        }

        /*
        public async Task<bool> AddChefToRestaurant(int RestoId, ApplicationUser Chef)
        {
            //Resto restoFound = await bdd.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);
            Resto restoFound = await bdd.Restos.FirstOrDefaultAsync(resto => resto.Id == RestoId);

            if (restoFound != null)
            {
                restoFound.Chefs.Add(Chef);
                await bdd.SaveChangesAsync();
                return true;
            }
            else
                return false;
        }

    */
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