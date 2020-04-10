using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Account
{
    // Данные участвующие в блокировке клиента
    public class UserBlockDataDTO
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTime DateTimeBlock { get; set; }
    }
}
