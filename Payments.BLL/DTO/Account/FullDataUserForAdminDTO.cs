using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.DTO.Account
{
    // Учетные данные об клиенте доступные администратору
    public class FullDataUserForAdminDTO
    {
        public FullDataUserForAdminDTO(string id, string email, string userName,
           bool lockoutEnabled, DateTime? lockoutEndDateUtc, int numberUnblockRequests)
        {
            Id = id;
            Email = email;
            UserName = userName;
            LockoutEnabled = lockoutEnabled;
            LockoutEndDateUtc = lockoutEndDateUtc;
            NumberUnblockRequests = numberUnblockRequests;
        }
        public string Id { get; }
        public string Email { get; }
        public string UserName { get; }
        public bool LockoutEnabled { get; }
        public DateTime? LockoutEndDateUtc { get; }
        public int NumberUnblockRequests { get; }
    }
}
