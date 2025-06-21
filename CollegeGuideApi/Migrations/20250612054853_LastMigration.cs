using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollegeGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class LastMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "aed35a70-f118-4d40-b1ff-20775c5b4ca1", new DateTime(2025, 6, 12, 5, 48, 52, 409, DateTimeKind.Utc).AddTicks(4837), "$2a$11$S2nes6T7hCfbQ3259t9kwumtc6ra6tCv4UXXVWuxIEfuXqWV7MxwG", "26b651f5-0524-4c42-846c-515960e40444" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "14d47841-7dc8-4784-b5df-9d260c71645a", new DateTime(2025, 6, 8, 8, 51, 23, 894, DateTimeKind.Utc).AddTicks(5269), "$2a$11$PssDsNGJKGhN51f5RB782u3QrGBvFBJkyhVxSOZshIDeFlWgSg0ay", "00f71135-2d0c-48eb-b344-c43dc3039ba3" });
        }
    }
}
