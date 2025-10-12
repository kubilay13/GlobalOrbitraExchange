using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GlobalOrbitra.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChainModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChainType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RpcUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChainModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Decimal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsToken = table.Column<bool>(type: "bit", nullable: false),
                    ChainId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenModels_ChainModel_ChainId",
                        column: x => x.ChainId,
                        principalTable: "ChainModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WalletAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TxHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionModel_TokenModels_TokenId",
                        column: x => x.TokenId,
                        principalTable: "TokenModels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransactionModel_UserModels_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWalletModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWalletModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWalletModels_TokenModels_TokenId",
                        column: x => x.TokenId,
                        principalTable: "TokenModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWalletModels_UserModels_UserId",
                        column: x => x.UserId,
                        principalTable: "UserModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ChainModel",
                columns: new[] { "Id", "ChainType", "IsActive", "Name", "RpcUrl", "Symbol" },
                values: new object[,]
                {
                    { 1, "EVM", true, "Ethereum", "https://mainnet.infura.io/v3/YOUR_PROJECT_ID", "ETH" },
                    { 2, "TRON", true, "Tron", "https://api.trongrid.io", "TRX" },
                    { 3, "EVM", true, "Binance Smart Chain", "https://bsc-dataseed.binance.org", "BSC" },
                    { 4, "Solana", true, "Solana", "https://api.mainnet-beta.solana.com", "SOL" },
                    { 5, "TRON", true, "Tron Nile Testnet", "https://nile.trongrid.io", "TRX" }
                });

            migrationBuilder.InsertData(
                table: "TokenModels",
                columns: new[] { "Id", "ChainId", "ContractAddress", "Decimal", "IsActive", "IsToken", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, 1, "0x0000000000000000000000000000000000000000", 18m, true, false, "ETH", "ETH" },
                    { 2, 2, "TRX_NATIVE", 6m, true, false, "TRX", "TRX" },
                    { 3, 3, "BSC_NATIVE", 18m, true, false, "BSC", "BSC" },
                    { 4, 4, "SOL_NATIVE", 9m, true, false, "SOL", "SOL" },
                    { 5, 5, "TRX_NATIVE", 6m, true, false, "TRX (Testnet)", "TRX" },
                    { 6, 5, "TXYZopYRdj2D9XRtbG411XZZ3kM5VkAeBf", 6m, true, true, "Tether USDT (Nile)", "USDT" },
                    { 7, 5, "TEMVynQpntMqkPxP6wXTW2K7e4sM3cRmWz", 6m, true, true, "USD Coin (Nile)", "USDC" },
                    { 8, 5, "TVSvjZdyDSNocHm7dP3jvCmMNsCnMTPa5W", 18m, true, true, "BTT (Nile)", "BTT" },
                    { 9, 5, "TFT7sNiNDGZcqL7z7dwXUPpxrx1Ewk8iGL", 18m, true, true, "USDD Token (Nile)", "USDD" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenModels_ChainId",
                table: "TokenModels",
                column: "ChainId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionModel_TokenId",
                table: "TransactionModel",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionModel_UserId",
                table: "TransactionModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletModels_TokenId",
                table: "UserWalletModels",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletModels_UserId",
                table: "UserWalletModels",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionModel");

            migrationBuilder.DropTable(
                name: "UserWalletModels");

            migrationBuilder.DropTable(
                name: "TokenModels");

            migrationBuilder.DropTable(
                name: "UserModels");

            migrationBuilder.DropTable(
                name: "ChainModel");
        }
    }
}
