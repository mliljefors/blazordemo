using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using blazordemo.Areas.Identity.Data;
using blazordemo.Data;
using System.Text.Encodings.Web;

namespace blazordemo.Areas.Identity
{
    public class IdentityLibrary
    {
        public enum ContentType
        {
            ForgotPassword,
            ConfirmEmail,
            ChangedPassword,
        }

        public UserManager<blazordemoUser> UserManager { get; }
        private readonly IEmailSender _emailSender;

        public IdentityLibrary(UserManager<blazordemoUser> userManager, IEmailSender emailSender)
        {
            UserManager = userManager;
            _emailSender = emailSender;
        }

        public async Task SendEmail(ContentType contentType, string i_sEmail, string i_sCallbackURL)
        {
            SQLiteDBContext l_pContext = new SQLiteDBContext();
            EmailContent l_pContent = l_pContext.EmailContents.Single(s => s.Type == contentType.ToString());

            await _emailSender.SendEmailAsync(
                i_sEmail,
                l_pContent.Subject,
                l_pContent.Body + ((i_sCallbackURL != null) ? "<br>" + $"<a href='{HtmlEncoder.Default.Encode(i_sCallbackURL)}'>Reset Password</a>" : ""));
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code) // ConfirmEmail
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return Page();
        }

        public async Task OnGetAsync(string returnUrl = null) // Login
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task OnGetAsync(string returnUrl = null) // Register
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null) // RegisterConfirmation
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            DisplayConfirmAccountLink = false;

            return Page();
        }

        public IActionResult OnGet(string code = null) // ResetPassword
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }



        public async Task<IActionResult> OnPostAsync() // ForgotPassword
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

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) // Login
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null) // Logout
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) // Register
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new blazordemoUser { UserName = Input.Email, Email = Input.Email };
                var result = await _identityLibrary.UserManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _identityLibrary.UserManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _identityLibrary.SendEmail(IdentityLibrary.ContentType.ConfirmEmail, Input.Email, callbackUrl);

                    if (_identityLibrary.UserManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync() // ResetPassword
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _identityLibrary.UserManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _identityLibrary.UserManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                await _identityLibrary.SendEmail(IdentityLibrary.ContentType.ChangedPassword, Input.Email, null);

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
