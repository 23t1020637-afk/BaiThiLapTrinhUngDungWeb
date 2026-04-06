using Dapper;
using Microsoft.Data.SqlClient;
using SV23T1020637.DataLayers.Interfaces;
using SV23T1020637.Models.Common;
using SV23T1020637.Models.Sales;

namespace SV23T1020637.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho đơn hàng (Order) và chi tiết đơn hàng (OrderDetail) trên CSDL SQL Server
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// Lấy thông tin toàn bộ đơn hàng của khách hàng
        /// </summary>
        /// <param name="CustomerID"></param>
        /// <returns></returns>
        public async Task<List<Order>> ListOrderCustomerAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Orders WHERE CustomerID = @CustomerID";
            var orders = await connection.QueryAsync<Order>(sql, new { CustomerID = customerId });
            return orders.ToList();
        }

        /// <summary>
        /// Bổ sung một đơn hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Dữ liệu đơn hàng</param>
        /// <returns>Mã đơn hàng vừa được tạo (OrderID)</returns>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                VALUES (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                SELECT SCOPE_IDENTITY();";

            // Khi khởi tạo đơn hàng mới, các thông tin như AcceptTime, ShipperID, ShippedTime, FinishedTime thường là null
            var result = await connection.ExecuteScalarAsync<int>(sql, data);
            return result;
        }

        /// <summary>
        /// Thêm một mặt hàng vào chi tiết đơn hàng
        /// </summary>
        /// <param name="data">Dữ liệu chi tiết đơn hàng</param>
        /// <returns>True nếu thành công, False nếu thất bại</returns>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)";

            var result = await connection.ExecuteAsync(sql, data);
            return result > 0;
        }

        /// <summary>
        /// Xóa một đơn hàng (và toàn bộ chi tiết của đơn hàng đó)
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu thất bại</returns>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            // Cần xóa chi tiết đơn hàng trước (ràng buộc khóa ngoại), sau đó mới xóa đơn hàng
            string sql = @"
                DELETE FROM OrderDetails WHERE OrderID = @OrderID;
                DELETE FROM Orders WHERE OrderID = @OrderID;";

            var result = await connection.ExecuteAsync(sql, new { OrderID = orderID });
            return result > 0;
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng cần xóa</param>
        /// <returns>True nếu thành công, False nếu thất bại</returns>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                DELETE FROM OrderDetails 
                WHERE OrderID = @OrderID AND ProductID = @ProductID";

            var result = await connection.ExecuteAsync(sql, new { OrderID = orderID, ProductID = productID });
            return result > 0;
        }

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ của một đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <returns>Đối tượng OrderViewInfo hoặc null nếu không tìm thấy</returns>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT o.*,
                       c.CustomerName,
                       c.ContactName as CustomerContactName,
                       c.Address as CustomerAddress,
                       c.Phone as CustomerPhone,
                       c.Email as CustomerEmail,
                       e.FullName as EmployeeName,
                       s.ShipperName,
                       s.Phone as ShipperPhone
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                WHERE o.OrderID = @OrderID";

            var result = await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { OrderID = orderID });
            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <param name="productID">Mã mặt hàng</param>
        /// <returns>Đối tượng OrderDetailViewInfo hoặc null nếu không tìm thấy</returns>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT od.*, p.ProductName, p.Photo, p.Unit
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID AND od.ProductID = @ProductID";

            var result = await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql, new { OrderID = orderID, ProductID = productID });
            return result;
        }

        /// <summary>
        /// Tìm kiếm và phân trang danh sách đơn hàng
        /// </summary>
        /// <param name="input">Điều kiện tìm kiếm đơn hàng</param>
        /// <returns>Danh sách đơn hàng được phân trang PagedResult</returns>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            // Xử lý chuỗi tìm kiếm
            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            // Ép kiểu Status về int. Quy ước: nếu (int)Status == 0 nghĩa là lấy tất cả trạng thái.
            int status = (int)input.Status;

            // Câu lệnh đếm tổng số dòng dữ liệu
            string sqlCount = @"
                SELECT COUNT(*) 
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                WHERE (@SearchValue = N'' OR c.CustomerName LIKE @SearchValue OR e.FullName LIKE @SearchValue OR s.ShipperName LIKE @SearchValue)
                  AND (@Status = 0 OR o.Status = @Status)
                  AND (@DateFrom IS NULL OR CAST(o.OrderTime AS DATE) >= CAST(@DateFrom AS DATE))
                  AND (@DateTo IS NULL OR CAST(o.OrderTime AS DATE) <= CAST(@DateTo AS DATE))";

            // Câu lệnh truy vấn dữ liệu
            string sqlQuery = @"
                SELECT 
    o.*,
    c.CustomerName,
    c.ContactName as CustomerContactName,
    c.Address as CustomerAddress,
    c.Phone as CustomerPhone,
    c.Email as CustomerEmail,
    e.FullName as EmployeeName,
    s.ShipperName,
    s.Phone as ShipperPhone,
    ISNULL(od.TotalOrderPrice, 0) AS TotalOrderPrice

