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
    public class LoginModel : PageModel
    {
        private readonly IdentityLibrary _identityLibrary;

        public LoginModel(SignInManager<blazordemoUser> signInManager, UserManager<blazordemoUser> userManager)
        {
            _identityLibrary = new IdentityLibrary(ContentType.Login, this, userManager, null, signInManager);
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            await _identityLibrary.OnGetAsync(returnUrl, ErrorMessage, null);

            ReturnUrl = _identityLibrary.Result;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
           return await _identityLibrary.OnPostAsync(null, null, Input.Email, Input.Password, Input.RememberMe);
        }
    }
}
