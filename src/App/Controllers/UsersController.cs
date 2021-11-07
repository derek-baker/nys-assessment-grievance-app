using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Library.Services.Clients.Database.Repositories;
using System;
using Library.Models.DataTransfer;
using Library.Email;
using Microsoft.AspNetCore.Http.Extensions;
using Library.Services;

namespace App.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _users;
        private readonly IEmailClient _email;
        
        public UsersController(
            UserRepository userSettings, 
            IEmailClient email)
        {
            _users = userSettings;
            _email = email;
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
        public async Task<IActionResult> CreateUser(UserCreateInput input)
        {
            var generatedPassword = await _users.CreateUser(input.UserName);

            Uri encodedUrl = new Uri(Request.GetEncodedUrl());
            string host = HostService.GetAppUrlFromAmbientInfo(encodedUrl);
            // TO DO: prevent users from being created with the same username
            // TODO: send email to user with password
            await _email.SendUserCreationEmail(
                to: input.UserName,
                password: generatedPassword,
                loginUrl: host);

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

        [HttpDelete]
        [CustomAuth]
        [ActionName("deleteUser")]
        public async Task<IActionResult> ResetUserPassword(Guid userId)
        {
            await _users.ResetUserPassword(userId);
            return Ok();
        }
    }
}
