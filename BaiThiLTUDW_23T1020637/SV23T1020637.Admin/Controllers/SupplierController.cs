using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Partner;


namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến nhà cung cấp
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class SupplierController : Controller
    {
        private const string SUPPLIER_SEARCH = "SupplierSearchInput"; //Key lưu dữ liệu tìm kiếm của khách hàng vào Session
        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị danh sách khách hàng (Return Search)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(SUPPLIER_SEARCH);
            if (input == null)
                input = new PaginationSearchInput
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            return View(input);
        }

        /// <summary>
        /// Search input and return search a list customers
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListSuppliersAsync(input);
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH, input);
            return View(result);
        }


        /// <summary>
        /// Bổ sung 1 nhà cung cấp mới
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";
            return View("Edit", new Supplier());
        }

        /// <summary>
        /// Cập nhật 1 nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }

        /// <summary>
        /// Xoá 1 nhà cung cấp
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xoá</param>
        /// <returns></returns>
        [HttpGet]
        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.isSuplier = await PartnerDataService.IsUsedSupplierAsync(id);
            return View(model);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Supplier = await PartnerDataService.GetSupplierAsync(id);
            if (Supplier != null)
            {
                await PartnerDataService.DeleteSupplierAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier supplier)
        {
            try
            {
                #region TODO: KIểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(supplier.SupplierName))
                    ModelState.AddModelError(nameof(supplier.SupplierName), "Tên nhà cung cấp không được để trống.");
                if (string.IsNullOrWhiteSpace(supplier.ContactName))
                    ModelState.AddModelError(nameof(supplier.ContactName), "Tên ContactName không được để trống.");
                if (string.IsNullOrEmpty(supplier.Province))
                    ModelState.AddModelError(nameof(supplier.Province), "Hãy chọn Province !");
                // kiểm tra email phone
                if (!ModelState.IsValid)
                    return View("Edit", supplier);
                #endregion
                #region Tiền xử lý dữ liệu trước khi lưu vào database
                if (string.IsNullOrEmpty(supplier.Address)) supplier.Address = "";
                if (string.IsNullOrEmpty(supplier.Phone)) supplier.Phone = "";
                if (string.IsNullOrEmpty(supplier.Email)) supplier.Email = "";
                #endregion
                #region lưu vào CSDL
                if (supplier.SupplierID != 0)
                {
                    await PartnerDataService.UpdateSupplierAsync(supplier);
                    return View(nameof(Edit));
                }
                await PartnerDataService.AddSupplierAsync(supplier);
                #endregion
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", supplier);
            }
        }


    }
}
