using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Library.Services.Clients.Database.Repositories;
using System;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _users;
        
        public UsersController(UserRepository userSettings)
        {
            _users = userSettings;
        }

        [HttpGet]
        [CustomAuth]
        [ActionName("getUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _users.GetUsers();
            return Ok(users);
        }

        [HttpPost]
        [CustomAuth]
        [ActionName("createUser")]
        public async Task<IActionResult> CreateUser(string userEmail)
        {
            await _users.CreateUser(userEmail);
            // TODO: send email to user 
            return Ok();
        }

        [HttpDelete]
        [CustomAuth]
        [ActionName("deleteUser")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _users.DeleteUser(userId);
            return Ok();
        }
    }
}
