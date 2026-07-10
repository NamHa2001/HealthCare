using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientDoctorLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientDoctorLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HealthProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InitiatedBy = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConsentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsentScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ConsentUserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDoctorLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientDoctorLinks_HealthProfiles_HealthProfileId",
                        column: x => x.HealthProfileId,
                        principalTable: "HealthProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientDoctorLinks_Users_DoctorUserId",
                        column: x => x.DoctorUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctorLinks_DoctorUserId_HealthProfileId",
                table: "PatientDoctorLinks",
                columns: new[] { "DoctorUserId", "HealthProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctorLinks_DoctorUserId_Status",
                table: "PatientDoctorLinks",
                columns: new[] { "DoctorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctorLinks_HealthProfileId_Status",
                table: "PatientDoctorLinks",
                columns: new[] { "HealthProfileId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientDoctorLinks");
        }
    }
}
