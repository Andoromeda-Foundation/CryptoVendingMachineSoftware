using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XiaoTianQuanServer.Migrations
{
    public partial class RemoveMachineLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_VendingMachines_VendingMachineId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ExclusiveUseLock",
                table: "VendingMachines");

            migrationBuilder.RenameColumn(
                name: "VendingMachineId",
                table: "Transactions",
                newName: "InventoryId");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "Transactions",
                newName: "BasePrice");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_VendingMachineId",
                table: "Transactions",
                newName: "IX_Transactions_InventoryId");

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Settled",
                table: "Transactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionExpiry",
                table: "Transactions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "LightningNetworkTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionId = table.Column<Guid>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    PaymentRequest = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightningNetworkTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LightningNetworkTransactions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LightningNetworkTransactions_TransactionId",
                table: "LightningNetworkTransactions",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Inventories_InventoryId",
                table: "Transactions",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Inventories_InventoryId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "LightningNetworkTransactions");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Settled",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionExpiry",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "Transactions",
                newName: "VendingMachineId");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "Transactions",
                newName: "PaymentType");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_InventoryId",
                table: "Transactions",
                newName: "IX_Transactions_VendingMachineId");

            migrationBuilder.AddColumn<Guid>(
                name: "ExclusiveUseLock",
                table: "VendingMachines",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_VendingMachines_VendingMachineId",
                table: "Transactions",
                column: "VendingMachineId",
                principalTable: "VendingMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
