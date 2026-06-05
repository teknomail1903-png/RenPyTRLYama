using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RenPyTRLauncher.Data;

#nullable disable

namespace RenPyTRLauncher.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("RenPyTRLauncher.Models.Announcement", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                b.Property<string>("Message").IsRequired().HasColumnType("TEXT");
                b.Property<string>("Title").IsRequired().HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Announcements");
            });

            modelBuilder.Entity("RenPyTRLauncher.Models.Game", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("Categories")
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasConversion(
                        new ValueConverter<System.Collections.Generic.List<string>, string>(
                            v => EfValueConverters.StringListToDb(v),
                            v => EfValueConverters.StringListFromDb(v)));
                b.Property<DateTime>("CreatedDate").HasColumnType("TEXT");
                b.Property<string>("Description").IsRequired().HasColumnType("TEXT");
                b.Property<int>("DownloadCount").HasColumnType("INTEGER");
                b.Property<string>("ImagePath").IsRequired().HasColumnType("TEXT");
                b.Property<bool>("IsFeatured").HasColumnType("INTEGER");
                b.Property<bool>("IsTop10").HasColumnType("INTEGER");
                b.Property<bool>("IsVip").HasColumnType("INTEGER");
                b.Property<string>("Name").IsRequired().HasColumnType("TEXT");
                b.Property<string>("PatchFilePath").IsRequired().HasColumnType("TEXT");
                b.Property<string>("PatchVersion").IsRequired().HasColumnType("TEXT");
                b.Property<string>("Version").IsRequired().HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Games");
            });

            modelBuilder.Entity("RenPyTRLauncher.Models.User", b =>
            {
                b.Property<Guid>("Id").HasColumnType("TEXT");
                b.Property<string>("DownloadedPatchIds")
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasConversion(
                        new ValueConverter<System.Collections.Generic.List<Guid>, string>(
                            v => EfValueConverters.GuidListToDb(v),
                            v => EfValueConverters.GuidListFromDb(v)));
                b.Property<string>("Email").IsRequired().HasColumnType("TEXT");
                b.Property<string>("FavoriteGameIds")
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasConversion(
                        new ValueConverter<System.Collections.Generic.List<Guid>, string>(
                            v => EfValueConverters.GuidListToDb(v),
                            v => EfValueConverters.GuidListFromDb(v)));
                b.Property<bool>("IsVip").HasColumnType("INTEGER");
                b.Property<string>("PasswordHash").IsRequired().HasColumnType("TEXT");
                b.Property<string>("RecentDownloadedGameIds")
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasConversion(
                        new ValueConverter<System.Collections.Generic.List<Guid>, string>(
                            v => EfValueConverters.GuidListToDb(v),
                            v => EfValueConverters.GuidListFromDb(v)));
                b.Property<DateTime>("RegisterDate").HasColumnType("TEXT");
                b.Property<string>("Role").IsRequired().HasColumnType("TEXT");
                b.Property<int>("TotalDownloadCount").HasColumnType("INTEGER");
                b.Property<string>("Username").IsRequired().HasColumnType("TEXT");
                b.Property<DateTime?>("VipEndDate").HasColumnType("TEXT");
                b.HasKey("Id");
                b.ToTable("Users");
            });
#pragma warning restore 612, 618
        }
    }
}
