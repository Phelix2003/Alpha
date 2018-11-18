using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace Alpha.Models
{
    // Vous pouvez ajouter des données de profil pour l'utilisateur en ajoutant d'autres propriétés à votre classe ApplicationUser. Pour en savoir plus, consultez https://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Notez qu'authenticationType doit correspondre à l'élément défini dans CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Ajouter les revendications personnalisées de l’utilisateur ici
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
           : base("LocalConnection", throwIfV1Schema: false)
//            : base("AzureConnection", throwIfV1Schema: false)
            
        {
        }

        static ApplicationDbContext()
        {
            // Useful for debug mode. Need to be desabled in production mode.
            // Set the database intializer which is run once during application start
            // This seeds the database with admin user credentials and admin role
            // Chose one of the following option to initialize the DB.

            // it will create the database if none exists as per the configuration
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());

            //drops an existing database and creates a new database, if your model classes (entity classes) have been changed.
            //So, you don't have to worry about maintaining your database schema, when your model classes change.
            //Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());

            //this initializer drops an existing database every time you run the application, irrespective of whether your model classes have changed or not.
            //This will be useful when you want a fresh database every time you run the application, for example when you are developing the application.
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    // Roles definition based on the application
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }
        public string Description { get; set; }
    }
}