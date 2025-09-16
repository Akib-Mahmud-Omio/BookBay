using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Book_Store_1.Models
{
    public class DiscussionMessage
    {
        [Key]
        public int message_id { get; set; }

        [Required]
        public int discussion_id { get; set; }

        [ForeignKey("discussion_id")]
        public virtual Discussion Discussion { get; set; }

        public int? parent_message_id { get; set; }

        [ForeignKey("parent_message_id")]
        public virtual DiscussionMessage ParentMessage { get; set; }

        public int? customer_id { get; set; }

        [ForeignKey("customer_id")]
        public virtual Customer Customer { get; set; }

        [Required]
        public string message_text { get; set; }

        public DateTime posted_on { get; set; } = DateTime.Now;
    }
}
