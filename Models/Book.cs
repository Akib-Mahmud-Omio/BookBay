namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Book")]
    public partial class Book
    {

        [Key]
        public int book_id { get; set; }

        [Required]
        [StringLength(255)]
        public string title { get; set; }

        [StringLength(100)]
        public string author { get; set; }

        [StringLength(50)]
        public string genre { get; set; }

        [Column(TypeName = "text")]
        public string description { get; set; }

        public decimal price { get; set; }

        public int? stock { get; set; }

        [Column(TypeName = "date")]
        public DateTime? published_date { get; set; }

        [StringLength(50)]
        public string region { get; set; }

        public int? age { get; set; }

        [StringLength(8000)]
        public string image { get; set; }
    }
}
