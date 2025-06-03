using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using TukaranWebApp.Data;
using TukaranWebApp.Models;

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

        // [HttpGet]
        // public IActionResult Login() => View();

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

            // var redirectUrl = Url.Action("GoogleResponse");
            // var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            // return Challenge(properties, GoogleDefaults.AuthenticationScheme);

            var redirectUrl = Url.Action("GoogleResponse", "Account", null, Request.Scheme);

            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {

            // Mendapatkan info user dari cookie scheme (hasil challenge)
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToAction("Login", "Account"); // jika gagal, redirect ke login biasa

            // Ambil claim user dari Google
            var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims.ToList();

            // Contoh ambil email dan nama user
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            // TODO: Implementasikan logic login / register user di database sesuai email dan nama
            if (email != null)
            {
                var account = await _accountRepository.GetByUsernameAsync(email);
                if (account == null)
                {
                    // Jika user belum terdaftar, bisa buat akun baru
                    account = new Account { Username = email, PasswordHash = "GoogleLogin" }; // Simulasi password hash
                    await _accountRepository.CreateAsync(account);
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    // Simpan informasi user ke cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("SetNewPassword", "Account");
                }
                else if (account.PasswordHash == "GoogleLogin")
                {
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    // Simpan informasi user ke cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    // Jika user sudah terdaftar tapi belum set password, redirect ke SetNewPassword
                    return RedirectToAction("SetNewPassword", "Account");
                }
                else
                {
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    // Simpan informasi user ke cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Home");
                }


            }


            // Jika email tidak ditemukan, redirect ke halaman login dengan pesan error
            ViewBag.Error = "Email tidak terdaftar.";
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var account = await _accountRepository.GetAccountLoginAsync(new Account
            {
                Username = username,
                PasswordHash = password // Simulasi password hash, seharusnya di-hash
            });
            // Cek apakah akun ditemukan
            if (account == null)
            {
                ViewBag.Error = "Invalid credentials";
                return View();
            }

            var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");


        }

        [HttpPost]
        public async Task<IActionResult> SaveNewPassword(string password, string passwordConfirm)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordConfirm))
            {
                ViewBag.Error = "Password tidak boleh kosong";
                return View("SetNewPassword");
            }

            if (password != passwordConfirm)
            {
                ViewBag.Error = "Password tidak cocok";
                return View("SetNewPassword");
            }

            // Simulasi update password, seharusnya di-hash
            var username = User.Identity?.Name;
            if (username == null)
            {
                return RedirectToAction("Login");
            }

            var account = await _accountRepository.GetByUsernameAsync(username);
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            account.PasswordHash = password; // Simulasi update password hash
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
