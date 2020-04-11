using Payments.BLL.DTO.Account;
using Payments.BLL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Interfaces
{
    public interface IUserService : IDisposable
    {
        Task<OperationDetails> Create(UserDTO userDTO);
        Task<ClaimsIdentity> Authenticate(UserDTO userDTO);
        Task<bool> IsLockedOutAsync(string userId);
        List<DataUserForAdminDTO> GetUsers();
        Task<IEnumerable<FullDataUserForAdminDTO>> GetDataAboutUsersAsync(
            List<DataUserForAdminDTO> users, string adminId);
        Task<string> FindUserIdAsync(string nameCurrentUser);
        Task<UserBlockDataDTO> FindUserForBlockAsync(string id);
        void CheckDateBlock(DateTime dateBlock);
        Task BlockUserAccount(UserBlockDataDTO userBlockDataDTO);
        Task<UserBlockDataDTO> FindUserForUnBlockAsync(string id);
        Task UnblockUserAccount(UserBlockDataDTO userBlockDataDTO);

    }
}
