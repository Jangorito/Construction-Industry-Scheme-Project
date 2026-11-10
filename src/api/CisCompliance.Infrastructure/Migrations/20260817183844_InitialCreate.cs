using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CisCompliance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Supplier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    LineId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountGbp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PredictedLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PredictedConfidence = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Reasoning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectiveLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsOverridden = table.Column<bool>(type: "bit", nullable: false),
                    OverrideReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverriddenAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineEntity_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RuleCitationEntity",
                columns: table => new
                {
                    InvoiceLineEntityId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Resolved = table.Column<bool>(type: "bit", nullable: false),
                    SourceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Verified = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleCitationEntity", x => new { x.InvoiceLineEntityId, x.Id });
                    table.ForeignKey(
                        name: "FK_RuleCitationEntity_InvoiceLineEntity_InvoiceLineEntityId",
                        column: x => x.InvoiceLineEntityId,
                        principalTable: "InvoiceLineEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineEntity_InvoiceId_LineId",
                table: "InvoiceLineEntity",
                columns: new[] { "InvoiceId", "LineId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleCitationEntity");

            migrationBuilder.DropTable(
                name: "InvoiceLineEntity");

            migrationBuilder.DropTable(
                name: "Invoices");
        }
    }
}
