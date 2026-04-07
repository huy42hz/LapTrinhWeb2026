using SV22T1020158.DataLayers.Interfaces;
using SV22T1020158.DataLayers.SQLServer;
using SV22T1020158.Models;
using SV22T1020158.Models.Security;
using SV22T1020158.Models.Shop;

namespace SV22T1020158.BusinessLayers
{
    public static class SecurityDataService
    {
        private static readonly IUserAccountRepository customerAccountDB;
        private static readonly IUserAccountRepository employeeAccountDB;

        static SecurityDataService()
        {
            customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
            employeeAccountDB = new EmployeeAccountRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            // HASH 
            string hashedPassword = CryptHelper.HashMD5(password);

            // check employee
            var user = await employeeAccountDB.Authorize(userName, hashedPassword);
            if (user != null)
                return user;

            // check customer
            user = await customerAccountDB.Authorize(userName, hashedPassword);
            return user;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public static async Task<bool> ChangePasswordAsync(string userName, string newPassword)
        {
            string hashedPassword = CryptHelper.HashMD5(newPassword);

            if (await employeeAccountDB.ChangePasswordAsync(userName, hashedPassword))
                return true;

            return await customerAccountDB.ChangePasswordAsync(userName, hashedPassword);
        }

        /// <summary>
        /// Kiểm tra mật khẩu
        /// </summary>
        public static async Task<bool> VerifyPasswordAsync(string userName, string password)
        {
            string hashedPassword = CryptHelper.HashMD5(password);

            var user = await employeeAccountDB.Authorize(userName, hashedPassword);
            if (user != null)
                return true;

            user = await customerAccountDB.Authorize(userName, hashedPassword);
            return user != null;
        }
    }
}