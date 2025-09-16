using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Book_Store_1.Models
{
    public class Discussion
    {
        [Key]
        public int discussion_id { get; set; }

        public int? book_id { get; set; }

        [ForeignKey("book_id")]
        public virtual Book Book { get; set; }

        [StringLength(255)]
        public string book_name { get; set; }

        [Required]
        [StringLength(255)]
        public string title { get; set; }

        public int? created_by { get; set; }

        [ForeignKey("created_by")]
        public virtual Customer CreatedByCustomer { get; set; }

        public DateTime created_on { get; set; } = DateTime.Now;

        public bool is_closed { get; set; } = false;

        public virtual ICollection<DiscussionMessage> DiscussionMessages { get; set; } = new List<DiscussionMessage>();
    }
}
