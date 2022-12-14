using EventHub.EventManagement.Application.Contracts.Infrastructure;
using EventHub.EventManagement.Application.DTOs;
using EventHub.EventManagement.Application.Models;
using EventHub.EventManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace EventHub.EventManagement.Presentation
{
   [Route("api/accounts")]
   [ApiController]
   public class AccountsController : ControllerBase
   {
      private readonly UserManager<User> _userManager;
      private readonly IEmailSender _emailSender;

      public AccountsController(UserManager<User> userManager, IEmailSender emailSender)
      {
         _userManager = userManager;
         _emailSender = emailSender;
      }

      [HttpPost("forgotPassword")]
      public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
      {
         if (!ModelState.IsValid)
            return BadRequest(ModelState);

         var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email!);
         if (user is null)
         {
            return NotFound();
         }

         var token = await _userManager.GeneratePasswordResetTokenAsync(user);
         var param = new Dictionary<string, string?>
         {
            {"token", token },
            {"email", forgotPasswordDto.Email }
         };

         var redirectURI = QueryHelpers.AddQueryString(forgotPasswordDto.ResetPasswordClientURI!, param);
         var email = new Email(new List<string> { forgotPasswordDto.Email! }, "reset password token", redirectURI);
         await _emailSender.SendEmailAsync(email);

         return Ok();

      }
   }
}

