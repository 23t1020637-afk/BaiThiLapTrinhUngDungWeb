using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.Admin.Models;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Sales;
using System.Diagnostics;

namespace SV23T1020637.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Hiển thị trang chủ của ứng dụng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var conditionProduct = new ProductSearchInput();
            var conditiOrder = new OrderSearchInput();
            var condition = new PaginationSearchInput();
            conditionProduct ??= new ProductSearchInput()
            {
                CategoryID = 0,
                SupplierID = 0,
                MaxPrice = 0,
                MinPrice = 0,
                SearchValue = "",
                Page = 1,
                PageSize =  0
            };
            conditiOrder ??= new OrderSearchInput()
            {
                Page = 1,
                PageSize = 0,
                SearchValue = "",
                Status = 0,
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now,
            };
            condition ??= new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 0,
                SearchValue = ""
            };
            
            var order = await SalesDataService.ListOrdersAsync(conditiOrder);
            var customer = await PartnerDataService.ListCustomersAsync(condition);
            var product = await CatalogDataService.ListProductsAsync(conditionProduct);

            var lstDonHang = new List<OrderViewInfo>();
            decimal doanhThu = 0;
            #region doanhThu
            foreach(var i in order.DataItems)
            {
                doanhThu += (decimal)(await SalesDataService.ListDetailsAsync(i.OrderID)).Sum(sale => sale.SalePrice);
                if (i.Status >= OrderStatusEnum.New)
                    lstDonHang.Add(await SalesDataService.GetOrderAsync(i.OrderID));
            }
               
            #endregion
            var countDonHang = order.DataItems.Count;
            var countKhachHang = customer.DataItems.Count;
            var countSanPham = product.DataItems.Count;
            var lstTopProduct = new List<Product>();
            
            ViewBag.doanhThu = doanhThu;
            ViewBag.countDonHang = countDonHang;
            ViewBag.countKhachHang = countKhachHang;
            ViewBag.countSanPham = countSanPham;
            ViewBag.lstTopProduct = lstTopProduct;
            ViewBag.lstDonHang = lstDonHang;
            return View();
        }

    }
}
