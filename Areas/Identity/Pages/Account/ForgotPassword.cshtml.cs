using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using blazordemo.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using blazordemo.Data;
using System.Linq;

namespace blazordemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public ForgotPasswordModel(UserManager<blazordemoUser> userManager, IEmailSender emailSender)
        {
            _identityLibrary = new IdentityLibrary(userManager, emailSender);
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
            if (ModelState.IsValid)
            {
                var user = await _identityLibrary.UserManager.FindByEmailAsync(Input.Email);
                if (user != null && await _identityLibrary.UserManager.IsEmailConfirmedAsync(user))
                {
                    var code = await _identityLibrary.UserManager.GeneratePasswordResetTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ResetPassword",
                        pageHandler: null,
                        values: new { area = "Identity", code },
                        protocol: Request.Scheme);

                    await _identityLibrary.SendEmail(IdentityLibrary.ContentType.ForgotPassword, Input.Email, callbackUrl);
                }

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
