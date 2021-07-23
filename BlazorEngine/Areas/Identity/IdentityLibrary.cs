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
        private const string IndexPage = "/_Host";

        public enum ContentType
        {
            Login,
            Logout,

            Register,
            RegisterConfirmation,
            ConfirmEmail,

            OneTimePassword,
            OneTimePasswordConfirmation,
            ConfirmOneTimePassword,

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

        private string GenerateOneTimePassword(string i_sUserID)
        {
            string l_sIdentifier;

            // generate random number password

            l_sIdentifier = (new Random()).Next(100000, 999999).ToString();

            return l_sIdentifier;
        }

        private async Task CreateOrUpdateOneTimePassword(bool i_bIsCreate, string i_sUserID, string i_sIdentifier, int i_iStatus)
        {
            SQLiteDBContext l_pContext = new SQLiteDBContext();
            OneTimePassword l_pOneTimePassword;

            if (i_bIsCreate)
            {
                // create OneTimePassword in database

                l_pOneTimePassword = new OneTimePassword();
                l_pContext.OneTimePasswords.Add(l_pOneTimePassword);

                List<int> l_pIds = l_pContext.OneTimePasswords.Select(x => x.Id).ToList();
                l_pOneTimePassword.Id = 1 + ((l_pIds.Count > 0) ? Int32.Parse(l_pIds.Max().ToString()) : 0);
                l_pOneTimePassword.UserId = i_sUserID;
                l_pOneTimePassword.Identifier = i_sIdentifier;
                l_pOneTimePassword.TimeStamp = System.DateTime.Now.ToString();
            }
            else
            {
                // find OneTimePassword in database

                l_pOneTimePassword = l_pContext.OneTimePasswords.Single(s => (s.UserId == i_sUserID) && (s.Identifier == i_sIdentifier));
            }

            l_pOneTimePassword.Status = i_iStatus;

            await l_pContext.SaveChangesAsync();
        }

        private bool ValidateOneTimePassword(string i_sUserID, string i_sIdentifier)
        {
            bool l_bValid = false;
            SQLiteDBContext l_pContext = new SQLiteDBContext();

            // check that the password exists in database

            if (l_pContext.OneTimePasswords.Count(s => (s.UserId == i_sUserID) && (s.Identifier == i_sIdentifier) && (s.Status == 0)) > 0)
            {
                l_bValid = true;
            }

            return l_bValid;
        }

        public async Task SendEmail(ContentType i_iType, string i_sEmail, string i_sCallbackURL)
        {
            // read email content from database

            SQLiteDBContext l_pContext = new SQLiteDBContext();
            EmailContent l_pContent = l_pContext.EmailContents.Single(s => s.Type == i_iType.ToString());

            // send email with content

            await _emailSender.SendEmailAsync(
                i_sEmail,
                l_pContent.Subject,
                l_pContent.Body + ((i_sCallbackURL != null) ? "<br>" + $"<a href='{HtmlEncoder.Default.Encode(i_sCallbackURL)}'>" + l_pContent.Subject + "</a>" : ""));
        }

        public async Task<blazordemoUser> FindUserByID(string i_sUserID, string i_sEmail)
        {
            blazordemoUser l_pUser;
            bool l_bFindByUser = (i_sUserID != null);

            if (l_bFindByUser ? (i_sUserID == null) : (i_sEmail == null)) return null;

            // find specified user by email
            
            l_pUser = l_bFindByUser ? await _userManager.FindByIdAsync(i_sUserID) : await _userManager.FindByEmailAsync(i_sEmail);
            if (l_pUser == null) return null;

            return l_pUser;
        }

        public async Task<IActionResult> SignInAsync(blazordemoUser i_pUser, string i_sReturnURL)
        {
            await _signInManager.SignInAsync(i_pUser, isPersistent: false);

            return _pageModel.LocalRedirect(i_sReturnURL);
        }

        public async Task<IActionResult> OnGetAsync(string i_sReturnURL, string i_sCode, string i_sEmail)
        {
            blazordemoUser l_pUser;
            IdentityResult l_pResult;

            Result = null;

            i_sReturnURL ??= _pageModel.Url.Content("~/");

            switch (_contentType)
            {
                case ContentType.Login: // requires : Email, Code

                    if (!string.IsNullOrEmpty(i_sCode))
                    {
                        _pageModel.ModelState.AddModelError(string.Empty, i_sCode);
                    }

                    // clear the existing cookie

                    await _pageModel.HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                    Result = i_sReturnURL;

                    break;

                case ContentType.ConfirmEmail: // requires : Email, Code

                    if (i_sEmail == null || i_sCode == null) return _pageModel.RedirectToPage(IndexPage);

                    // find the specified user

                    if((l_pUser = await FindUserByID(i_sEmail, null)) == null) return _pageModel.NotFound($"Unable to load user with ID '{i_sEmail}'.");

                    // validate the token

                    i_sCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(i_sCode));
                    l_pResult = await _userManager.ConfirmEmailAsync(l_pUser, i_sCode);
                    
                    Result = l_pResult.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
                
                break;

                case ContentType.ConfirmOneTimePassword: // requires : Email, Code

                    if (i_sEmail == null || i_sCode == null) return _pageModel.RedirectToPage(IndexPage);

                    // find the specified user

                    if ((l_pUser = await FindUserByID(i_sEmail, null)) == null) return _pageModel.NotFound($"Unable to load user with ID '{i_sEmail}'.");

                    // validate the OTP code

                    i_sCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(i_sCode));

                    if (ValidateOneTimePassword(i_sEmail, i_sCode))
                    {
                        // update the OTP record Status in the database

                        await CreateOrUpdateOneTimePassword(false, i_sEmail, i_sCode, 1);

                        // sign the user in

                        return await SignInAsync(l_pUser, i_sReturnURL);
                    }
                    else
                    {
                        // validation failed, reset to top page

                        return _pageModel.RedirectToPage(IndexPage);
                    }

                case ContentType.Register: // requires : ReturnURL

                    Result = i_sReturnURL;

                    break;

                case ContentType.RegisterConfirmation: // requires : Email
                    
                    if (i_sEmail == null) return _pageModel.RedirectToPage(IndexPage);

                    // find the specified user by email
                    
                    if ((l_pUser = await FindUserByID(null, i_sEmail)) == null) return _pageModel.NotFound($"Unable to load user with email '{i_sEmail}'.");

                    break;
            }

            return _pageModel.Page();
        }

        public async Task<IActionResult> OnPostAsync(string i_sReturnURL, string i_sCode, string i_sEmail, string i_sPassword, bool i_bChecked)
        {
            blazordemoUser l_pUser;
            IdentityResult l_pResult;

            Result = null;

            if (_pageModel.ModelState.IsValid)
            {
                switch (_contentType)
                {
                    case ContentType.Login: // requires : ReturnURL, Email, Password, Checked

                        i_sReturnURL ??= _pageModel.Url.Content("~/");

                        // sign in the specified user

                        var signInResult = await _signInManager.PasswordSignInAsync(i_sEmail, i_sPassword, i_bChecked, lockoutOnFailure: false);
                        
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

                            await SendEmail(_contentType, i_sEmail, callbackUrl);

                            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                return _pageModel.RedirectToPage("RegisterConfirmation", new { email = i_sEmail, returnUrl = i_sReturnURL });
                            }
                            else
                            {
                                return await SignInAsync(l_pUser, i_sReturnURL);
                            }
                        }
                        
                        // display any errors

                        foreach (var error in l_pResult.Errors)
                        {
                            _pageModel.ModelState.AddModelError(string.Empty, error.Description);
                        }

                        break;

                    case ContentType.ForgotPassword: // requires : Email, Checked

                        // find specified user by email

                        l_pUser = await _userManager.FindByEmailAsync(i_sEmail);

                        // check that the user is registered
                        // even if the user is not registered, redirect to confirmation page anyways

                        if (l_pUser != null && await _userManager.IsEmailConfirmedAsync(l_pUser))
                        {
                            string code, callbackUrl;

                            if (i_bChecked)
                            {
                                // generate and store one-time password

                                code = GenerateOneTimePassword(l_pUser.Id);
                                await CreateOrUpdateOneTimePassword(true, l_pUser.Id, code, 0);
                            }
                            else
                            {
                                // generate reset token

;                                code = await _userManager.GeneratePasswordResetTokenAsync(l_pUser);
                            }

                            // encode the code

                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                            // generate link for either reset password OR one-time password

                            if (i_bChecked)
                            {
                                callbackUrl = _pageModel.Url.Page("/Account/ConfirmOneTimePassword", pageHandler: null, values: new { area = "Identity", userId = l_pUser.Id, code }, protocol: _pageModel.Request.Scheme);
                                
                            }
                            else
                            {
                                callbackUrl = _pageModel.Url.Page("/Account/ResetPassword", pageHandler: null, values: new { area = "Identity", code }, protocol: _pageModel.Request.Scheme);
                            }

                            // send email

                            await SendEmail(i_bChecked ? ContentType.OneTimePassword : ContentType.ForgotPassword, i_sEmail, callbackUrl);
                        }

                        return _pageModel.RedirectToPage(i_bChecked ? "./OneTimePasswordConfirmation" : "./ForgotPasswordConfirmation");

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

                            await SendEmail(_contentType, i_sEmail, null);

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
