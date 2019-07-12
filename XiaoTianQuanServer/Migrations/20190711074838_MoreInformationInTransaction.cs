using Microsoft.EntityFrameworkCore.Migrations;

namespace XiaoTianQuanServer.Migrations
{
    public partial class MoreInformationInTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentHash",
                table: "LightningNetworkTransactions",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Settled",
                table: "LightningNetworkTransactions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Settled",
                table: "Transactions",
                column: "Settled");

            migrationBuilder.CreateIndex(
                name: "IX_LightningNetworkTransactions_Settled",
                table: "LightningNetworkTransactions",
                column: "Settled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Settled",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_LightningNetworkTransactions_Settled",
                table: "LightningNetworkTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentHash",
                table: "LightningNetworkTransactions");

            migrationBuilder.DropColumn(
                name: "Settled",
                table: "LightningNetworkTransactions");
        }
    }
}
