using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using blazordemo.Areas.Identity.Data;
using blazordemo.Data;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace blazordemo.Areas.Identity
{
    public class IdentityLibrary
    {
        public enum ContentType
        {
            Login,
            Logout,

            Register,
            RegisterConfirmation,
            ConfirmEmail,

            ForgotPassword,
            ForgotPasswordConfirmation,
            ResetPassword,
            ResetPasswordConfirmation,
        }

        public enum ResultType
        {
            none,

            RedirectToPage,
            NotFound,
            LocalRedirect,
            Page,
        }

        private readonly ContentType _contentType;
        private readonly PageModel _pageModel;
        private readonly UserManager<blazordemoUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<blazordemoUser> _signInManager;

        public string Result { get; set; }

        public IdentityLibrary(ContentType contentType, 
            PageModel pageModel, 
            UserManager<blazordemoUser> userManager, 
            IEmailSender emailSender,
            SignInManager<blazordemoUser> signInManager)
        {
            _contentType = contentType;
            _pageModel = pageModel;
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public async Task SendEmail(string i_sEmail, string i_sCallbackURL)
        {   
            // read email content from database

            SQLiteDBContext l_pContext = new SQLiteDBContext();
            EmailContent l_pContent = l_pContext.EmailContents.Single(s => s.Type == _contentType.ToString());

            // send email with content

            await _emailSender.SendEmailAsync(
                i_sEmail,
                l_pContent.Subject,
                l_pContent.Body + ((i_sCallbackURL != null) ? "<br>" + $"<a href='{HtmlEncoder.Default.Encode(i_sCallbackURL)}'>Reset Password</a>" : ""));
        }

        public async Task<IActionResult> OnGetAsync(string i_sReturnURL, string i_sCode, string i_sEmail)
        {
            blazordemoUser l_pUser;
            IdentityResult l_pResult;

            Result = null;

            switch (_contentType)
            {
                case ContentType.Login: // requires : UserID, Code

                    if (!string.IsNullOrEmpty(i_sCode))
                    {
                        _pageModel.ModelState.AddModelError(string.Empty, i_sCode);
                    }

                    i_sReturnURL ??= _pageModel.Url.Content("~/");

                    // clear the existing cookie
                    await _pageModel.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                    Result = i_sReturnURL;

                    break;

                case ContentType.ConfirmEmail: // requires : Email, Code

                    if (i_sEmail == null || i_sCode == null) return _pageModel.RedirectToPage("/Index");

                    // find specified user by email

                    l_pUser = await _userManager.FindByIdAsync(i_sEmail);
                    if (l_pUser == null) return _pageModel.NotFound($"Unable to load user with ID '{i_sEmail}'.");
                    
                    // confirm the code that was sent via email link URL

                    i_sCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(i_sCode));
                    l_pResult = await _userManager.ConfirmEmailAsync(l_pUser, i_sCode);
                    
                    Result = l_pResult.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
                
                break;

                case ContentType.Register: // requires : ReturnURL

                    Result = i_sReturnURL;

                    break;

                case ContentType.RegisterConfirmation: // requires : Email

                    if (i_sEmail == null) return _pageModel.RedirectToPage("/Index");

                    // find specified user by email

                    l_pUser = await _userManager.FindByEmailAsync(i_sEmail);
                    if (l_pUser == null) return _pageModel.NotFound($"Unable to load user with ID '{i_sEmail}'.");
                    
                break;
            }

            return _pageModel.Page();
        }

        public async Task<IActionResult> OnPostAsync(string i_sReturnURL, string i_sCode, string i_sEmail, string i_sPassword, bool i_bRemember)
        {
            blazordemoUser l_pUser;
            IdentityResult l_pResult;

            Result = null;

            if (_pageModel.ModelState.IsValid)
            {
                switch (_contentType)
                {
                    case ContentType.Login: // requires : ReturnURL, Email, Password, Remember

                        i_sReturnURL ??= _pageModel.Url.Content("~/");

                        // sign in the specified user

                        var signInResult = await _signInManager.PasswordSignInAsync(i_sEmail, i_sPassword, i_bRemember, lockoutOnFailure: false);
                        
                        if (signInResult.Succeeded)
                        {
                            return _pageModel.LocalRedirect(i_sReturnURL);
                        }
                        if (signInResult.IsLockedOut)
                        {
                            return _pageModel.RedirectToPage("./Lockout");
                        }
                        else
                        {
                            _pageModel.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }

                        break;

                    case ContentType.Logout: // requires : ReturnURL

                        // sign out the current user

                        await _signInManager.SignOutAsync();

                        if (i_sReturnURL != null)
                        {
                            return _pageModel.LocalRedirect(i_sReturnURL);
                        }
                        else
                        {
                            return _pageModel.RedirectToPage();
                        }

                    case ContentType.Register: // requires : ReturnURL, Email, Password

                        i_sReturnURL ??= _pageModel.Url.Content("~/");

                        // create the specified user

                        l_pUser = new blazordemoUser { UserName = i_sEmail, Email = i_sEmail };
                        l_pResult = await _userManager.CreateAsync(l_pUser, i_sPassword);

                        if (l_pResult.Succeeded)
                        {
                            // send email with registration confirmation link

                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(l_pUser);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = _pageModel.Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = l_pUser.Id, code = code, returnUrl = i_sReturnURL },
                                protocol: _pageModel.Request.Scheme);

                            await SendEmail(i_sEmail, callbackUrl);

                            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                return _pageModel.RedirectToPage("RegisterConfirmation", new { email = i_sEmail, returnUrl = i_sReturnURL });
                            }
                            else
                            {
                                await _signInManager.SignInAsync(l_pUser, isPersistent: false);
                                return _pageModel.LocalRedirect(i_sReturnURL);
                            }
                        }
                        
                        // display any errors

                        foreach (var error in l_pResult.Errors)
                        {
                            _pageModel.ModelState.AddModelError(string.Empty, error.Description);
                        }

                        break;

                    case ContentType.ForgotPassword: // requires : Email

                        // find specified user by email

                        l_pUser = await _userManager.FindByEmailAsync(i_sEmail);

                        if (l_pUser != null && await _userManager.IsEmailConfirmedAsync(l_pUser))
                        {
                            // send email with generated reset password link

                            var code = await _userManager.GeneratePasswordResetTokenAsync(l_pUser);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = _pageModel.Url.Page("/Account/ResetPassword", pageHandler: null, values: new { area = "Identity", code }, protocol: _pageModel.Request.Scheme);

                            await SendEmail(i_sEmail, callbackUrl);
                        }

                        return _pageModel.RedirectToPage("./ForgotPasswordConfirmation");

                    case ContentType.ResetPassword: // requires : Email, Password, Code

                        // find specified user by email
                        // even if the user does not exist, redirect to confirmation page anyways

                        l_pUser = await _userManager.FindByEmailAsync(i_sEmail);
                        if (l_pUser == null) return _pageModel.RedirectToPage("./ResetPasswordConfirmation");
                        
                        // reset password

                        l_pResult = await _userManager.ResetPasswordAsync(l_pUser, i_sCode, i_sPassword);
                        if (l_pResult.Succeeded)
                        {
                            // send email with confirmation of password change

                            await SendEmail(i_sEmail, null);

                            return _pageModel.RedirectToPage("./ResetPasswordConfirmation");
                        }

                        // display any errors

                        foreach (var error in l_pResult.Errors)
                        {
                            _pageModel.ModelState.AddModelError(string.Empty, error.Description);
                        }

                        break;
                }
            }

            return _pageModel.Page();
        }
    }
}
