using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.Services
{
    public static class ServiceBankAccount
    {
        public static string FormNumberAccount()
        {
            Random randDigit = new Random();
            string formedNumberBankAccount = "";
            for (int i = 0; i < 14; i++)
            {
                int temp = randDigit.Next() % 10;
                formedNumberBankAccount += temp.ToString();
            }
            return formedNumberBankAccount;
        }
    }
}