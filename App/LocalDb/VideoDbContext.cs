using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace App.LocalDb
{


    public class VideoDbContext : DbContext
    {
        public VideoDbContext() { }


        //model
        public DbSet<FrameModel> Frames { get; set; }


        //connection
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={AppContext.BaseDirectory}video.db");


        //create model option
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FrameModel>(buildAction =>
            {
                buildAction.ToTable<FrameModel>(name: "FrameModel");

                buildAction
                .HasKey(x => x.Id);

                buildAction
                .Property<int>(x => x.Id)
                .HasColumnType<int>("INTEGER")
                .ValueGeneratedOnAdd();

                buildAction
                .Property<string>(x => x.Name)
                .HasColumnType<string>("TEXT")
                .IsRequired(true);

                buildAction
                .Property<string>(x => x.Frame)
                .HasColumnType<string>("TEXT")
                .IsRequired(true);
            });
        }
    }
}
