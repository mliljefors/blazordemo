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

namespace blazordemo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public RegisterConfirmationModel(UserManager<blazordemoUser> userManager, IEmailSender sender)
        {
            _identityLibrary = new IdentityLibrary(ContentType.RegisterConfirmation, this, userManager, sender, null);
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            IActionResult l_pResult = await _identityLibrary.OnGetAsync(returnUrl, null, null, email);

            Email = email; 
            DisplayConfirmAccountLink = false;

            return l_pResult;
        }
    }
}
