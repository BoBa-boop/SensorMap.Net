using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.IO;

namespace SensorMap.EF
{
    public class AppDBContext : DbContext
    {
        /// <summary>
        /// "add-migration Initial" в Консоль диспетчера пакетов для миграции
        /// </summary>
        /// <param name="options"></param>
        public AppDBContext(DbContextOptions options) : base(options) { }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Mechanism> Mechanisms { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<PLC> PLCs { get; set; }
        public DbSet<PLCInputs> SensorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SectorConfiguration());
            modelBuilder.ApplyConfiguration(new MechanismConfiguration());
            modelBuilder.ApplyConfiguration(new SensorConfiguration());
            modelBuilder.ApplyConfiguration(new PLCConfiguration());
            modelBuilder.ApplyConfiguration(new SensorAssignmentConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }




    public class SectorConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> builder)
        {
            builder.HasKey(sector => sector.Id);
            builder
                .HasMany(sector => sector.Mechanisms)
                .WithOne(mech => mech.Sector)
                .HasForeignKey(mech => mech.SectorID);
        }
    }
    public class MechanismConfiguration : IEntityTypeConfiguration<Mechanism>
    {
        public void Configure(EntityTypeBuilder<Mechanism> builder)
        {
            builder
                .HasKey(mech => mech.Id);

            builder
                .HasOne(mech => mech.PLC)
                .WithOne(plc => plc.Mechanism)
                .HasForeignKey<PLC>(m => m.MechId);
        }
    }
    public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
    {
        public void Configure(EntityTypeBuilder<Sensor> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
    public class SensorAssignmentConfiguration : IEntityTypeConfiguration<PLCInputs>
    {
        public void Configure(EntityTypeBuilder<PLCInputs> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(pi => pi.PLC)
               .WithMany(p => p.Inputs)
               .HasForeignKey(pi => pi.PLCId);
            builder.HasOne(pi => pi.Sensor)
               .WithMany()
               .HasForeignKey(pi => pi.SensorId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
    public class PLCConfiguration : IEntityTypeConfiguration<PLC>
    {
        public void Configure(EntityTypeBuilder<PLC> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .HasMany(p => p.Inputs)
                .WithOne(i => i.PLC)
                .HasForeignKey(i => i.PLCId);
        }
    }
}
