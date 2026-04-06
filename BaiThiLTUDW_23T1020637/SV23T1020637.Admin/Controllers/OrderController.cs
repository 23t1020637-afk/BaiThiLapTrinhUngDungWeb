using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV23T1020637.Admin.AppCodes;
using SV23T1020637.BusinessLayers;
using SV23T1020637.Models.Catalog;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Sales;
using System.Threading.Tasks;

namespace SV23T1020637.Admin.Controllers
{
    /// <summary>
    /// Cung cấp các chức năng quản lý dữ liệu liên quan đến đơn hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.Sales},{WebUserRoles.Administrator}")]
    public class OrderController : Controller
    {

        /// <summary>
        /// Tìm kiếm, hiển thị danh sách đơn hàng 
        /// </summary>
        /// <returns></returns>
        private const string ORDER_SEARCH = "OrderSearchInput"; //Key lưu dữ liệu tìm kiếm của khách hàng vào Session
        const string PRODUCT_SEARCH = "OrderProductSearchInput";
        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị danh sách khách hàng (Return Search)
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            input ??= new OrderSearchInput
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    Status = 0,
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now,
                };
            return View(input);
        }

        /// <summary>
        /// Search input and return search a list order
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(ORDER_SEARCH, input);
            ViewBag.data = input.DateFrom;
            return View(result);
        }

        /// <summary>
        /// Bổ sung 1 đơn hàng mới
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            PaginationSearchInput conditionCustomer = new()
            {
                Page = 1,
                PageSize = 599,
                SearchValue = ""
            };
            ViewBag.Customer = (await PartnerDataService.ListCustomersAsync(conditionCustomer)).DataItems;
            ViewBag.Province = await DictionaryDataService.ListProvincesAsync();
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            input ??= new ProductSearchInput
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            return View(input);
        }
        [HttpPost]
        public async Task<IActionResult> Create(OrderViewInfo data)
        {
            int orderID = await SalesDataService.AddOrderAsync(new Order()
            {
                CustomerID = data.CustomerID,
                OrderTime = DateTime.Now,
                DeliveryAddress = data.DeliveryAddress,
                DeliveryProvince = data.DeliveryProvince,
                EmployeeID = int.Parse(User.GetUserData().UserId),
                Status = OrderStatusEnum.Accepted
            });
            var cart = ShoppingCartHelper.GetShoppingCart();
            foreach(var i in cart)
            {
                await SalesDataService.AddDetailAsync(new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    SalePrice = i.SalePrice
                });
            }
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> SearchProduct(ProductSearchInput condition)
        {
            var result = await CatalogDataService.ListProductsAsync(condition);
            var cartIds = ShoppingCartHelper.GetShoppingCart().Select(x => x.ProductID).ToHashSet();
            result.DataItems = result.DataItems.Where(p => !cartIds.Contains(p.ProductID)).ToList();
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, condition);
            return PartialView(result);
        }
        /// <summary>
        /// Xem chi tiết thông tin của đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xem chi tiết</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            ViewBag.Orderdetail = await SalesDataService.ListDetailsAsync(id);
            return View(model);
        }
        /// <summary>
        /// Xoá 1 đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xoá</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var modal = await SalesDataService.GetOrderAsync(id);
            return View(modal);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(Order order)
        {
            await SalesDataService.DeleteOrderAsync(order.OrderID);
            return RedirectToAction("Index", "Order");
        }


        /// <summary>
        /// Xoá 1 sản phẩm trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng có sản phẩm cần xoá</param>
        /// <param name="productId">Mã sản phẩm cần xoá</param>
        /// <returns></returns>


        /// <summary>
        /// Xác nhận đơn hàng, chuyển sang trạng thái đã tiếp nhận
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Accept(int id)
        {
            var modal = await SalesDataService.GetOrderAsync(id);
            return View(modal);
        }
        [HttpPost]
        public async Task<IActionResult> Accept(Order oder)
        {
            var temp = await SalesDataService.GetOrderAsync(oder.OrderID);
            var update_order = temp;
            update_order.EmployeeID = oder.EmployeeID;
            update_order.CustomerID = oder.CustomerID;
            update_order.DeliveryAddress = oder.DeliveryAddress;
            update_order.AcceptTime = DateTime.Now; // khi chấp nhận đơn
            update_order.ShippedTime = null;        // chưa giao
            update_order.FinishedTime = null;       // chưa hoàn tất
            update_order.Status = OrderStatusEnum.Accepted;
            await SalesDataService.UpdateOrderAsync(update_order);
            return RedirectToAction("Detail", "Order", new { id = oder.OrderID});
        }

        /// <summary>
        /// Chuyển đơn hàng sang trạng thái giao hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển trạng thái giao hàng</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Shipping(Order order)
        {
            var model = await SalesDataService.GetOrderAsync(order.OrderID);
            model.ShipperID = order.ShipperID;
            model.ShippedTime = DateTime.Now;
            model.Status = OrderStatusEnum.Shipping;
            await SalesDataService.UpdateOrderAsync(model);
            return RedirectToAction("Detail", "Order", new { id = order.OrderID });
        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái hoàn thành
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển trạng thái hoàn thành</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Finish(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Finish(Order order)
        {
            var model = await SalesDataService.GetOrderAsync(order.OrderID);
            model.Status = OrderStatusEnum.Completed;
            model.FinishedTime = DateTime.Now;
            await SalesDataService.UpdateOrderAsync(model);
            return RedirectToAction("Detail", "Order", new { id = order.OrderID });
        }
        /// <summary>
        /// CHuyển đơn hàng sang trạng thái từ chối
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển trạng thái từ chối</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Reject(Order order)
        {
            var model = await SalesDataService.GetOrderAsync(order.OrderID);
            model.Status = OrderStatusEnum.Rejected;
            await SalesDataService.UpdateOrderAsync(model);
            return RedirectToAction("Detail", "Order", new { id = order.OrderID });
        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái huỷ bỏ
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển trạng thái huỷ bỏ</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(Order order)
        {
            var model = await SalesDataService.GetOrderAsync(order.OrderID);
            model.Status = OrderStatusEnum.Cancelled;
            await SalesDataService.UpdateOrderAsync(model);
            return RedirectToAction("Detail", "Order", new { id = order.OrderID });
        }

        //-------------------- Cart --------------------------//
        [HttpPost]
        public async Task<IActionResult> AddItemCart(OrderDetailViewInfo data)
        {
            ShoppingCartHelper.AddItemToCart(data);
            return RedirectToAction("Create", "Order");
        }
        /// <summary>
        /// Cập nhật 1 sản phẩm trong đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng có sản phẩm cần cập nhật</param>
        /// <param name="productId">Mã sản phẩm cần cập nhật</param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> EditCartItem(int id)
        {
            var lst = ShoppingCartHelper.GetShoppingCart();
            var model = lst.FirstOrDefault(i => i.ProductID == id);
             return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditCartItem(int ProductID, int Quantity, Decimal SalePrice)
        {
            ShoppingCartHelper.UpdateCartItem(ProductID, Quantity, SalePrice);
            return RedirectToAction("Create", "Order");
        }
        [HttpGet]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            var model = ShoppingCartHelper.GetShoppingCart().FirstOrDefault(i => i.ProductID == id);
            return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCartItem(OrderDetailViewInfo data)
        {
            ShoppingCartHelper.DeleteCartItem(data.ProductID);
            return RedirectToAction("Create", "Order");
        }
        /// <summary>
        /// Xoá tất cả giỏ hàng của đơn hàng
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> ClearCart()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ClearCart(int? id)
        {
            ShoppingCartHelper.ClearCart();
            return RedirectToAction("Create", "Order");
        }

    }
}
