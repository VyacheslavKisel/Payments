using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Account
{
    // Модель представления для регистрации клиента
    public class RegisterModel
    {
        [Required(ErrorMessage = "Введите адрес электронной почты")]
        [Display(Name = "Адрес электронной почты")]
        [RegularExpression(@"\w{1,}@\w{1,}.\w{1,}",
            ErrorMessage = "Введите адрес в формате a@b.c, где а,b,c - буквы или цифры")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не меньше 6 и не больше 100 символов", MinimumLength = 6)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Подтверждение пароля")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string PasswordConfirm { get; set; }
    }
}