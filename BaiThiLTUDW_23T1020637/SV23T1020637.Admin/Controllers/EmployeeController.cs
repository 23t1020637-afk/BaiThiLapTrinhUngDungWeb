using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.HR;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến nhân viên
    /// </summary>
    [Authorize(Roles=$"{WebUserRoles.Administrator}")]
    public class EmployeeController : Controller
    {
        readonly int PageSize = 10;
        readonly string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";
        // GET: Employee
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý nhân viên";
            var condition = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_CONDITION);
            condition ??= new PaginationSearchInput()
            {
                SearchValue = "",
                Page = 1,
                PageSize = PageSize
            };
            return View(condition);
        }
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = PageSize;
            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, input);
            return View(result);
        }
        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Employee = await HRDataService.GetEmployeeAsync(id);
            if (Employee == null)
            {
                return NotFound();
            }

            return View(Employee);
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            return View("Edit", new Employee());
        }



        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Employee = await HRDataService.GetEmployeeAsync(id);
            if (Employee == null)
            {
                return NotFound();
            }
            return View(Employee);
        }


        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
                return NotFound();

            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null)
                return NotFound();
            ViewBag.isUsed = await HRDataService.IsUsedEmployeeAsync(id);
            return View(employee);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee != null)
            {
                await HRDataService.DeleteEmployeeAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// Thay đổi mật khẩu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Employee = await HRDataService.GetEmployeeAsync(id);
            if (Employee == null)
            {
                return NotFound();
            }
            return View(Employee);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(int id, string Password, string Repassword)
        {
            var Employee = await HRDataService.GetEmployeeAsync(id);
            if (Employee == null)
                return NotFound();

            if (Password != Repassword)
            {
                ViewBag.Error = "Nhập lại mật khẩu không đúng!";
                return View(Employee);
            }

            await SecurityDataService.UserAccountService.ChangePassword(SecurityDataService.UserAccountService.UserTypes.Employee, Employee.Email, Password, Repassword);

            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// cấp quyền
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> ChangeRole(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var Employee = await _context.Employee
        //        .FirstOrDefaultAsync(m => m.EmployeeID == id);
        //    if (Employee == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(Employee);
        //}
        //[HttpPost]
        //public async Task<IActionResult> ChangeRole(int id, string Password, string Repassword)
        //{
        //    var Employee = await _context.Employee.FindAsync(id);
        //    if (Employee == null)
        //        return NotFound();

        //    if (Password != Repassword)
        //    {
        //        ViewBag.Error = "Nhập lại mật khẩu không đúng!";
        //        return View(Employee);
        //    }

        //    Employee.Password = API.GetMD5(Password);

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}
        [HttpGet]
        public async Task<IActionResult> ChangeRole(int id)
        {
            Employee model;
            if (id == null || (model = await HRDataService.GetEmployeeAsync(id)) == null)
                return RedirectToAction("Index");
            var role = await HRDataService.GetEmployeeRoleAsync(id);
            ViewBag.Role = role.Split(',').ToList();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int EmployeeID, bool roleCustomer, bool roleDataManager, bool roleSales, bool roleAdministrator)
        {
            List<string> LstRole = new ();
            if (roleCustomer)
                LstRole.Add(WebUserRoles.Customer);
            if (roleDataManager)
                LstRole.Add(WebUserRoles.DataManager);
            if (roleSales)
                LstRole.Add(WebUserRoles.Sales);
            if (roleAdministrator)
                LstRole.Add(WebUserRoles.Administrator);
            string role = string.Join(",", LstRole);
            bool kt = await HRDataService.UpdateEmployeeRolesAsync(EmployeeID, role);
           
            return RedirectToAction("ChangeRole", "Employee", new {id = EmployeeID });
        }
            [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data) //, IFormFile? uploadPhoto
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";


                #region TODO: KIểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Tên nhân viên không được để trống.");
                if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng!");
                // kiểm tra email phone
                if (!ModelState.IsValid)
                    return View("Edit", data);
                #endregion
                #region Tiền xử lý dữ liệu trước khi lưu vào database

                #endregion
                #region lưu vào CSDL
                if (data.EmployeeID != 0)
                {
                   await HRDataService.UpdateEmployeeAsync(data);
                    return View(nameof(Edit));
                }
                await HRDataService.AddEmployeeAsync(data);
                #endregion
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }
    }

}
