using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.BankAccount
{
    // Модель представления для добавления 
    // банковского счета
    public class CreatureBankAccountModel
    {
        [Required]
        public string ApplicationUserId { get; set; }

        [Required(ErrorMessage = "Поле номер банковской карты не должно быть пустым")]
        [RegularExpression(@"[0-9]{16}",
            ErrorMessage = "Введите номер банковской карты в формате aaaabbbbccccdddd, где a, b, c, d - цифры от 0 до 9")]
        [Display(Name = "Номер банковской карты")]
        public string NumberCard { get; set; }

        [Required(ErrorMessage = "Поле наименование счета не должно быть пустым")]
        [StringLength(100, ErrorMessage = "Наименование счета должно содержать не меньше 3 и не больше 100 символов", MinimumLength = 3)]
        [Display(Name = "Наименование счета")]
        public string Name { get; set; }
    }
}