FROM Orders o

LEFT JOIN (
    SELECT 
        OrderID,
        SUM(Quantity * SalePrice) AS TotalOrderPrice
    FROM OrderDetails
    GROUP BY OrderID
) od ON o.OrderID = od.OrderID

LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID

WHERE (@SearchValue = N'' OR c.CustomerName LIKE @SearchValue 
    OR e.FullName LIKE @SearchValue 
    OR s.ShipperName LIKE @SearchValue)
  AND (@Status = 0 OR o.Status = @Status)
  AND (@DateFrom IS NULL OR CAST(o.OrderTime AS DATE) >= CAST(@DateFrom AS DATE))
  AND (@DateTo IS NULL OR CAST(o.OrderTime AS DATE) <= CAST(@DateTo AS DATE))

ORDER BY o.OrderTime DESC
OFFSET @Offset ROWS 
FETCH NEXT @PageSize ROWS ONLY";

            //if (input.PageSize > 0)
            //{
            //    sqlQuery += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            //}

            var parameters = new
            {
                SearchValue = searchValue,
                Status = status,
                DateFrom = input.DateFrom,
                DateTo = input.DateTo,
                Offset = input.Offset,
                PageSize = input.PageSize
            };

            int rowCount = await connection.ExecuteScalarAsync<int>(sqlCount, parameters);
            var dataItems = await connection.QueryAsync<OrderViewInfo>(sqlQuery, parameters);

            return new PagedResult<OrderViewInfo>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = dataItems.ToList()
            };
        }

        /// <summary>
        /// Lấy danh sách các mặt hàng nằm trong một đơn hàng
        /// </summary>
        /// <param name="orderID">Mã đơn hàng</param>
        /// <returns>Danh sách chi tiết mặt hàng</returns>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT od.*, p.ProductName, p.Photo, p.Unit
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID";

            var result = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { OrderID = orderID });
            return result.ToList();
        }

       

        /// <summary>
        /// Cập nhật thông tin đơn hàng (trạng thái, người giao hàng, thời gian, v.v.)
        /// </summary>
        /// <param name="data">Dữ liệu đơn hàng cần cập nhật</param>
        /// <returns>True nếu thành công, False nếu thất bại</returns>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Orders
                SET CustomerID = @CustomerID,
                    OrderTime = @OrderTime,
                    DeliveryProvince = @DeliveryProvince,
                    DeliveryAddress = @DeliveryAddress,
                    EmployeeID = @EmployeeID,
                    AcceptTime = @AcceptTime,
                    ShipperID = @ShipperID,
                    ShippedTime = @ShippedTime,
                    FinishedTime = @FinishedTime,
                    Status = @Status
                WHERE OrderID = @OrderID";

            var result = await connection.ExecuteAsync(sql, data);
            return result > 0;
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong đơn hàng
        /// </summary>
        /// <param name="data">Dữ liệu chi tiết đơn hàng cần cập nhật</param>
        /// <returns>True nếu thành công, False nếu thất bại</returns>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE OrderDetails
                SET Quantity = @Quantity,
                    SalePrice = @SalePrice
                WHERE OrderID = @OrderID AND ProductID = @ProductID";

            var result = await connection.ExecuteAsync(sql, data);
            return result > 0;
        }
    }
}