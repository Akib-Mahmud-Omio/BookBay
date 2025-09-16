using System.Collections.Generic;

namespace E_Book_Store_1.Models
{
    public class ReportsViewModel
    {
        public decimal TotalSales { get; set; }
        public int OrdersThisMonth { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }

        public decimal[] MonthlySalesData { get; set; } // Monthly sales amounts
        public int[] CategoriesData { get; set; }       // Sales per category

        public List<Book> TopSellingBooks { get; set; }
    }
}
