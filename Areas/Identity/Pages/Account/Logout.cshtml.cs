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
    public class LogoutModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public LogoutModel(SignInManager<blazordemoUser> signInManager)
        {
            _identityLibrary = new IdentityLibrary(ContentType.Logout, this, null, null, signInManager);
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            return await _identityLibrary.OnPostAsync(returnUrl, null, null, null, null, false);
        }
    }
}
