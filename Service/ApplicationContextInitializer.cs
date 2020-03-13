using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    // Инициализатор базы данных
    public class ApplicationContextInitializer : CreateDatabaseIfNotExists<ApplicationContext>
    {
        protected override void Seed(ApplicationContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
            var roleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context));

            var adminRole = new ApplicationRole { Name = "administrator" };
            var clientRole = new ApplicationRole { Name = "client" };

            roleManager.Create(adminRole);
            roleManager.Create(clientRole);

            string password = "123456";
            var admin = new ApplicationUser { Email = "admin@gmail.com", UserName = "admin@gmail.com" };
            var result = userManager.Create(admin, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(admin.Id, adminRole.Name);
            }

            var client = new ApplicationUser { Email = "client@gmail.com", UserName = "client@gmail.com" };
            var resultClient = userManager.Create(client, password);

            if (resultClient.Succeeded)
            {
                userManager.AddToRole(client.Id, clientRole.Name);
            }

            base.Seed(context);
        }
    }
}
