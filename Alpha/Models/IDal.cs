using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace Alpha.Models
{
    public interface IDal : IDisposable
    {
        Task<Resto> CreateRestaurant(string name, string telephone);
        Task<Resto> CreateRestaurant(string name, string telephone, ApplicationUser Admin);

        Task<List<Resto>> GetAllRestaurants();
        //Task<bool> AddChefToRestaurant(int RestoId, ApplicationUser Chef);
    }
}