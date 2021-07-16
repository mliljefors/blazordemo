using System;
using blazordemo.Areas.Identity.Data;
using blazordemo.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(blazordemo.Areas.Identity.IdentityHostingStartup))]
namespace blazordemo.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<blazordemoContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("blazordemoContextConnection")));

                services.AddDefaultIdentity<blazordemoUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<blazordemoContext>();
            });
        }
    }
}