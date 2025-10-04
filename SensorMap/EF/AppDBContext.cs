using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        public AppDBContext(DbContextOptions options) : base(options)
        {
           
        }

        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Mechanism> Mechanisms  {get; set; }
        public DbSet<Sensor> Sensors  {get; set; }
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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //var config = new ConfigurationBuilder()
            //           .AddJsonFile("appsettings.json")
            //           .SetBasePath(Directory.GetCurrentDirectory())
            //           .Build();
            //optionsBuilder.UseSqlite(config.GetConnectionString("DefaultConnection"));
        }
        public void InitializeTestData()
        {
            using var transaction = Database.BeginTransaction();

            try
            {
                // 1. Создаем участки
                var sectors = new[]
                {
                new Sector { Name = "2 нарезная линия"},
            };
                Sectors.AddRange(sectors);
                SaveChanges();

                // 2. Создаем PLC
                var plcs = new[]
                {
                new PLC { TypePLC = "Siemens S7-1200", IP = "192.168.1.10", Image = "plc_siemens.png" },
                new PLC { TypePLC = "Allen-Bradley", IP = "192.168.1.11", Image = "plc_ab.png" },
                new PLC { TypePLC = "Schneider Electric", IP = "192.168.1.12", Image = "plc_schneider.png" }
            };
                PLCs.AddRange(plcs);
                SaveChanges();

                // 3. Создаем механизмы
                var mechanisms = new[]
                {
                new Mechanism { Name = "Гидравлический пресс MDM", Path = "press_mechanism", Sector = sectors[0], Image = "/Resources/Схема МДМ.jpg", PLC = plcs[0] },
                new Mechanism { Name = "Шаблон", Path = "conveyor", Sector = sectors[0], Image = "/Resources/Шаблон.jpg", PLC = plcs[1] },
                new Mechanism { Name = "Механизация станков", Path = "robot", Sector = sectors[0], Image = "/Resources/2 нар линия механизация.jpg", PLC = plcs[0] }
            };
                Mechanisms.AddRange(mechanisms);
                SaveChanges();

                // 4. Создаем датчики (каталог)
                var sensors = new[]
                {
                new Sensor { Name = "Датчик давления ПД-100", Type = Sensor.SensorType.Давления,Image = "pressure_sensor.png" },
                new Sensor { Name = "Индуктивный датчик ИД-5", Type = Sensor.SensorType.Индуктивный, Image = "inductive_sensor.png" },
                new Sensor { Name = "Оптический барьер ОБ-2", Type = Sensor.SensorType.Оптический, Image = "optical_sensor.png" },
                new Sensor { Name = "Концевой выключатель КВ-1", Type = Sensor.SensorType.Концевик, Image = "limit_switch.png" },
                new Sensor { Name = "Энкодер ЭН-500", Type = Sensor.SensorType.Энкодер, Image = "encoder.png" },
                new Sensor { Name = "Лазерный дальномер ЛД-10", Type = Sensor.SensorType.Лазерный, Image = "laser_sensor.png" },
                new Sensor { Name = "Линейный датчик ЛП-25", Type = Sensor.SensorType.Линейка, Image = "linear_sensor.png" },
                new Sensor { Name = "Герконовый датчик ГД-3", Type = Sensor.SensorType.Геркон, Image = "reed_switch.png" }
            };
                Sensors.AddRange(sensors);
                SaveChanges();

                // 5. Создаем входы PLC
                var plcInputs = new List<PLCInputs>();

                // Для каждого PLC создаем несколько входов
                foreach (var plc in plcs)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        plcInputs.Add(new PLCInputs
                        {
                            Address = $"I{plc.Id}.{i}",
                            Location = $"находиться там {i}"
                        });
                    }
                   
                }
                SensorAssignments.AddRange(plcInputs);
                SaveChanges();

                // 6. Создаем назначения датчиков (расположение на механизмах)
            //    var assignments = new[]
            //    {
            //    // Пресс
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[0].Id, // Датчик давления
            //        PLCInputId = plcInputs[0].Id,
            //        MechanismId = mechanisms[0].Id,
            //        XPoint = 150,
            //        YPoint = 200,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    },
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[3].Id, // Концевик
            //        PLCInputId = plcInputs[1].Id,
            //        MechanismId = mechanisms[0].Id,
            //        XPoint = 300,
            //        YPoint = 100,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    },
                
            //    // Конвейер
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[2].Id, // Оптический барьер
            //        PLCInputId = plcInputs[8].Id,
            //        MechanismId = mechanisms[1].Id,
            //        XPoint = 400,
            //        YPoint = 150,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    },
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[1].Id, // Индуктивный датчик
            //        PLCInputId = plcInputs[9].Id,
            //        MechanismId = mechanisms[1].Id,
            //        XPoint = 200,
            //        YPoint = 150,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    },
                
            //    // Робот-манипулятор
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[4].Id, // Энкодер
            //        PLCInputId = plcInputs[2].Id,
            //        MechanismId = mechanisms[2].Id,
            //        XPoint = 250,
            //        YPoint = 250,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    },
                
            //    // Покрасочная камера
            //    new SensorAssignment
            //    {
            //        SensorId = sensors[7].Id, // Геркон
            //        PLCInputId = plcInputs[20].Id,
            //        MechanismId = mechanisms[3].Id,
            //        XPoint = 100,
            //        YPoint = 100,
            //        AssignedAt = DateTime.Now,
            //        IsActive = true
            //    }
            //};
            //    SensorAssignments.AddRange(assignments);
            //    SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }




    public class SectorConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> builder)
        {
           builder.HasKey(sector=>sector.Id);
            builder
                .HasMany(sector=>sector.Mechanisms)
                .WithOne(mech=>mech.Sector)
                .HasForeignKey(mech=>mech.SectorID);
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
                .HasForeignKey<Mechanism>(m=>m.PLCId);

            builder
                .HasOne(mech=> mech.Sector)
                .WithMany(sector=>sector.Mechanisms).HasForeignKey(mech=>mech.SectorID);

            builder
                .HasMany(mech => mech.Sensors)
                .WithOne(s => s.Mechanism)
                .HasForeignKey(s=>s.MechID);
        }
    }
    public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
    {
        public void Configure(EntityTypeBuilder<Sensor> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .HasOne(sensor => sensor.Inputs)
                .WithOne(i => i.Sensor)
                .HasForeignKey<Sensor>(s=>s.InputsID);
        }
    }
    public class SensorAssignmentConfiguration : IEntityTypeConfiguration<PLCInputs>
    {
        public void Configure(EntityTypeBuilder<PLCInputs> builder)
        {
            builder.HasKey(x => x.Id);
            builder
                .HasOne(i => i.PLC)
                .WithMany(plc => plc.Inputs)
                .HasForeignKey(i=>i.PLCId);
            builder
                .HasOne(i => i.Mechanism)
                .WithMany(m => m.Sensors)
                .HasForeignKey(i => i.SensorID);
            builder
                .HasOne(i => i.Sensor)
                .WithOne(s => s.Inputs)
                .HasForeignKey<PLCInputs>(i=>i.SensorID);
        }
    }
    public class PLCConfiguration : IEntityTypeConfiguration<PLC>
    {
        public void Configure(EntityTypeBuilder<PLC> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .HasOne(p => p.Mechanism)
                .WithOne(m => m.PLC)
                .HasForeignKey<PLC>(p=>p.MechId);
            builder
                .HasMany(p=>p.Inputs)
                .WithOne(i=>i.PLC)
                .HasForeignKey(i=>i.PLCId);
        }
    }
}
