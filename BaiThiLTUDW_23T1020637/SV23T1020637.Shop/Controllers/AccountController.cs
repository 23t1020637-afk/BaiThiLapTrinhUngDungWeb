using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.DataDictionary;
using SV23T1020637.Models.Partner;
using SV23T1020637.Shop.AppCodes;
using static SV23T1020637.BusinessLayers.SecurityDataService.UserAccountService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV23T1020637.Shop.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        public async Task<IActionResult> Index()
        {
            WebUserData userData = User.GetUserData();
            var id = int.Parse(userData.UserId);
            var model = await PartnerDataService.GetCustomerAsync(id);
            if(userData == null || id == null || model == null)
                return View("Error");
            return View(model);
        }
        public async Task<IActionResult> Edit()
        {
            WebUserData userData = User.GetUserData();
            var id = int.Parse(userData.UserId);
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (userData == null || id == null || model == null)
                return View("Error");
            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        //public IActionResult ChangePassword(string password)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Customer model)
        {
            await PartnerDataService.UpdateCustomerAsync(model);
            return RedirectToAction(nameof(Index));
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string Oldpassword, string password, string Repassword)
        {
            ViewBag.UserName = userName;

            // Kiểm tra thông tin đầu vào
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("userName", "Vui lòng nhập đầy đủ tên và mật khẩu");
                return View();
            }

            // Kiểm tra xem username và password (của Employee) có đúng hay không?
            var userAccount = SecurityDataService.UserAccountService.Authorize(UserTypes.Customer, userName, CryptHelper.HashMD5(password));
            if (userAccount.Result == null)
            {

                ModelState.AddModelError("Error", "Đăng nhập thất bại");
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
        [HttpGet]
        public async Task<IActionResult> ChangePasswordCustomers()
        {
            var userData = User.GetUserData();
            var customer = await PartnerDataService.GetCustomerAsync(int.Parse(userData.UserId));
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePasswordCustomers(string Email, string oldPassword  ,string NewPassword, string ConfirmPassword)
        {
            var userDatass = User.GetUserData();
            var customer = await PartnerDataService.GetCustomerAsync(int.Parse(userDatass.UserId));
            var userData = User.GetUserData();
            if ( string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ thông tin.");
                return View(customer);
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("Error", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View(customer);
            }

            // Gọi service để thực hiện việc đổi mật khẩu
            bool isChanged = (await SecurityDataService.UserAccountService.Authorize(UserTypes.Customer, userData.UserName, CryptHelper.HashMD5(oldPassword))) != null ;
            if (!isChanged)
            {
                ModelState.AddModelError("Error", "Đổi mật khẩu thất bại. Vui lòng kiểm tra mật khẩu cũ.");
                return View(customer);
            }
            await SecurityDataService.UserAccountService.ChangePassword(UserTypes.Customer, Email, CryptHelper.HashMD5(NewPassword), CryptHelper.HashMD5(ConfirmPassword));
            return RedirectToAction("ChangePasswordCustomers", "Account");
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
        [HttpGet]
        public IActionResult Register()
        {
            return View(new Customer());
        }
        [HttpPost]
        public async Task<IActionResult> Register(Customer data, string Password)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống.");
            if (string.IsNullOrWhiteSpace(data.ContactName))
                ModelState.AddModelError(nameof(data.ContactName), "Tên tương tác khách hàng không được để trống.");
            if (!(await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, 0)))
                ModelState.AddModelError(nameof(data.Email), "Email không hợp lệ!");
            if (string.IsNullOrWhiteSpace(Password))
                ViewData["PasswordError"] = "mật khẩu không được để trống.";
            if (string.IsNullOrWhiteSpace(data.Province))
                ModelState.AddModelError(nameof(data.Province), "Hãy chọn thành phố."); 
            // kiểm tra email phone
            if (!ModelState.IsValid)
                return View(nameof(Register));
            // sử lý dữ liệu trước khi vào database
            data.Phone ??= "";
            data.Address ??= "";
            await PartnerDataService.AddCustomerAsync(data);
            // Đăng nhập thành công
            await SecurityDataService.UserAccountService.ChangePassword(UserTypes.Customer, data.Email, null, CryptHelper.HashMD5(Password));
            WebUserData userData = new WebUserData()
            {
                UserId = data.CustomerID.ToString(),
                UserName = data.Email,
                DisplayName = data.ContactName,
                Photo ="nophoto.png",
                Roles = "".Split(',').ToList()
            };

            // 2. Ghi nhận trạng thái đăng nhập
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userData.CreatePrincipal());

            // 3. Quay về trang chủ
            return RedirectToAction("Index", "Home");
        }
    }
    }
