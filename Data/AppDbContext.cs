using Microsoft.EntityFrameworkCore;

namespace GorevTakipProgrami.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext>options): base(options)
    {
        
    }
}