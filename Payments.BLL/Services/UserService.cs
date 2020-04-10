using AutoMapper;
using Microsoft.AspNet.Identity;
using Payments.BLL.DTO.Account;
using Payments.BLL.Infrastructure;
using Service.Models;
using Service.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Payments.BLL.Services
{
    // Сервис по работе с учетными данными пользователей
    public class UserService : IDisposable
    {
        private UnitOfWork database;

        public UserService()
        {
            database = new UnitOfWork();
        }

        public async Task<OperationDetails> Create(UserDTO userDTO)
        {
            ApplicationUser user = await database.UserManager.FindByEmailAsync(userDTO.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = userDTO.Email,
                    UserName = userDTO.Email
                };
                var result = await database.UserManager.CreateAsync(user, userDTO.Password);
                if (result.Errors.Count() > 0)
                {
                    return new OperationDetails(false, result.Errors.FirstOrDefault(), "");
                }
                await database.UserManager.AddToRoleAsync(user.Id, userDTO.Role);
                await database.SaveAsync();
                return new OperationDetails(true, "Регистрация успешно пройдена", "");
            }
            else
            {
                return new OperationDetails(false, "Пользователь с таким именем уже существует", "Email");
            }
        }

        public async Task<ClaimsIdentity> Authenticate(UserDTO userDTO)
        {
            ClaimsIdentity claim = null;
            ApplicationUser user = await database.UserManager.FindAsync(userDTO.Email, userDTO.Password);
            if (user != null)
            {
                claim = await database.UserManager.CreateIdentityAsync(user,
                    DefaultAuthenticationTypes.ApplicationCookie);
            }
            return claim;
        }

        public async Task<bool> IsLockedOutAsync(string userId)
        {
            return await database.UserManager.IsLockedOutAsync(userId);
        }

        public List<DataUserForAdminDTO> GetUsers()
        {
            var users = database.UserManager.Users.ToList();
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<ApplicationUser,
              DataUserForAdminDTO>()).CreateMapper();
            var usersDTO = mapper.Map<List<ApplicationUser>,
                 List<DataUserForAdminDTO>>(users);
            return usersDTO;
        }

        public async Task<IEnumerable<FullDataUserForAdminDTO>> GetDataAboutUsersAsync(
            List<DataUserForAdminDTO> users, string adminId)
        {
            IEnumerable<BankAccount> bankAccountsUser = null;
            IEnumerable<BankAccount> bankAccountsUserRequestUnblock = null;
            List<int> countBankAccountsUser = new List<int>();
            foreach (var item in users)
            {
                bankAccountsUser = await database.BankAccounts
                .FindAllAsync(p => p.ApplicationUserId == item.Id);
                bankAccountsUserRequestUnblock = bankAccountsUser.Where(p => p.RequestUnblock == true);
                countBankAccountsUser.Add(bankAccountsUserRequestUnblock.Count());
            }
            int i = 0;
            DateTime? lockoutEndDate = null;
            List<FullDataUserForAdminDTO> dataAboutUsersDTOs =
                new List<FullDataUserForAdminDTO>();
            foreach (var item in users)
            {
                lockoutEndDate = item.LockoutEndDateUtc;
                if (lockoutEndDate != null)
                {
                    DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                    lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
                }
                if (item.Id != adminId)
                {
                    dataAboutUsersDTOs.Add(new FullDataUserForAdminDTO(
                        item.Id, item.Email, item.UserName, item.LockoutEnabled,
                        lockoutEndDate, countBankAccountsUser[i]));
                }
                i++;
            }
            return dataAboutUsersDTOs;
        }

        public async Task<string> FindUserIdAsync(string nameCurrentUser)
        {
            ApplicationUser currentUser = await database.UserManager.FindByNameAsync(nameCurrentUser);
            return currentUser.Id;
        }

        public async Task<UserBlockDataDTO> FindUserForBlockAsync(string id)
        {
            ApplicationUser applicationUser = await database.UserManager.FindByIdAsync(id);
            DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
            if (lockoutEndDate != null)
            {
                DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
            }
            UserBlockDataDTO userBlockData = new UserBlockDataDTO()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockData.DateTimeBlock = DateTime.Now;
            }
            else
            {
                userBlockData.DateTimeBlock = (DateTime)lockoutEndDate;
            }
            return userBlockData;
        }

        public void CheckDateBlock(DateTime dateBlock)
        {
            if (dateBlock <= DateTime.Now)
            {
                throw new ValidationBusinessLogicException("Дата, до которой " +
                    "будет заблокирован пользователь должна быть в будущем времени", "DateTimeBlock");
            }
        }

        public async Task BlockUserAccount(UserBlockDataDTO userBlockDataDTO)
        {
            var result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, true);
            if (result.Succeeded)
            {
                result = await database.UserManager
                    .SetLockoutEndDateAsync(userBlockDataDTO.UserId, (DateTimeOffset)userBlockDataDTO.DateTimeBlock);
                ApplicationUser applicationUser = await database.UserManager
                    .FindByIdAsync(userBlockDataDTO.UserId);
            }
        }

        public async Task<UserBlockDataDTO> FindUserForUnBlockAsync(string id)
        {
            ApplicationUser applicationUser = await database.UserManager.FindByIdAsync(id);
            DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
            if (lockoutEndDate != null)
            {
                DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
            }
            UserBlockDataDTO userBlockDataDTO = new UserBlockDataDTO()
            {
                UserId = applicationUser.Id,
                Email = applicationUser.Email,
                LockoutEnabled = applicationUser.LockoutEnabled,
            };
            if (applicationUser.LockoutEndDateUtc == null)
            {
                userBlockDataDTO.DateTimeBlock = DateTime.Now;
            }
            else
            {
                userBlockDataDTO.DateTimeBlock = (DateTime)lockoutEndDate;
            }
            return userBlockDataDTO;
        }

        public async Task UnblockUserAccount(UserBlockDataDTO userBlockDataDTO)
        {
            var result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, true);
            if (result.Succeeded)
            {
                result = await database.UserManager.SetLockoutEndDateAsync(userBlockDataDTO.UserId, DateTimeOffset.UtcNow);
                result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, false);
                ApplicationUser applicationUser = database.UserManager.FindById(userBlockDataDTO.UserId);
            }
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
