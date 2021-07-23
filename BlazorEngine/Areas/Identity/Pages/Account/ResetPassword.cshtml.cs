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
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace blazordemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public ResetPasswordModel(UserManager<blazordemoUser> userManager, IEmailSender emailSender)
        {
            _identityLibrary = new IdentityLibrary(ContentType.ResetPassword, this, userManager, emailSender, null);
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            string l_sEmail = HttpContext.Request.Query["userId"];

            if (code == null) return BadRequest("A code must be supplied for password reset.");

            Input = new InputModel
            {
                Email = l_sEmail,
                Code = code
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            return await _identityLibrary.OnPostAsync(null, Input.Code, Input.Email, Input.Password, false);
        }
    }
}
