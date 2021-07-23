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
    public class ConfirmEmailModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public ConfirmEmailModel(UserManager<blazordemoUser> userManager)
        {
            _identityLibrary = new IdentityLibrary(ContentType.ConfirmEmail, this, userManager, null, null);
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            IActionResult l_pResult = await _identityLibrary.OnGetAsync(null, code, userId);

            StatusMessage = _identityLibrary.Result;

            return l_pResult;
        }
    }
}
