using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Sales;
using SV23T1020637.Shop.AppCodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV23T1020637.Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        /// <summary>
        /// Giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var model = ShoppingCartHelper.GetShoppingCart();

            return View(model);
        }
        /// <summary>
        /// Thanh toán
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Checkout(int id)
        {
            var model = await SalesDataService.ListDetailsAsync(id);
            if (model == null || id == null) 
            {
                return RedirectToAction("Error", "Home");
            }
            ViewBag.Customer = await PartnerDataService.GetCustomerAsync(int.Parse(User.GetUserData().UserId));
            ViewBag.Order = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }

        /// <summary>
        /// Hóa đơn hàng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Confirmation()
        {
            if (ShoppingCartHelper.GetShoppingCart().Count == 0)
                return RedirectToAction("Cart");
            #region Thêm 1 đơn hàng: Order, Customer


            #region lấy thông tin 1 custormer
            var userID = int.Parse(User.GetUserData().UserId);
            if (userID == null)
                return View("Error", "Home");
            var custormer = await PartnerDataService.GetCustomerAsync(userID);
            #endregion
            Order order = new()
            {
                CustomerID = custormer.CustomerID,
                OrderTime = DateTime.Now,
                DeliveryProvince = custormer.Province,
                DeliveryAddress = custormer.Address,
                EmployeeID = 1,
                Status = OrderStatusEnum.New
            };
            int orderID = await SalesDataService.AddOrderAsync(order);
            #endregion

            #region Thêm vào các sản phẩm từ  giỏ hàng: OrderDetails
            var cart = ShoppingCartHelper.GetShoppingCart();
            foreach(var i in cart)
            {
                bool kt = await SalesDataService.AddDetailAsync
                (
                    new OrderDetail()
                    { 
                        OrderID = orderID,
                        ProductID = i.ProductID,
                        Quantity = 1,
                        SalePrice = i.SalePrice + 17000,
                    }
                );
                if( kt ) await CatalogDataService.DeleteProductAsync( i.ProductID );
            }
            #endregion

            ShoppingCartHelper.ClearCart();
            return RedirectToAction("History", "Cart");
        }
        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddItem(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            OrderDetailViewInfo data = new()
            {
                ProductName = product.ProductName,
                Unit = product.Unit,
                Photo = product.Photo,
                ProductID = product.ProductID,
                Quantity = 1,
                SalePrice = product.Price
            };
            //* Thêm thành công
            //_ xóa sản phẩm khỏi List product

            //_ thêm sản phẩm vào giỏ hàng
            ShoppingCartHelper.AddItemToCart(data);
            return RedirectToAction("Index", nameof(Product));
        }
        /// <summary>
        /// Xóa 1 mặt hàng khỏi giỏ hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DeleteItem(int ProductID)
        {

            //* Thêm thành công
            //_ xóa sản phẩm khỏi List product
            //_ thêm sản phẩm vào giỏ hàng
            ShoppingCartHelper.DeleteCartItem(ProductID);
            return RedirectToAction("Index", "Cart");
        }
        [HttpPost]
        public IActionResult ClearItems()
        {
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Index", "Cart");
        }
        [HttpGet]
        public async Task<IActionResult> Cancel(int OrderID)
        {
            await SalesDataService.DeleteOrderAsync(OrderID);
            return RedirectToAction("History", "Cart");
        }
        public async Task<IActionResult> History()
        {
            var userID = int.Parse(User.GetUserData().UserId);
            if (userID == null)
                return View("Error", "Home");
            var custormer = await PartnerDataService.GetCustomerAsync(userID);
            var listCustomerOrder = await SalesDataService.ListOrderCustomerAsyncs(custormer.CustomerID);
            var history = new List<OrderDetailViewHistory>();
            foreach(var i in listCustomerOrder)
            {
                var temp = await SalesDataService.ListDetailsAsync(i.OrderID);
                history.Add(new OrderDetailViewHistory() 
                {
                    OrderID = i.OrderID,
                    OrderTime = i.OrderTime,
                    Status = i.Status,
                    LstOrderDetails = temp
                });
            }
            history.Sort((a, b) => b.OrderID.CompareTo(a.OrderID));
            return View(history);
        }
    }
}

