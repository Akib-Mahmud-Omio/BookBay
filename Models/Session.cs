namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Session")]
    public partial class Session
    {
        [Key]
        public int session_id { get; set; }

        public int user_id { get; set; }

        public DateTime? login_time { get; set; }

        public DateTime? logout_time { get; set; }
    }
}
