using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        private readonly StreamWriter logStream = new StreamWriter("LogDb.txt", true);
        public AppDBContext(DbContextOptions options) : base(options) 
        {
            
        }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Mechanism> Mechanisms { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<SensorCharacteristic> SensorCharacteristic { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<SensorAssignments> SensorAssignments { get; set; }
        public DbSet<DeviceCharacteristic> DeviceCharacteristic { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(logStream.WriteLine, new[] { DbLoggerCategory.Database.Command.Name },
                Microsoft.Extensions.Logging.LogLevel.Information).EnableSensitiveDataLogging();
        }
        public override void Dispose()
        {
            base.Dispose();
            logStream.Dispose();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SectorConfiguration());
            modelBuilder.ApplyConfiguration(new MechanismConfiguration());
            modelBuilder.ApplyConfiguration(new SensorConfiguration());
            modelBuilder.ApplyConfiguration(new SensorTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DeviceCharacteristicConfiguration());
            modelBuilder.ApplyConfiguration(new SensorAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new SensorCharacteristicConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }

    public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
    {
        public void Configure(EntityTypeBuilder<DeviceType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.Characteristics).WithOne(c => c.DeviceType).HasForeignKey(k => k.DeviceTypeId).OnDelete(DeleteBehavior.Cascade);
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
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
    public class SensorTypeConfiguration : IEntityTypeConfiguration<SensorType>
    {
        public void Configure(EntityTypeBuilder<SensorType> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x=>x.Characteristics).WithOne(c=>c.SensorType).HasForeignKey(k=>k.SensorTypeId).OnDelete(DeleteBehavior.Cascade);
        }
    }
    public class SensorAssignmentConfiguration : IEntityTypeConfiguration<SensorAssignments>
    {
        public void Configure(EntityTypeBuilder<SensorAssignments> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.MasterDevice)
            .WithMany(x => x.ChildrenDevices)
            .HasForeignKey(x => x.MasterDeviceID)
            .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(device => device.Mechanisms)
                .WithOne(mech => mech.Device)
                .HasForeignKey(p => p.DeviceID)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
    public class SensorCharacteristicConfiguration : IEntityTypeConfiguration<SensorCharacteristic>
    {
        public void Configure(EntityTypeBuilder<SensorCharacteristic> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
    public class DeviceCharacteristicConfiguration : IEntityTypeConfiguration<DeviceCharacteristic>
    {
        public void Configure(EntityTypeBuilder<DeviceCharacteristic> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
