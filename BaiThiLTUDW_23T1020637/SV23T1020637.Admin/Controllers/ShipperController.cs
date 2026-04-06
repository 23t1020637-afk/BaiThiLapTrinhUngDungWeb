using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Partner;
using System.Buffers;
using System.Threading.Tasks;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến shipper
    /// </summary>
    [Authorize(Roles =$"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class ShipperController : Controller
    {
        readonly int PageSize = 10;
        readonly string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";

        // GET: Shippers
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý Người giao hàng";
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(SHIPPER_SEARCH_CONDITION);
            condition ??= new PaginationSearchInput()
            {
                Page = 1,
                PageSize = PageSize,
                SearchValue = ""
            };
            return View(condition);
        }
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            input.PageSize = PageSize;
            var result = await PartnerDataService.ListShippersAsync(input);
            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION, input);
            return View(result);
        }

        // GET: Shippers/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung Shipper";
            return View("Edit", new Shipper());
        }
        // GET: Shippers/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        // GET: Shippers/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Shipper? model;
            if (id == null || (model = await PartnerDataService.GetShipperAsync(id)) == null)
                return RedirectToAction("Index");
            ViewBag.isUsed = await  PartnerDataService.IsUsedShipperAsync(id);
            return View(model);
        }
        //POST: Shippers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shippers = await PartnerDataService.GetShipperAsync(id);
            if (shippers != null)
            {
                await PartnerDataService.DeleteShipperAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            try
            {
                #region TODO: KIểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName), "Tên shipper không được để trống.");
                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone), "Tên ContactName không được để trống.");
                // kiểm tra email phone
                if (!ModelState.IsValid)
                    return View("Edit", data);
                #endregion
                #region Tiền xử lý dữ liệu trước khi lưu vào database
                #endregion
                #region lưu vào CSDL
                if (data.ShipperID != 0)
                {
                    await PartnerDataService.UpdateShipperAsync(data);
                    return View(nameof(Edit));
                }
                await PartnerDataService.AddShipperAsync(data);
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
