using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace blazordemo.Data
{
    public class SQLiteDBContext : DbContext
    {
        public DbSet<EmailContent> EmailContents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=blazordemo.db");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<EmailContent>().ToTable("EmailContents");
    }
}
