namespace E_Book_Store_1.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order_Item
    {
        [Key]
        public int order_item_id { get; set; }

        public int? order_id { get; set; }

        public int? book_id { get; set; }

        public int quantity { get; set; }

        public decimal price { get; set; }

        public virtual Book Book { get; set; }

        public virtual Order Order { get; set; }
    }
}
