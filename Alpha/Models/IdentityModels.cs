﻿using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        //User Customization here    
        [Display(Name = "Registrated on")]
        public DateTime? RegisterDate { get; set; }
        [Display(Name = "Last Connection on")]
        public DateTime? LastLoginDate { get; set; }


        [Display(Name = "Address")]
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        // Use a sensible display name for views:
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        // Concatenate the address info for display in tables and such:
        public string DisplayAddress
        {
            get
            {
                string dspAddress =
                    string.IsNullOrWhiteSpace(this.Address) ? "" : this.Address;
                string dspCity =
                    string.IsNullOrWhiteSpace(this.City) ? "" : this.City;
                string dspState =
                    string.IsNullOrWhiteSpace(this.State) ? "" : this.State;
                string dspPostalCode =
                    string.IsNullOrWhiteSpace(this.PostalCode) ? "" : this.PostalCode;

                return string
                    .Format("{0} {1} {2} {3}", dspAddress, dspCity, dspState, dspPostalCode);
            }
        }

        public virtual ICollection<Resto> Resto_Admin { get; set; }
        public virtual ICollection<Resto> Resto_Chefs { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
           : base("LocalConnection", throwIfV1Schema: false)
//            : base("AzureConnection", throwIfV1Schema: false)            
        {
            // Set the class to call for initiating the DB
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }

        //static ApplicationDbContext()
        //{

        //}

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Resto>()
                        .HasMany<ApplicationUser>(s => s.Administrators)
                        .WithMany(c => c.Resto_Admin)
                        .Map(cs =>
                        {
                            cs.MapLeftKey("Resto_Admin");
                            cs.MapRightKey("Admin");
                            cs.ToTable("RestosAdmins");
                        });

            modelBuilder.Entity<Resto>()
            .HasMany<ApplicationUser>(s => s.Chefs)
            .WithMany(c => c.Resto_Chefs)
            .Map(cs =>
            {
                cs.MapLeftKey("Resto_Chefs");
                cs.MapRightKey("Chef");
                cs.ToTable("RestosChefs");
            });

            // Configure Student & StudentAddress entity
            modelBuilder.Entity<Resto>()
                        .HasOptional(s => s.Menu) // Mark Address property optional in Student entity
                        .WithRequired(ad => ad.resto); // mark Student property as required in StudentAddress entity. Cannot save StudentAddress without Student


        }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // Application table definition
        public DbSet<Resto> Restos { set; get; }
        public DbSet<Menu> Menus { set; get; }
        public DbSet<Item> Items { set; get; }      
     

    }

    // Roles definition based on the application
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }

        // Application Role customization here
        public string Description { get; set; } 
    }

}