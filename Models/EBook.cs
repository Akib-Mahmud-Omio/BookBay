using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace E_Book_Store_1.Models
{
    public class EBook
    {
        [Key]
        public int ebook_id { get; set; }

        [Required]
        public string title { get; set; }

        public string author { get; set; }

        public string genre { get; set; }

        public decimal price { get; set; }

        public string file_path { get; set; }

        public string cover_image { get; set; }

       

        // Add this if you want description
        public string description { get; set; }

        public DateTime created_at { get; set; } = DateTime.Now;

        //  Navigation property for subscriptions
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
