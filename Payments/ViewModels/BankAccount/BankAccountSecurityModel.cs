using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    public class BankAccountSecurityModel
    {
        public BankAccountSecurityModel(int id, string numberAccount, string numberCard, string name, double balance, bool lockoutEnabled)
        {
            Id = id;
            NumberAccount = numberAccount;
            NumberCard = numberCard;
            Name = name;
            Balance = balance;
            LockoutEnabled = lockoutEnabled;
        }
        public int Id { get; }
        public string NumberAccount { get; }
        public string NumberCard { get; }
        public string Name { get; }
        public double Balance { get; }
        public bool LockoutEnabled { get; }
    }
}