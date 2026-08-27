using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SewaRent_Api.Migrations
{
    /// <inheritdoc />
    public partial class Sewa02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "US_Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "US_Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandlordCode",
                table: "US_Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LandlordId",
                table: "US_Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BL_Receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PdfGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SysUserCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SysDateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysUserModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysDateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BL_Receipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NO_PaymentNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RentalRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RecipientRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScheduleDay = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SysUserCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SysDateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysUserModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysDateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NO_PaymentNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BL_Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RentalRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingPeriodMonth = table.Column<int>(type: "int", nullable: false),
                    BillingPeriodYear = table.Column<int>(type: "int", nullable: false),
                    RentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UtilityTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BankNameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountNumberSnapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PdfGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SysUserCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SysDateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysUserModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysDateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BL_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BL_Invoices_BL_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "BL_Receipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BL_InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SysUserCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SysDateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SysUserModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SysDateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BL_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BL_InvoiceItems_BL_Invoices_InvoiceEntityId",
                        column: x => x.InvoiceEntityId,
                        principalTable: "BL_Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_US_Users_LandlordCode",
                table: "US_Users",
                column: "LandlordCode",
                unique: true,
                filter: "[LandlordCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_US_Users_LandlordId",
                table: "US_Users",
                column: "LandlordId");

            migrationBuilder.CreateIndex(
                name: "IX_BL_InvoiceItems_InvoiceEntityId",
                table: "BL_InvoiceItems",
                column: "InvoiceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BL_InvoiceItems_InvoiceId",
                table: "BL_InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BL_Invoices_DueDate",
                table: "BL_Invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_BL_Invoices_InvoiceNumber",
                table: "BL_Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BL_Invoices_ReceiptId",
                table: "BL_Invoices",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_BL_Invoices_RentalRequestId",
                table: "BL_Invoices",
                column: "RentalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BL_Invoices_Status",
                table: "BL_Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BL_Receipts_InvoiceId",
                table: "BL_Receipts",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BL_Receipts_ReceiptNumber",
                table: "BL_Receipts",
                column: "ReceiptNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NO_PaymentNotifications_NotificationType",
                table: "NO_PaymentNotifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_NO_PaymentNotifications_RentalRequestId",
                table: "NO_PaymentNotifications",
                column: "RentalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NO_PaymentNotifications_SentAt",
                table: "NO_PaymentNotifications",
                column: "SentAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BL_InvoiceItems");

            migrationBuilder.DropTable(
                name: "NO_PaymentNotifications");

            migrationBuilder.DropTable(
                name: "BL_Invoices");

            migrationBuilder.DropTable(
                name: "BL_Receipts");

            migrationBuilder.DropIndex(
                name: "IX_US_Users_LandlordCode",
                table: "US_Users");

            migrationBuilder.DropIndex(
                name: "IX_US_Users_LandlordId",
                table: "US_Users");

            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "US_Users");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "US_Users");

            migrationBuilder.DropColumn(
                name: "LandlordCode",
                table: "US_Users");

            migrationBuilder.DropColumn(
                name: "LandlordId",
                table: "US_Users");
        }
    }
}
