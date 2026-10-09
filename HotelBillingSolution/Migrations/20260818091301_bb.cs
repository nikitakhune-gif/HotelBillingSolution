using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBillingWeb.Migrations
{
    /// <inheritdoc />
    public partial class bb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HousekeepingTasks",
                columns: table => new
                {
                    HousekeepingTaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TaskDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCleaned = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaskType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToStaffId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousekeepingTasks", x => x.HousekeepingTaskId);
                    table.ForeignKey(
                        name: "FK_HousekeepingTasks_AppUsers_AssignedToStaffId",
                        column: x => x.AssignedToStaffId,
                        principalTable: "AppUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousekeepingTasks_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HousekeepingTasks_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HousekeepingTaskAttachments",
                columns: table => new
                {
                    HousekeepingTaskAttachmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HousekeepingTaskId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousekeepingTaskAttachments", x => x.HousekeepingTaskAttachmentId);
                    table.ForeignKey(
                        name: "FK_HousekeepingTaskAttachments_HousekeepingTasks_HousekeepingTaskId",
                        column: x => x.HousekeepingTaskId,
                        principalTable: "HousekeepingTasks",
                        principalColumn: "HousekeepingTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTaskAttachments_HousekeepingTaskId",
                table: "HousekeepingTaskAttachments",
                column: "HousekeepingTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_AssignedToStaffId",
                table: "HousekeepingTasks",
                column: "AssignedToStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_BookingId",
                table: "HousekeepingTasks",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_HousekeepingTasks_RoomId",
                table: "HousekeepingTasks",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HousekeepingTaskAttachments");

            migrationBuilder.DropTable(
                name: "HousekeepingTasks");
        }
    }
}
