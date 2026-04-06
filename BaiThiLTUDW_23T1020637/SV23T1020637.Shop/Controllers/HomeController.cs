using Microsoft.AspNetCore.Mvc;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Shop.AppCodes;
using SV23T1020637.Shop.Models;
using System.Diagnostics;

namespace SV23T1020637.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Product = await CatalogDataService.GetProductAsync(245);
            ViewBag.Product0 = await CatalogDataService.GetProductAsync(96);
            
            var condition = new ProductSearchInput
            {
                Page = 1,
                PageSize = ApplicationContext.PageSize - 2,
                SearchValue = "",
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };
            var model = await CatalogDataService.ListProductsAsync(condition);
            var temp = model;
            temp.DataItems.Sort((i1, i2) => i1.Price.CompareTo(i2.Price));
            ViewBag.ProductSales = temp.DataItems;
            if(User.GetUserData() != null)
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
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
