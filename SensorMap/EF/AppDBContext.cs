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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public AppDBContext(DbContextOptions options) : base(options) 
        {
            
        }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Mechanism> Mechanisms { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<HelpfulFile> HelpfulFiles { get; set; }
        public DbSet<SensorCharacteristic> SensorCharacteristic { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<SensorAssignments> SensorAssignments { get; set; }
        public DbSet<DeviceCharacteristic> DeviceCharacteristic { get; set; }
       
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
            modelBuilder.ApplyConfiguration(new HelpfulFilesConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var s = base.SaveChanges();
            var changesEntities = ChangeTracker.Entries().
                Where(x=>x.State == EntityState.Modified ||
                        x.State == EntityState.Added||
                        x.State == EntityState.Deleted)
                .ToList();

            foreach (var entity in changesEntities)
            {
                foreach (var prop in entity.Properties.Where(p=>p.IsModified))
                {
                    Logger.Info(prop.Metadata.DeclaringType.ClrType.Name + $" |{entity.State.ToString()}| "+ "Id:" + entity.CurrentValues.GetValue<int>("Id") + prop.OriginalValue + "->" + prop.CurrentValue);                    
                }
                if(entity.State == EntityState.Deleted)
                {
                    Logger.Info(entity.Metadata.DisplayName()+$" |{entity.State.ToString()}| " + "Id:"+ entity.CurrentValues.GetValue<int>("Id"));
                }
                if (entity.State == EntityState.Added)
                {
                    Logger.Info(entity.Metadata.DisplayName() + $" |{entity.State.ToString()}| " + "Id:" + entity.CurrentValues.GetValue<int>("Id"));
                }
            }
            return s;
        }
    }

    public class HelpfulFilesConfiguration : IEntityTypeConfiguration<HelpfulFile>
    {
        public void Configure(EntityTypeBuilder<HelpfulFile> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Device).WithMany(d => d.Files).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Sensor).WithMany(d => d.Files).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Mechanism).WithMany(m => m.Files).OnDelete(DeleteBehavior.Cascade);
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
