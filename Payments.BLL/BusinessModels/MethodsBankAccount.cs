using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.BusinessModels
{
    public static class MethodsBankAccount
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
