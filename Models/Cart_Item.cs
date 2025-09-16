namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Cart_Item
    {
        [Key]
        public int cart_item_id { get; set; }

        public int? cart_id { get; set; }

        public int? book_id { get; set; }

        public int quantity { get; set; }

        public virtual Book Book { get; set; }

        public virtual Cart Cart { get; set; }
    }
}
