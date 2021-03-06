﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    public class BankAccountSecurityModel
    {
        public BankAccountSecurityModel(int id, string numberAccount, string numberCard, string name,
            double balance, bool lockoutEnabled, bool requestUnblock)
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
        public string NumberAccount { get; }
        public string NumberCard { get; }
        public string Name { get; }
        public double Balance { get; }
        public bool LockoutEnabled { get; }
        public bool RequestUnblock { get; }
    }
}