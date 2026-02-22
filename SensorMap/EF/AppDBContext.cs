using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Data.Common;
using System.IO;

namespace SensorMap.EF
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions options) : base(options) { }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Mechanism> Mechanisms { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<PLC> PLCs { get; set; }
        public DbSet<SensorAssignments> SensorAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SectorConfiguration());
            modelBuilder.ApplyConfiguration(new MechanismConfiguration());
            modelBuilder.ApplyConfiguration(new SensorConfiguration());
            modelBuilder.ApplyConfiguration(new SensorTypeConfiguration());
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
                .HasForeignKey(mech => mech.SectorID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    public class MechanismConfiguration : IEntityTypeConfiguration<Mechanism>
    {
        public void Configure(EntityTypeBuilder<Mechanism> builder)
        {
            builder
                .HasKey(mech => mech.Id);
            //// Навигационное свойство к сектору
            //builder
            //    .HasOne(mechanism => mechanism.Sector)
            //    .WithMany(sector => sector.Mechanisms)
            //    .HasForeignKey(mechanism => mechanism.SectorID)
            //    .OnDelete(DeleteBehavior.Restrict); // Механизмы остаются при удалении сектора

            //// Навигационное свойство к устройству PLC
            //builder
            //    .HasOne(mechanism => mechanism.PLC)
            //    .WithMany(plc => plc.Mechanisms)
            //    .HasForeignKey(mechanism => mechanism.PLCID)
            //    .OnDelete(DeleteBehavior.Restrict); // Механизмы остаются, если устройство удалено
            
            builder
                .HasMany(x => x.SensorsAssig)
                .WithOne(sens => sens.Mechanism)
                .HasForeignKey(sens => sens.MechanismId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
    public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
    {
        public void Configure(EntityTypeBuilder<Sensor> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(t => t.SensorType).WithMany(s => s.Sensors)
                .HasForeignKey(s=>s.SensorTypeID)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
    public class SensorTypeConfiguration : IEntityTypeConfiguration<SensorType>
    {
        public void Configure(EntityTypeBuilder<SensorType> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
    public class SensorAssignmentConfiguration : IEntityTypeConfiguration<SensorAssignments>
    {
        public void Configure(EntityTypeBuilder<SensorAssignments> builder)
        {
            builder.HasKey(x => x.Id);
            //builder.HasOne(se => se.PLC)
            //   .WithMany(p => p.Inputs)
            //   .HasForeignKey(se => se.PLCId);

            builder.HasOne(se => se.Sensor)
               .WithMany(s=>s.Sensors)
               .HasForeignKey(se => se.SensorId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(se=>se.Mechanism)
            //    .WithMany(m=>m.SensorsAssig)
            //    .HasForeignKey(se=>se.MechanismId);
        }
    }
    public class PLCConfiguration : IEntityTypeConfiguration<PLC>
    {
        public void Configure(EntityTypeBuilder<PLC> builder)
        {
            builder.HasKey(x => x.Id);
            builder
                .HasMany(plc => plc.Mechanisms)
                .WithOne(mech => mech.PLC)
                .HasForeignKey(p => p.PLCID)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
