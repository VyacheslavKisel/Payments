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
        public string NumberCard { get; set; }
        public string Name { get; set; }
        public double Balance { get; set; }
        public bool LockoutEnabled { get; set; }

        [MaxLength(128)]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}
