using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using MVC.Models;

namespace MVC.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;

        public AccountController(UserManager<User> um, SignInManager<User> sm)
        {
            userManager = um;
            signInManager = sm;
        }

        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        errors[entry.Key] = entry.Value.Errors.First().ErrorMessage;
                    }
                }
                return Json(new { success = false, errors = errors });
            }

            var result = await signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password,
                    isPersistent: loginViewModel.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { success = false, errors = new { Password = "Invalid username or password" } });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string>();
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        errors[entry.Key] = entry.Value.Errors.First().ErrorMessage;
                    }
                }
                return Json(new { success = false, errors = errors });
            }

            User user = new()
            {
                Name = registerViewModel.Name,
                UserName = registerViewModel.UserName,
                Address = registerViewModel.Address
            };

            var result = await userManager.CreateAsync(user, registerViewModel.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }

            var resultErrors = new Dictionary<string, string>();
            foreach (var error in result.Errors)
            {
                resultErrors["General"] = error.Description;
            }
            return Json(new { success = false, errors = resultErrors });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }
    }

}
