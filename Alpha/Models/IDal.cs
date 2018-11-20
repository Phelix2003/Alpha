using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace Alpha.Models
{
    public interface IDal : IDisposable
    {
        Task CreateRestaurant(string name, string telephone);
        Task<List<Resto>> GetAllRestaurants();
    }
}