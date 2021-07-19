using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blazordemo.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace blazordemo.Data
{
    public class blazordemoContext : IdentityDbContext<blazordemoUser>
    {
        public blazordemoContext(DbContextOptions<blazordemoContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
