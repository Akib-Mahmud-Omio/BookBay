namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Feedback")]
    public partial class Feedback
    {
        [Key]
        public int feedback_id { get; set; }

        public int? customer_id { get; set; }

        public int? book_id { get; set; }

        public int? rating { get; set; }

        [Column(TypeName = "text")]
        public string comment { get; set; }

        public DateTime? date { get; set; }

        public virtual Book Book { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
