using SV23T1020637.Models.Sales;

namespace SV23T1020637.Admin.AppCodes
{
    public static class ShoppingCartHelper
    {
        /// <summary>
        /// Tên biến để lưu giỏ hàng trong session
        /// </summary>
        const string CART = "ShoppingCart";
        /// <summary>
        /// Lấy giỏ hàng
        /// </summary>
        /// <returns></returns>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null) 
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }
        /// <summary>
        /// Thêm vào giỏ hàng
        /// </summary>
        /// <param name="data"></param>
        public static void AddItemToCart(OrderDetailViewInfo data)
        {
            var cart = GetShoppingCart();
            cart.Add(data);
            ApplicationContext.SetSessionData(CART, cart);
        }
        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong giỏ
        /// </summary>
        /// <param name="ProductID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        public static void UpdateCartItem(int ProductID, int quantity,decimal salePrice)
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == ProductID);
            if (existItem != null) 
            {
                existItem.Quantity = quantity;
                existItem.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        /// <summary>
        /// Xóa một mặt hàng khỏi giỏ hàng
        /// </summary>
        /// <param name="ProductID"></param>
        public static void DeleteCartItem(int ProductID) 
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == ProductID);
            if(existItem != null)            
                cart.Remove(existItem);
            ApplicationContext.SetSessionData(CART, cart);
        }
        /// <summary>
        /// Xóa toàn bộ mặt hàng khỏi giỏ hàng
        /// </summary>
        public static void ClearCart() 
        {
            var cart = GetShoppingCart();
            cart.Clear();
            ApplicationContext.SetSessionData(CART, cart);
        }

        
    }
}
