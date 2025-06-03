using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TukaranWebApp.Data;
using TukaranWebApp.Models;
using BCrypt.Net;

namespace TukaranWebApp.Controllers
{
    // [Route("[controller]")]
    public class AccountController : Controller
    {

        private readonly ILogger<AccountController> _logger;
        private readonly IAccountRepository _accountRepository;


        public AccountController(ILogger<AccountController> logger, IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            _logger = logger;
        }

    
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SetNewPassword()
        {
            return View();
        }

        public IActionResult SignInGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                if (!authenticateResult.Succeeded)
                    return RedirectToAction("Login", "Account"); // jika gagal, redirect ke login biasa

                // Ambil claim user dari Google
                var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims.ToList();

                // ambil email dan nama user
                var account = new Account();
                account.Username = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var claims_ = new List<Claim> { new Claim(ClaimTypes.Name, account.Username) };
                var identity = new ClaimsIdentity(claims_, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                var acc = await _accountRepository.CheckLoginGoogle(account);
                if (acc.PasswordHash == "Google Login")
                {
                    return RedirectToAction("SetNewPassword", "Account");
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception  ex)
            {
                // Log error
                 ViewBag.Error = "An unexpected error occurred. Please contact admin.";
                 ViewData["ServerError"] = "An unexpected error occurred. Please contact admin.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        { 
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Username and password are required";
                    ViewData["ServerError"] = "Username and password are required";
                    return View();
                }
                var res = await _accountRepository.GetByUsernameAsync(username);
                // Cek apakah akun ditemukan
                if (res == null)
                {
                    ViewBag.Error = "Account not found";
                    ViewData["ServerError"] = "Account not found";
                    return View();
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, res.PasswordHash);
                
                if (!isPasswordValid)
                {
                    ViewBag.Error = "Invalid credentials";
                    ViewData["ServerError"] = "Invalid credentials";
                    return View();
                }

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, res.Username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception  ex)
            {
                // Log error
                ViewBag.Error = "An unexpected error occurred. Please contact admin.";
                ViewData["ServerError"] = "An unexpected error occurred. Please contact admin.";
                return View("Login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetNewPassword(string password,string passwordConfirm)
        {
            var username = User.Identity?.Name;
            if (username == null)
            {
                return RedirectToAction("Login");
            }

            var account = await _accountRepository.GetByUsernameAsync(username);
            if (account == null)
            {
                ViewBag.Error = "Account not found";
                ViewData["ServerError"] = "Account not found";
                return View();
            }
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
            {
                ViewBag.Error = "Password and confirmation are required";
                return View();
            }
            if (password != passwordConfirm)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }
            if (password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long";
                return View();
            }
            
            // Hash the password before saving
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _accountRepository.UpdateAsync(account);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


    }
}
