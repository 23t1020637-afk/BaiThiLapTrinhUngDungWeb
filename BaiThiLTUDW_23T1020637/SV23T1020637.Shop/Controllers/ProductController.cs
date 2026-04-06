using Microsoft.AspNetCore.Mvc;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Common;
using SV23T1020637.Shop.AppCodes;

namespace SV23T1020637.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const string CUSTORMER_SEARCH = "CustomerSearchInput";
        private const int pageSize = 6;
         [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Category = await SelectListHelper.Categories();
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(CUSTORMER_SEARCH);
            if (input == null)
                input = new ProductSearchInput
                {
                    Page = 1,
                    PageSize = 6,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            var result = await CatalogDataService.ListProductsAsync(input);
            ViewBag.maxPrice = (decimal)1690000000;
            ViewBag.minPrice = (decimal)12000;
            return View(input);
        }
         public async Task<IActionResult> Search(ProductSearchInput condition, int Sort)
        {
            condition.PageSize = 6;
            var model = await CatalogDataService.ListProductsAsync(condition);
            if (User.GetUserData() != null)
            {
                var cart = ShoppingCartHelper.GetShoppingCart();
                foreach (var i in cart)
                    model.DataItems.RemoveAll(j => j.ProductID == i.ProductID);
                var order = await SalesDataService.ListOrderCustomerAsyncs(int.Parse(User.GetUserData().UserId));
                foreach (var i in order)
                {
                    var lstDetails = await SalesDataService.ListDetailsAsync(i.OrderID);
                    foreach (var j in lstDetails)
                    {
                        model.DataItems.RemoveAll(mdr => mdr.ProductID == j.ProductID);
                    }

                }
            }
            switch (Sort)
            {
                case 1: // Tăng dần
                    model.DataItems.Sort((a, b) => a.Price.CompareTo(b.Price));
                    break;

                case 2: // Giảm dần
                    model.DataItems.Sort((a, b) => b.Price.CompareTo(a.Price));
                    break;
                case 3: // Tăng dần
                    model.DataItems.Sort((a, b) => a.ProductName.CompareTo(b.ProductName));
                    break;

                case 4: // Giảm dần
                    model.DataItems.Sort((a, b) => b.ProductName.CompareTo(a.ProductName));
                    break;
                default:
                    break;
            }
            ViewBag.Sort = Sort;
            ViewBag.min = condition.MinPrice; ViewBag.max = condition.MaxPrice;
            ApplicationContext.SetSessionData(CUSTORMER_SEARCH, condition);
            return View(model);
        }
        public async Task<IActionResult> Detail(int? id)
        {
            var model = await CatalogDataService.GetProductAsync(id??0);
            if(id  == null || model == null)
                return RedirectToAction(nameof(Index));
            ViewBag.Category = (await CatalogDataService.GetCategoryAsync((int)model.CategoryID)).CategoryName;
            return View(model);
        }
    }
}
