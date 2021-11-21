using Library.Models.Entities;
using System;
using System.Threading.Tasks;

namespace Library.Services.Clients.Database.Repositories
{
    public interface ISessionRepository
    {
        Task<Session> CreateSession(Guid userId);
        Task<Session> GetUserSession(Guid userId, Guid sessionId);
    }
}