using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp_Adance_signalR.Data;

public class AuthenticationDbContext : IdentityDbContext<IdentityUser>
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Conversation> Conversations { get; set; }
}