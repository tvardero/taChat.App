using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace taChat.App.Controllers;

public class AccountController : Controller
{
    public AccountController(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return HttpContext.User.Identity?.Name == null ? View() : Redirect("/");
    }

    [HttpPost]
    public async Task<IActionResult> Login(AccountViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _signInManager.UserManager.FindByNameAsync(model.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {
                    return Redirect(model.ReturnUrl ?? "/");
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Password), "Password is incorrect");
                }
            }
            else
            {
                ModelState.AddModelError(nameof(model.UserName), "Username does not exist");
            }
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(AccountViewModel model)
    {
        if (ModelState.IsValid)
        {
            User user = new() { UserName = model.UserName };

            var result = await _signInManager.UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return Redirect(model.ReturnUrl ?? "/");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    switch (error.Code)
                    {
                        case "InvalidUserName":
                        case "DuplicateUserName":
                            {
                                ModelState.AddModelError(nameof(model.UserName), error.Description);
                                break;
                            }

                        case "PasswordTooShort":
                        case "PasswordRequiresUniqueChars":
                        case "PasswordRequiresNonAlphanumeric":
                        case "PasswordRequiresLower":
                        case "PasswordRequiresUpper":
                            {
                                ModelState.AddModelError(nameof(model.Password), error.Description);
                                break;
                            }

                        default:
                            {
                                ModelState.AddModelError("", error.Description);
                                break;
                            }
                    }
                }
            }
        }
        return View(nameof(Login), model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    private readonly SignInManager<User> _signInManager;
}