using Library.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public interface IUserRepository
    {
        Task<string> CreateUser(string username, string password = null);
        Task DeleteUser(Guid userId);
        Task<User> GetUser(string username);
        Task<User> GetUserById(Guid userId);
        Task<IEnumerable<User>> GetUsers();
        Task RecordLogin(Guid userId);
        Task<(string Password, User user)> ResetUserPassword(Guid userId);
    }
}