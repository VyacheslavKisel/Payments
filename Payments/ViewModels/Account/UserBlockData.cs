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
        public string Email { get; set; }
        public bool LockoutEnabled { get; set; }

        [Required(ErrorMessage = "Введите дату и время, до которой вы хотите заблокировать аккаунт")]
        [DataType(DataType.DateTime, ErrorMessage = "Введите дату и время в формате yyyy.mm.dd hh:nn," +
            " где yyyy - год, mm - месяц, dd - день, hh - часы, yy - минуты")]
        public DateTime DateTimeBlock { get; set; }
    }
}