using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Payment
{
    // Модель представления для
    // подготовки платежа
    public class PreparationPaymentModel
    {
        [Required]
        public int BankAccountId { get; set; }

        [Required(ErrorMessage = "Введите сумму платежа")]
        [Display(Name = "Сумма")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "Платеж не может быть меньше 0.01")]
        [RegularExpression(@"\d{1,},{0,1}\d{0,2}", ErrorMessage = "Сумма должна быть либо целым числом, " +
            "либо дробным с одним или двумя знаками после запятой")]
        public string Sum { get; set; }

        [Required(ErrorMessage = "Введите получателя")]
        [Display(Name = "Получатель")]
        [StringLength(128, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 128 символов")]
        public string Recipient { get; set; }

        [Required(ErrorMessage = "Введите код ЕГРПОУ")]
        [Display(Name = "Код ЕГРПОУ")]
        [RegularExpression(@"\d{8}", ErrorMessage = "Код ЕГРПОУ должен быть введен в формате ххххyyyy, " +
            "где х, y - цифры от 0 до 9 (8 цифр)")]
        public string CodeEgrpou { get; set; }

        [Required(ErrorMessage = "Введите код IBAN")]
        [Display(Name = "Код IBAN")]
        [RegularExpression(@"[A-Z]{2}[0-9]{27}", ErrorMessage = "Код IBAN в формате " +
            "UABBXXXXXXYYYYYYYYYYYYYYYYYYY, где UA - код страны - 2 латинские буквы верхнего регистра," +
            " BB - контрольный разряд - 2 цифры" +
            "ХХХХХХ - код банка (МФО) - 6 цифр, YYYYYYYYYYYYYYYYYYY - расчетный счет в банке - 19 цифр")]
        public string CodeIban { get; set; }

        [Required(ErrorMessage = "Введите назначение платежа")]
        [Display(Name = "Назначение платежа")]
        [StringLength(128, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 128 символов")]
        public string Purpose { get; set; }
    }
}