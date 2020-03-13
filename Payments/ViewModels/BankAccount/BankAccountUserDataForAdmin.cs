using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    public class BankAccountUserDataForAdmin
    {
        public BankAccountUserDataForAdmin(int id, string numberCard, string name, double balance, bool lockoutEnabled)
        {
            Id = id;
            NumberCard = numberCard;
            Name = name;
            Balance = balance;
            LockoutEnabled = lockoutEnabled;
        }
        public int Id { get; }
        public string NumberCard { get; }
        public string Name { get; }
        public double Balance { get; }
        public bool LockoutEnabled { get; }
    }
}