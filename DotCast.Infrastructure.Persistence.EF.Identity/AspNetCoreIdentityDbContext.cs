using DotCast.Infrastructure.AppUser;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.EF.Identity;

namespace DotCast.Infrastructure.Persistence.EF.Identity
{
    public class AspNetCoreIdentityDbContext : AspNetIdentityDbContextBase<UserInfo>
    {
        public AspNetCoreIdentityDbContext(DbContextOptions<AspNetCoreIdentityDbContext> options) : base(options)
        {
        }
    }
}