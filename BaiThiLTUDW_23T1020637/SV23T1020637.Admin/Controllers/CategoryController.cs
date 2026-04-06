using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Common;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan dến loại hàng
    /// </summary>
    [Authorize(Roles =$"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class CategoryController : Controller
    {
        readonly int PageSize = 10;
        readonly string CATEGORY_SEARCH_CONDITION = "CategorySearchCondition";

        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Quản lý Loại hàng";
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_CONDITION);
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
            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_CONDITION, input);
            return View(result);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View("Edit", new Category());
        }
        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Category = await CatalogDataService.GetCategoryAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            return View(Category);
        }



        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Category = await CatalogDataService.GetCategoryAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            ViewBag.isUsed = await  CatalogDataService.IsUsedCategoryAsync(id);
            return View(Category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Category = await CatalogDataService.GetCategoryAsync(id);
            if (Category != null)
            {
                await CatalogDataService.DeleteCategoryAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SaveData(Category category)
        {
            try
            {
                #region TODO: KIểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                    ModelState.AddModelError(nameof(category.CategoryName), "Tên loại hàng không được để trống.");
                if (string.IsNullOrWhiteSpace(category.Description))
                    ModelState.AddModelError(nameof(category.Description), "Tên mô tả không được để trống.");

                if (!ModelState.IsValid)
                    return View("Edit", category);
                #endregion
                #region Tiền xử lý dữ liệu trước khi lưu vào database
                #endregion
                #region lưu vào CSDL
                if (category.CategoryID != 0)
                {
                    await CatalogDataService.UpdateCategoryAsync(category);
                    return View(nameof(Edit));
                }
                await CatalogDataService.AddCategoryAsync(category);
                #endregion
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", category);
            }
        }
    }

}
