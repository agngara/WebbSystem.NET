using System.Security.Claims;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using MVC.Models;

namespace MVC.Controllers
{
    public class AccountController : BaseController
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;

        public AccountController(UserManager<User> um, SignInManager<User> sm)
        {
            userManager = um;
            signInManager = sm;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelStateErrorResponse("Invalid Login"));
            }

            var user = await userManager.FindByNameAsync(loginViewModel.UserName);

            // Om användaren finns, kontrollera om den är deaktiverad
            if (user != null && user.IsActive == false)
            {
                return Json(CreateResponse(
                    success: false,
                    errors: new Dictionary<string, string>
                    {
                        ["Password"] = "This account has been deactivated"
                    },
                    message: "Account is deactivated",
                    notify: true
                ));
            }

            var result = await signInManager.PasswordSignInAsync(
                loginViewModel.UserName,
                loginViewModel.Password,
                isPersistent: loginViewModel.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                return Json(CreateResponse(
                    success: true,
                    message: "Login successful!",
                    redirectUrl: Url.Action("Index", "Home") ?? "/"
                ));
            }

            return Json(CreateResponse(
                success: false,
                errors: new Dictionary<string, string>
                {
                    ["Password"] = "Invalid username or password"
                },
                notify: true
            ));
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelStateErrorResponse("Invalid Account Registration"));
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
                return Json(CreateResponse(
                    success: true,
                    message: "Registration successful!",
                    redirectUrl: Url.Action("Index", "Home") ?? "/",
                    notify: true
                ));
            }

            return Json(CreateResponse(
                success: false,
                errors: result.Errors.ToDictionary(e => "General", e => e.Description),
                notify: true
            ));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var settingsViewModel = new SettingsViewModel()
            {
                Name = user.Name,
                Address = user.Address ?? "",
                IsPrivate = user.IsPrivate ?? false,
            };
            return View(settingsViewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateSettings(SettingsViewModel settingsViewModel)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelStateErrorResponse("Invalid Account Settings"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                return Json(CreateResponse(
                    success: false,
                    message: "User not found",
                    notify: true
                ));
            }

            if (!string.IsNullOrWhiteSpace(settingsViewModel.Password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await userManager.ResetPasswordAsync(user, token, settingsViewModel.Password);

                if (!passwordResult.Succeeded)
                {
                    return Json(CreateResponse(
                        success: false,
                        errors: passwordResult.Errors.ToDictionary(_ => "Password", e => e.Description),
                        notify: true
                    ));
                }
            }

            user.Name = settingsViewModel.Name!;
            user.Address = settingsViewModel.Address;
            user.IsPrivate = settingsViewModel.IsPrivate;

            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Json(CreateResponse(
                    success: false,
                    errors: updateResult.Errors.ToDictionary(_ => "General", e => e.Description),
                    notify: true
                ));
            }

            return Json(CreateResponse(
                success: true,
                message: "Settings updated successfully",
                notify: true
            ));
        }

        // GET: Visar deaktiveringssidan
        [Authorize]
        [HttpGet]
        public IActionResult Deactivate()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeactivateAccount(DeactivateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(ModelStateErrorResponse("Invalid deactivation request"));
            }

            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(CreateResponse(
                    success: false,
                    message: "User not found",
                    notify: true
                ));
            }

            // Verifiera lösenord
            var passwordValid = await userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
            {
                return Json(CreateResponse(
                    success: false,
                    errors: new Dictionary<string, string>
                    {
                        ["Password"] = "Invalid password"
                    },
                    notify: true
                ));
            }

            try
            {
                // Deaktivera kontot
                user.IsActive = false;

                // Spara ändringar
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return Json(CreateResponse(
                        success: false,
                        errors: updateResult.Errors.ToDictionary(e => "General", e => e.Description),
                        notify: true
                    ));
                }

                // Logga ut användaren
                await signInManager.SignOutAsync();

                return Json(CreateResponse(
                    success: true,
                    redirectUrl: Url.Action("LogIn", "Account") ?? "/"
                ));

            }
            catch (Exception ex)
            {
                // Logga felet här om du har logging
                return Json(CreateResponse(
                    success: false,
                    message: "An error occurred while deactivating your account. Please try again.",
                    notify: true
                ));
            }
        }

        public IActionResult DeactivationConfirmed()
        {
            return View();
        }

    }
}
