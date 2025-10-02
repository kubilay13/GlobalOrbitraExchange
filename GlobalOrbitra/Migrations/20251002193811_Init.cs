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
                    Chain = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ChainModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenModels_ChainModel_ChainModelId",
                        column: x => x.ChainModelId,
                        principalTable: "ChainModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetTransactionModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WalletAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commusion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransactionModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransactionModels_UserModels_UserId",
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
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
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
                    { 4, "Solana", true, "Solana", "https://api.mainnet-beta.solana.com", "SOL" }
                });

            migrationBuilder.InsertData(
                table: "TokenModels",
                columns: new[] { "Id", "Chain", "ChainId", "ChainModelId", "ContractAddress", "Decimal", "IsActive", "IsToken", "Name", "Symbol" },
                values: new object[,]
                {
                    { 1, 0, 1, null, "0x0000000000000000000000000000000000000000", 18m, true, false, "ETH", "ETH" },
                    { 2, 0, 2, null, "TRX_NATIVE", 6m, true, false, "TRX", "TRX" },
                    { 3, 0, 3, null, "BSC_NATIVE", 18m, true, false, "BSC", "BSC" },
                    { 4, 0, 4, null, "SOL_NATIVE", 9m, true, false, "SOL", "SOL" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransactionModels_UserId",
                table: "AssetTransactionModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenModels_ChainModelId",
                table: "TokenModels",
                column: "ChainModelId");

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
                name: "AssetTransactionModels");

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
