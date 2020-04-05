using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Payment
{
    public class ReplenishBankAccountModel
    {
        [Required]
        public int BankAccountId { get; set; }

        [Required(ErrorMessage = "Введите номер карты")]
        [Display(Name = "Номер банковского счета")]
        [RegularExpression(@"[0-9]{14}", ErrorMessage = "Номер банковского счета " +
           "должен быть в формате aaabbbbcccdddd, где a, b, c, d - цифры от 0 до 9")]
        public string NumberBankAccount { get; set; }

        [Required(ErrorMessage = "Введите сумму платежа")]
        [Display(Name = "Сумма")]
        [RegularExpression(@"\d{1,},{0,1}\d{0,2}", ErrorMessage = "Сумма должна быть либо целым числом, " +
            "либо дробным с одним или двумя знаками после запятой")]
        public string Sum { get; set; }
    }
}