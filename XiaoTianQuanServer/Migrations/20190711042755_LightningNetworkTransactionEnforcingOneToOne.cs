using Microsoft.EntityFrameworkCore.Migrations;

namespace XiaoTianQuanServer.Migrations
{
    public partial class LightningNetworkTransactionEnforcingOneToOne : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LightningNetworkTransactions_TransactionId",
                table: "LightningNetworkTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_LightningNetworkTransactions_TransactionId",
                table: "LightningNetworkTransactions",
                column: "TransactionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LightningNetworkTransactions_TransactionId",
                table: "LightningNetworkTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_LightningNetworkTransactions_TransactionId",
                table: "LightningNetworkTransactions",
                column: "TransactionId");
        }
    }
}
