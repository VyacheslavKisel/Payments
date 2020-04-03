using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models
{
    // Модель банковского счета
    public class BankAccount
    {
        public int Id { get; set; }
        public string NumberAccount { get; set; }
        public string NumberCard { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public bool LockoutEnabled { get; set; }

        [MaxLength(128)]
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }
}
