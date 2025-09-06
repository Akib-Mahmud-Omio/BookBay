namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Log
    {
        [Key]
        public int log_id { get; set; }

        public int admin_id { get; set; }

        [StringLength(50)]
        public string entity_type { get; set; }

        public int entity_id { get; set; }

        [StringLength(50)]
        public string action_type { get; set; }

        public DateTime? action_timestamp { get; set; }

        public virtual Admin Admin { get; set; }
    }
}
