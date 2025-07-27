using Microsoft.EntityFrameworkCore;

namespace GBES.Models
{
    public class GBESportsAppDbContext : DbContext
    {
        public GBESportsAppDbContext() : base()
        {
            // Empty   매개변수 없는 생성자
        }

        public GBESportsAppDbContext(DbContextOptions<GBESportsAppDbContext> options)
            : base(options)
        {
            // Empty   매개변수 있는 생성자
        }

        public DbSet<Z_Member> Z_Members { get; set; }
        public DbSet<Z_PartyName> Z_PartyNames { get; set; }
        public DbSet<Z_Event> Z_Events { get; set; }
        public DbSet<Z_Section> Z_Sections { get; set; }
        public DbSet<Z_Detail> Z_Details { get; set; }
        public DbSet<Z_PartyEntry> Z_PartyEntries { get; set; }
        public DbSet<Z_GameResult> Z_GameResults { get; set; }
        public DbSet<Z_GameOfficer> Z_GameOfficers { get; set; }
        public DbSet<Z_PhysicalScoreCard> Z_PhysicalScoreCards { get; set; }
        public DbSet<Z_JonghapResult> Z_JonghapResults { get; set; }
        public DbSet<Z_PhysicalKingRecord> Z_PhysicalKingRecords { get; set; }


    }
}
