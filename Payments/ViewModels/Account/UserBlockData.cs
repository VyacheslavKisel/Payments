using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Payments.ViewModels.Account
{
    // Модель представления для данных о пользователе, 
    // которые требуются для его блокировки и разблокироки
    public class UserBlockData
    {
        public string UserId { get; set; }

        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Display(Name = "Заблокирована учетная запись или нет")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Дата, до которой заблокирована учетная запись")]
        [Required(ErrorMessage = "Введите дату до которой вы хотите заблокировать учетную запись")]
        [DataType(DataType.Date)]
        public DateTime DateTimeBlock { get; set; }
    }
}