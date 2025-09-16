using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Book_Store_1.Models
{
    public class Subscription
    {
        [Key]
        public int subscription_id { get; set; }

        [ForeignKey("Customer")]
        public int customer_id { get; set; }

        public int plan_months { get; set; }
        public string plan_name { get; set; }
        public decimal price { get; set; }

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public bool is_active { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual EBook EBook { get; set; }
    }
}
