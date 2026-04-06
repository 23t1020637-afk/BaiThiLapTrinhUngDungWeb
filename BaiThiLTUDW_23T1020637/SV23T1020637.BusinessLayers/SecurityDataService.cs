using SV23T1020637.DataLayers.Interfaces;
using SV23T1020637.DataLayers.SQLServer;
using SV23T1020637.Models.Partner;
using SV23T1020637.Models.Security;

namespace SV23T1020637.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng/ nghiệp vụ liên quan đến quản lý bảo mật của hệ thống
    /// </summary>
    public static class SecurityDataService
    {
        public static class UserAccountService
        {
            private static readonly IUserAccountRepository employeeAccountDB;
            private static readonly IUserAccountRepository customerAccountDB;
            static UserAccountService()
            {
                employeeAccountDB = new EmployeeAccountRepository(Configuration.ConnectionString);
                customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
            }

            public static async Task<UserAccount?> Authorize(UserTypes userType, string username, string password)
            {
                if (userType == UserTypes.Employee)
                    return await employeeAccountDB.AuthorizeAsync(username, password);
                else
                    return await customerAccountDB.AuthorizeAsync(username, password);
            }

            public static async Task<bool> ChangePassword(UserTypes userType, string username, string password, string newPassword)
            {
                if (userType == UserTypes.Employee)
                    return await employeeAccountDB.ChangePasswordAsync(username, password);
                else
                    return await customerAccountDB.ChangePasswordAsync(username, password);
            }

            //public static int Register(Customer data, string password)
            //{
            //    return ((CustomerAccountRepository)customerAccountDB).Register(data, password);
            //}


            //    public static bool UpdateCustomerProfile(Customer data)
            //    {
            //        return ((CustomerAccountRepository)customerAccountDB).(data);
            //    }
            //}

            public enum UserTypes
            {
                Employee,
                Customer
            }

        }
    }
}
