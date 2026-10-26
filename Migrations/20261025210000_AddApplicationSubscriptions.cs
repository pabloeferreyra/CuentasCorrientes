using CuentasCorrientes.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CuentasCorrientes.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261025210000_AddApplicationSubscriptions")]
public partial class AddApplicationSubscriptions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ApplicationSubscriptions",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                ExternalReference = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                MercadoPagoId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                MonthlyAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CurrencyId = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApplicationSubscriptions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationSubscriptions_ExternalReference",
            table: "ApplicationSubscriptions",
            column: "ExternalReference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApplicationSubscriptions_MercadoPagoId",
            table: "ApplicationSubscriptions",
            column: "MercadoPagoId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApplicationSubscriptions");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.14")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
        Microsoft.EntityFrameworkCore.NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("CuentasCorrientes.Models.ApplicationSubscription", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("integer");
            Microsoft.EntityFrameworkCore.NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(entity.Property<int>("Id"));
            entity.Property<string>("CurrencyId").IsRequired().HasMaxLength(3).HasColumnType("character varying(3)");
            entity.Property<DateTime>("CreatedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("ExternalReference").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property<decimal>("MonthlyAmount").HasPrecision(18, 2).HasColumnType("numeric(18,2)");
            entity.Property<string>("MercadoPagoId").HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property<DateTime>("UpdatedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("UserId").IsRequired().HasMaxLength(450).HasColumnType("character varying(450)");
            entity.HasKey("Id");
            entity.HasIndex("ExternalReference").IsUnique();
            entity.HasIndex("MercadoPagoId").IsUnique();
            entity.ToTable("ApplicationSubscriptions");
        });
    }
}
