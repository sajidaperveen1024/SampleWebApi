using Microsoft.EntityFrameworkCore;
using SampleWebApi.Model;
using System.Collections.Generic;

namespace SampleWebApi.Db
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
    }
}
