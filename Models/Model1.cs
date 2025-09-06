using System;
using System.Collections.Generic;
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

        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<Admin>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.author)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.genre)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Book>()
                .Property(e => e.region)
                .IsUnicode(false);

            modelBuilder.Entity<Book>()
                .Property(e => e.image)
                .IsUnicode(false);

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

            modelBuilder.Entity<Log>()
                .Property(e => e.entity_type)
                .IsUnicode(false);

            modelBuilder.Entity<Log>()
                .Property(e => e.action_type)
                .IsUnicode(false);
        }
    }
}
