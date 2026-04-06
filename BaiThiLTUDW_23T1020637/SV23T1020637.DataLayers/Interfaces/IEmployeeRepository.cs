using SV23T1020637.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV23T1020637.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Employee
    /// </summary>
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        /// <summary>
        /// Kiểm tra xem email của nhân viên có hợp lệ không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của nhân viên mới
        /// Nếu id <> 0: Kiểm tra email của nhân viên có mã là id
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
        /// <summary>
        /// Cập nhật phân quyền nhân viên
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        Task<bool> UpdateRoleAsync(int employeeID, string role);
        /// <summary>
        /// lấy chuỗi phân quyền
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        Task<string> GetRoleAsync(int employeeID);
    }
}
