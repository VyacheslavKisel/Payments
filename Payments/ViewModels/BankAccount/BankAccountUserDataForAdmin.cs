using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    public class BankAccountUserDataForAdmin
    {
        public BankAccountUserDataForAdmin(int id, string numberAccount, string numberCard, 
            string name, double balance, bool lockoutEnabled, bool requestUnblock)
        {
            Id = id;
            NumberAccount = numberAccount;
            NumberCard = numberCard;
            Name = name;
            Balance = balance;
            LockoutEnabled = lockoutEnabled;
            RequestUnblock = requestUnblock;
        }

        public int Id { get; }

        [Display(Name = "Номер счета")]
        public string NumberAccount { get; }

        [Display(Name = "Номер карты")]
        public string NumberCard { get; }

        [Display(Name = "Номер счета")]
        public string Name { get; }

        [Display(Name = "Остаток")]
        public double Balance { get; }

        public bool LockoutEnabled { get; }

        public bool RequestUnblock { get; }
    }
}