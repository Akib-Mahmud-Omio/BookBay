using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using E_Book_Store_1.Models;

namespace E_Book_Store_1.Helpers
{
    public class SessionHelper
    {
        //for customer login
        private const string CustomerSessionKey = "CustomerId";

        public static void SetCustomerId(int customerId)
        {
            HttpContext.Current.Session[CustomerSessionKey] = customerId;
        }

        public static int? GetCustomerId()
        {
            return HttpContext.Current.Session[CustomerSessionKey] as int?;
        }

        public static void RemoveCustomerId()
        {
            HttpContext.Current.Session.Remove(CustomerSessionKey);
        }

        //for admin login
        private const string AdminSessionKey = "AdminId";

        public static void SetAdminId(int adminId)
        {
            HttpContext.Current.Session[AdminSessionKey] = adminId;
        }

        public static int? GetAdminId()
        {
            return HttpContext.Current.Session[AdminSessionKey] as int?;
        }

        public static void RemoveAdminId()
        {
            HttpContext.Current.Session.Remove(AdminSessionKey);
        }
    }
}