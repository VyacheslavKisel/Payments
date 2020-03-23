using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Account
{
    // Модель представления входа в учетную запись
    public class LoginModel
    {
        [Required]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}