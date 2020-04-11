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
    // Контекст базы данных
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext() : base("PaymentsContext") { }

        static ApplicationContext()
        {
            Database.SetInitializer<ApplicationContext>(new ApplicationContextInitializer());
        }

        public DbSet<BankAccount> BankAccounts { get; set; }

        public DbSet<Payment> Payments { get; set; }
    }
}
