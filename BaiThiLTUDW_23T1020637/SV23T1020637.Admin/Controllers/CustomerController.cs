using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Partner;
using SV23T1020637.Models.Security;

namespace SV23T1020637.Admin.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.DataManager},{WebUserRoles.Administrator},{WebUserRoles.Customer}")]
    public class CustomerController : Controller
    {
        readonly int pageSize = 10;
        const string CUSTOMER_SEARCH_CONDITION = "CustomerSearchCondition";

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý Khách hàng";
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH_CONDITION);
            condition ??= new PaginationSearchInput()
            {
                Page = 1,
                PageSize = pageSize,
                SearchValue = ""
            };
            return View(condition);
        }
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = pageSize;
            var result = await PartnerDataService.ListCustomersAsync(input);
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_CONDITION, input);
            return View(result);
        }


        // GET: Customer/Create
        public async Task<IActionResult> CreateAsync()
        {
            ViewBag.Title = "Bổ sung khách hàng!";
            return View("Edit", new Customer());
        }
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
                return NotFound();

            var employee = await PartnerDataService.GetCustomerAsync(id);
            if (employee == null)
                return NotFound();
            ViewBag.isCustomer = await PartnerDataService.IsUsedCustomerAsync(id);
            return View(employee);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Customer = await PartnerDataService.GetCustomerAsync(id);
            if (Customer != null)
            {
                PartnerDataService.DeleteCustomerAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Customer =  await PartnerDataService.GetCustomerAsync(id);
            if (Customer == null)
            {
                return NotFound();
            }
            return View(Customer);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string Password)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);

            await SecurityDataService.UserAccountService.ChangePassword(SecurityDataService.UserAccountService.UserTypes.Customer , customer.Email, Password, Password);

            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng !" : "Cập nhật thông tin khách hàng";
                //Kiểm tra dữ liệu đầu vào: FullName và Email là bắt buộc, Email chưa được sử dụng bởi nhân viên khác
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập họ tên khách hàng");
                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Vui lòng nhập tên tương tác ");
                if (!await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng!");
                if (string.IsNullOrEmpty(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Hãy chọn Province !");
                if (!ModelState.IsValid)
                    return View("Edit", data);


                //Lưu dữ liệu vào database (bổ sung hoặc cập nhật)
                if (data.CustomerID != 0)
                {
                    await PartnerDataService.UpdateCustomerAsync(data);
                    return View("Edit", data);
                }
                await PartnerDataService.AddCustomerAsync(data);
                return RedirectToAction(nameof(Index));
            }
            catch //(Exception ex)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }

    }

}
