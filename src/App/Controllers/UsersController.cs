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
        private readonly IUserRepository _users;
        private readonly IEmailClient _email;
        
        public UsersController(
            IUserRepository users, 
            IEmailClient email)
        {
            _users = users;
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
            if (!input.UserName.Contains("@") || !input.UserName.Contains("."))
                return BadRequest("The user's email appears to be invalid");

            var generatedPassword = await _users.CreateUser(input.UserName);
            
            // TODO: prevent users from being created with the same username
            
            await _email.SendUserCreationEmail(
                to: input.UserName,
                password: generatedPassword,
                loginUrl: GetAppUrl());

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

        [HttpPost]
        [CustomAuth]
        [ActionName("resetUserPassword")]
        public async Task<IActionResult> ResetUserPassword(Guid userId)
        {
            var (password, user) = await _users.ResetUserPassword(userId);
            await _email.SendUserCreationEmail(
                to: user.UserName,
                password: password,
                loginUrl: GetAppUrl());
            return Ok();
        }

        private string GetAppUrl() =>
            HostService.GetAppUrlFromAmbientInfo(new Uri(Request.GetEncodedUrl()));
    }
}
