using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace E_Book_Store_1.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Connection")
        {
        }
        public virtual DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Customer>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.address)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .Property(e => e.phone_number)
                .IsUnicode(false);
        }
    }
}
