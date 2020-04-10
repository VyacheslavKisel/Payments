using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Account
{
    // Частичные учетные данные о клиенте
    public class DataUserForAdminDTO
    {
        public DataUserForAdminDTO(string id, string email, string userName,
           bool lockoutEnabled, DateTime? lockoutEndDateUtc)
        {
            Id = id;
            Email = email;
            UserName = userName;
            LockoutEnabled = lockoutEnabled;
            LockoutEndDateUtc = lockoutEndDateUtc;
        }
        public string Id { get; }
        public string Email { get; }
        public string UserName { get; }
        public bool LockoutEnabled { get; }
        public DateTime? LockoutEndDateUtc { get; }
    }
}
