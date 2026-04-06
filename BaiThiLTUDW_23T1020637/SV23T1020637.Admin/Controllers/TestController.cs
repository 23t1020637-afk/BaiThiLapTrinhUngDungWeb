using Microsoft.AspNetCore.Mvc;
using SV23T1020637.DataLayers.SQLServer;
using SV23T1020637.Models.Common;

namespace SV23T1020637.Admin.Controllers
{
    public class TestController : Controller
    {
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };
            string connectionString = "Server=ADMIN-PC;Database=LiteCommerceDB;User Id=sa;Password=123;Trusted_Connection=True;TrustServerCertificate=True;";
            var repository = new CustomerRepository(connectionString);
            var result = await repository.ListAsync(input);
            return Json(result);
        }
    }
}
