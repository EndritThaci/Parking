using Microsoft.EntityFrameworkCore;
using Parking_project.Models;

namespace Parking_project.Data
{
    public class AplicationDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Organizata> Organizata { get; set; }
        public DbSet<Useri> Useri { get; set; }
        public DbSet<NjesiOrg> NjesiOrg { get; set; }
        public DbSet<Lokacioni> Lokacioni { get; set; }
        public DbSet<Vendi> Vendi { get; set; }
        public DbSet<Sherbimi> Sherbimi { get; set; }
        public DbSet<CilsimetParkimit> CilsimetParkimit { get; set; }
        public DbSet<TransaksionParkimi> TransaksionParkimi { get; set; }
        public DbSet<Detajet> Detajet { get; set; }
        public DbSet<TransaksionDetaj> TransaksionDetaj { get; set; }
        public DbSet<Banka> Bank { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<CardDetails> CardDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. NjesiOrg -> Organizata (Many-to-One)
            modelBuilder.Entity<NjesiOrg>()
                .HasOne(n => n.Organizata)
                .WithMany()
                .HasForeignKey(n => n.BiznesId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Lokacioni -> NjesiOrg (Many-to-One)
            modelBuilder.Entity<Lokacioni>()
                .HasOne(l => l.NjesiOrg)
                .WithMany()
                .HasForeignKey(l => l.NjesiteId)
                .OnDelete(DeleteBehavior.Cascade);

            //2.5 Useri -> Organizata (Many-to-One)
            modelBuilder.Entity<Useri>()
                .HasOne(n => n.Organizata)
                .WithMany()
                .HasForeignKey(n => n.BiznesId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Vendi -> Lokacioni (Many-to-One)
            modelBuilder.Entity<Vendi>()
                .HasOne(v => v.Lokacioni)
                .WithMany()
                .HasForeignKey(v => v.LokacioniId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Sherbimi -> Organizata (Many-to-One)
            modelBuilder.Entity<Sherbimi>()
                .HasOne(s => s.Organizata)
                .WithMany()
                .HasForeignKey(s => s.BiznesId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. CilsimetParkimit (Multiple FKs)
            modelBuilder.Entity<CilsimetParkimit>()
                .HasOne(c => c.Sherbimi)
                .WithMany()
                .HasForeignKey(c => c.SherbimiId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CilsimetParkimit>()
                .HasOne(c => c.NjesiOrg)
                .WithMany()
                .HasForeignKey(c => c.NjesiteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6. Detajet -> CilsimetParkimit
            modelBuilder.Entity<Detajet>()
                .HasOne(d => d.CilsimetParkimit)
                .WithMany()
                .HasForeignKey(d => d.CilsimetiId)
                .OnDelete(DeleteBehavior.Cascade);

            // 7. TransaksionParkimi (Multiple FKs)
            modelBuilder.Entity<TransaksionParkimi>()
                .HasOne(t => t.Vendi)
                .WithMany()
                .HasForeignKey(t => t.VendiParkimitId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TransaksionParkimi>()
                .HasOne(t => t.Njesia)
                .WithMany()
                .HasForeignKey(t => t.NjesiaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TransaksionParkimi>()
                .HasOne(t => t.Cilsimet)
                .WithMany()
                .HasForeignKey(t => t.CilsimiId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransaksionParkimi>()
               .HasOne(t => t.User)
               .WithMany()
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.NoAction);

            // 8. TransaksionDetaj (Many-to-Many Join Table with Composite Key)
            modelBuilder.Entity<TransaksionDetaj>()
                .HasKey(td => new { td.TransaksionId, td.SherbimiId });

            modelBuilder.Entity<TransaksionDetaj>()
                .HasOne(td => td.TransaksionParkimi)
                .WithMany()
                .HasForeignKey(td => td.TransaksionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TransaksionDetaj>()
                .HasOne(td => td.Sherbimi)
                .WithMany()
                .HasForeignKey(td => td.SherbimiId)
                .OnDelete(DeleteBehavior.Cascade);

            // 9. CardDetails (Multiple FKs)
            modelBuilder.Entity<CardDetails>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CardDetails>()
                .HasOne(c => c.BankAccount)
                .WithMany()
                .HasForeignKey(c => c.BankAcountId)
                .OnDelete(DeleteBehavior.Cascade);

            // 10. BankAccount 
            modelBuilder.Entity<BankAccount>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BankAccount>()
                .HasOne(d => d.Bank)
                .WithMany()
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
