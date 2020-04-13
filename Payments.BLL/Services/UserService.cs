using AutoMapper;
using Microsoft.AspNet.Identity;
using NLog;
using Payments.BLL.DTO.Account;
using Payments.BLL.Infrastructure;
using Payments.BLL.Interfaces;
using Service.Interfaces;
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
    public class UserService : IUserService
    {
        private IUnitOfWork database;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UserService(IUnitOfWork unitOfWork)
        {
            database = unitOfWork;
        }

        public async Task<OperationDetails> Create(UserDTO userDTO)
        {
            try
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
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return new OperationDetails(false, "Пользователь с таким именем уже существует", "Email");
        }

        public async Task<ClaimsIdentity> Authenticate(UserDTO userDTO)
        {
            ClaimsIdentity claim = null;
            try
            {
                ApplicationUser user = await database.UserManager.FindAsync(userDTO.Email, userDTO.Password);
                if (user != null)
                {
                    claim = await database.UserManager.CreateIdentityAsync(user,
                        DefaultAuthenticationTypes.ApplicationCookie);
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return claim;
        }

        public async Task<bool> IsLockedOutAsync(string userId)
        {
            try
            {
                return await database.UserManager.IsLockedOutAsync(userId);
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
                return true;
            }
        }

        public List<DataUserForAdminDTO> GetUsers()
        {
            List<DataUserForAdminDTO> usersDTO = null;
            try
            {
                var users = database.UserManager.Users.ToList();
                var mapper = new MapperConfiguration(cfg => cfg.CreateMap<ApplicationUser,
                  DataUserForAdminDTO>()).CreateMapper();
                usersDTO = mapper.Map<List<ApplicationUser>,
                     List<DataUserForAdminDTO>>(users);
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return usersDTO;
        }

        public async Task<IEnumerable<FullDataUserForAdminDTO>> GetDataAboutUsersAsync(
            List<DataUserForAdminDTO> users, string adminId)
        {
            List<FullDataUserForAdminDTO> dataAboutUsersDTOs =
               new List<FullDataUserForAdminDTO>();
            try
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
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return dataAboutUsersDTOs;
        }

        public async Task<string> FindUserIdAsync(string nameCurrentUser)
        {
            try
            {
                ApplicationUser currentUser = await database.UserManager.FindByNameAsync(nameCurrentUser);
                return currentUser.Id;
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
                return null;
            }
        }

        public async Task<UserBlockDataDTO> FindUserForBlockAsync(string id)
        {
            UserBlockDataDTO userBlockDataDTO = null;
            if (id == null)
            {
                throw new Exception("Не существует пользователя с запрашиваемым id");
            }
            try
            {
                ApplicationUser applicationUser = await database.UserManager.FindByIdAsync(id);
                if (applicationUser == null)
                {
                    throw new Exception("Не существует пользователя с запрашиваемым id");
                }
                else
                {
                    DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
                    if (lockoutEndDate != null)
                    {
                        DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                        lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
                    }
                    userBlockDataDTO = new UserBlockDataDTO()
                    {
                        UserId = applicationUser.Id,
                        Email = applicationUser.Email,
                        LockoutEnabled = applicationUser.LockoutEnabled
                    };
                    if (applicationUser.LockoutEndDateUtc == null)
                    {
                        userBlockDataDTO.DateTimeBlock = DateTime.Now;
                    }
                    else
                    {
                        userBlockDataDTO.DateTimeBlock = (DateTime)lockoutEndDate;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return userBlockDataDTO;
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
            try
            {
                CheckDateBlock(userBlockDataDTO.DateTimeBlock);
                var result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, true);
                if (result.Succeeded)
                {
                    result = await database.UserManager
                        .SetLockoutEndDateAsync(userBlockDataDTO.UserId, (DateTimeOffset)userBlockDataDTO.DateTimeBlock);
                    ApplicationUser applicationUser = await database.UserManager
                        .FindByIdAsync(userBlockDataDTO.UserId);
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public async Task<UserBlockDataDTO> FindUserForUnBlockAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("Не существует пользователя с запрашиваемым id");
            }
            UserBlockDataDTO userBlockDataDTO = null;
            try
            {
                ApplicationUser applicationUser = await database.UserManager.FindByIdAsync(id);
                if (applicationUser == null)
                {
                    throw new Exception("Не существует пользователя с запрашиваемым id");
                }
                else
                {
                    DateTime? lockoutEndDate = applicationUser.LockoutEndDateUtc;
                    if (lockoutEndDate != null)
                    {
                        DateTime localLockoutEndDate = (DateTime)lockoutEndDate;
                        lockoutEndDate = (DateTime?)localLockoutEndDate.AddHours(3);
                    }
                    userBlockDataDTO = new UserBlockDataDTO()
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
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
            return userBlockDataDTO;
        }

        public async Task UnblockUserAccount(UserBlockDataDTO userBlockDataDTO)
        {
            try
            {
                var result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, true);
                if (result.Succeeded)
                {
                    result = await database.UserManager.SetLockoutEndDateAsync(userBlockDataDTO.UserId, DateTimeOffset.UtcNow);
                    result = await database.UserManager.SetLockoutEnabledAsync(userBlockDataDTO.UserId, false);
                    ApplicationUser applicationUser = database.UserManager.FindById(userBlockDataDTO.UserId);
                }
            }
            catch (Exception exception)
            {
                logger.Error($"{exception.Message} {exception.StackTrace}");
            }
        }

        public void Dispose()
        {
            database.Dispose();
        }
    }
}
