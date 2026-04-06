using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using static SV23T1020637.BusinessLayers.SecurityDataService.UserAccountService;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng liên quan đến tài khoản người dùng (đăng nhập, đăng xuất, đổi mật khẩu)
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            ViewBag.UserName = userName;

            // Kiểm tra thông tin đầu vào
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("userName", "Vui lòng nhập đầy đủ tên và mật khẩu");
                return View();
            }

            // Kiểm tra xem username và password (của Employee) có đúng hay không?
            var userAccount = SecurityDataService.UserAccountService.Authorize(UserTypes.Employee, userName, CryptHelper.HashMD5(password));
            var result = userAccount;
            if (userAccount.Result == null )
            {
                ModelState.AddModelError("userName", "Đăng nhập thất bại");
                return View();
            }

            // ĐĂNG NHẬP THÀNH CÔNG

            // 1. Tạo ra thông tin "định danh" người dùng

            WebUserData userData = new WebUserData()
            {
                UserId = userAccount.Result.UserId,
                UserName = userAccount.Result.UserName,
                DisplayName = userAccount.Result.DisplayName,
                Photo = userAccount.Result.Photo,
                Roles = userAccount.Result.RoleNames.Split(',').ToList()
            };

            // 2. Ghi nhận trạng thái đăng nhập
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            // 3. Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userData = User.GetUserData();
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            // Gọi service để thực hiện việc đổi mật khẩu
            bool isChanged = await SecurityDataService.UserAccountService.Authorize(UserTypes.Employee, userData.UserName, CryptHelper.HashMD5(oldPassword))!=null;
            if (!isChanged)
            {
                ModelState.AddModelError("Error", "Đổi mật khẩu thất bại. Vui lòng kiểm tra mật khẩu cũ.");
                return View();
            }
            await SecurityDataService.UserAccountService.ChangePassword(UserTypes.Employee, userData.UserName, CryptHelper.HashMD5(newPassword), CryptHelper.HashMD5(confirmPassword));
            return RedirectToAction("Index", "Home");
        }


        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

}
