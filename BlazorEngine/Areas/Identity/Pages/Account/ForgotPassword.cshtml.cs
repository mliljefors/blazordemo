using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using blazordemo.Areas.Identity.Data;
using static blazordemo.Areas.Identity.IdentityLibrary;
using Microsoft.AspNetCore.Components.Forms;

namespace blazordemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public ForgotPasswordModel(UserManager<blazordemoUser> userManager, IEmailSender emailSender)
        {
            _identityLibrary = new IdentityLibrary(ContentType.ForgotPassword, this, userManager, emailSender, null);
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {            
            return await _identityLibrary.OnPostAsync(null, null, Input.Email, null, (Request.Form["button2"].Count == 1));
        }
    }
}
