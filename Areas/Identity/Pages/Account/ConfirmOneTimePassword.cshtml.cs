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
    public class ConfirmOneTimePassword : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public ConfirmOneTimePassword(SignInManager<blazordemoUser> signInManager, UserManager<blazordemoUser> userManager)
        {
            _identityLibrary = new IdentityLibrary(ContentType.ConfirmOneTimePassword, this, userManager, null, signInManager);
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            return await _identityLibrary.OnGetAsync(null, code, userId);
        }
    }
}
