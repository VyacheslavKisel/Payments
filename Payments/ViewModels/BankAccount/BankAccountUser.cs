using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    public class BankAccountUser
    {
        public BankAccountUser(int id, string numberCard, string name, double balance)
        {
            Id = id;
            NumberCard = numberCard;
            Name = name;
            Balance = balance;
        }
        public int Id { get; }
        public string NumberCard { get; }
        public string Name { get; }
        public double Balance { get; }
    }
}