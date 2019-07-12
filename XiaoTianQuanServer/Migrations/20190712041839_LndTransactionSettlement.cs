using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XiaoTianQuanServer.Migrations
{
    public partial class LndTransactionSettlement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Transactions",
                newName: "Fulfilled");

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionFulfilled",
                table: "Transactions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "PaymentHash",
                table: "LightningNetworkTransactions",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionExpiry",
                table: "Transactions",
                column: "TransactionExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_LightningNetworkTransactions_PaymentHash",
                table: "LightningNetworkTransactions",
                column: "PaymentHash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionExpiry",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_LightningNetworkTransactions_PaymentHash",
                table: "LightningNetworkTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionFulfilled",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "Fulfilled",
                table: "Transactions",
                newName: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentHash",
                table: "LightningNetworkTransactions",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
