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
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<Cart_Item> Cart_Item { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<Order_Item> Order_Item { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<Discussion> Discussions { get; set; }
        public virtual DbSet<DiscussionMessage> DiscussionMessages { get; set; }
        public virtual DbSet<EBook> EBooks { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }


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

            modelBuilder.Entity<Book>()
                .HasMany(e => e.Cart_Item)
                .WithOptional(e => e.Book)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Book>()
                .HasMany(e => e.Feedbacks)
                .WithOptional(e => e.Book)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Book>()
                .HasMany(e => e.Order_Item)
                .WithOptional(e => e.Book)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Cart>()
                .HasMany(e => e.Cart_Item)
                .WithOptional(e => e.Cart)
                .WillCascadeOnDelete();

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

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Carts)
                .WithOptional(e => e.Customer)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Feedbacks)
                .WithOptional(e => e.Customer)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Customer>()
                .HasMany(e => e.Orders)
                .WithOptional(e => e.Customer)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Feedback>()
                .Property(e => e.comment)
                .IsUnicode(false);

            modelBuilder.Entity<Log>()
                .Property(e => e.entity_type)
                .IsUnicode(false);

            modelBuilder.Entity<Log>()
                .Property(e => e.action_type)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.status)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.total_price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.Order_Item)
                .WithOptional(e => e.Order)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Order_Item>()
                .Property(e => e.price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Discussion>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<DiscussionMessage>()
                .Property(e => e.message_text)
                .IsUnicode(false);

            modelBuilder.Entity<Discussion>()
                .HasMany(d => d.DiscussionMessages)
                .WithRequired(m => m.Discussion)
                .HasForeignKey(m => m.discussion_id)
                .WillCascadeOnDelete();

            modelBuilder.Entity<EBook>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<EBook>()
                .Property(e => e.author)
                .IsUnicode(false);

            modelBuilder.Entity<EBook>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<EBook>()
                .Property(e => e.file_path)
                .IsUnicode(false);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Subscriptions)
                .WithRequired(s => s.Customer)
                .WillCascadeOnDelete(false);

        }
    }
}
