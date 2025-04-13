using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthServer.Migrations
{
    /// <inheritdoc />
    public partial class roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "asp_net_user_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                table: "asp_net_user_tokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "provider_key",
                table: "asp_net_user_logins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                table: "asp_net_user_logins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "asp_net_roles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "asp_net_roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "asp_net_roles",
                columns: new[] { "id", "concurrency_stamp", "description", "level", "name", "normalized_name" },
                values: new object[,]
                {
                    { new Guid("0195cf65-c3df-cb1f-ad9c-9f5cf959aa50"), null, "Has the highest level of access, including the ability to manage users, roles, permissions, and all resources. Can typically view, modify, and delete any data or resource within the system", 0, "Adminstrator", "ADMINSTRATOR" },
                    { new Guid("0195cf69-b973-7219-5606-ef45849095f9"), null, "Has access to specific resources or departments, allowing them to manage users, permissions, and data within their area of responsibility. May have the ability to create, update, and delete data, but within a defined scope", 1, "Manager", "MANAGER" },
                    { new Guid("0195cfb7-c950-ff05-7f2b-64b483f0a0b2"), null, "Can create, update, and potentially delete specific types of data or resources. May have access to a wider range of resources than a \"Viewer\" but less than a \"Manager\" or \"Administrator\".", 2, "Editor", "EDITOR" },
                    { new Guid("0195d4c4-6334-c620-1444-7f8b9928ee56"), null, "Has read-only access to specific resources or data. Can only view information, but cannot modify or delete it. ", 3, "Viewer", "VIEWER" },
                    { new Guid("0195d4c6-1ae2-f9fb-13fb-12411040064a"), null, "Access to view and edit their own information.", 4, "Customer", "CUSTOMER" }
                });

            migrationBuilder.InsertData(
                table: "asp_net_users",
                columns: new[] { "id", "access_failed_count", "concurrency_stamp", "created_at", "email", "email_confirmed", "first_name", "last_name", "lockout_enabled", "lockout_end", "normalized_email", "normalized_user_name", "password_hash", "phone_number", "phone_number_confirmed", "security_stamp", "two_factor_enabled", "user_name" },
                values: new object[] { new Guid("0195cf7a-9180-12ef-9b45-8635127e6629"), 0, "58e5c795-f2df-44af-98cf-967b5b0d88fd", new DateTime(2025, 3, 30, 17, 36, 29, 726, DateTimeKind.Utc).AddTicks(1967), "andersonruban1281@gmail.com", true, "Anderson", "Ruban", false, null, "andersonruban1281@gmail.com", "DEV", "AQAAAAIAAYagAAAAEHRvE3/L9LvU5mvULrvcB1nakNephv8BoHKbDaIlZk6iqVN7t7mCLrhaNM3xlQpDqA==", null, false, null, false, "Dev" });

            migrationBuilder.InsertData(
                table: "asp_net_user_roles",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { new Guid("0195cf65-c3df-cb1f-ad9c-9f5cf959aa50"), new Guid("0195cf7a-9180-12ef-9b45-8635127e6629") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "asp_net_roles",
                keyColumn: "id",
                keyValue: new Guid("0195cf69-b973-7219-5606-ef45849095f9"));

            migrationBuilder.DeleteData(
                table: "asp_net_roles",
                keyColumn: "id",
                keyValue: new Guid("0195cfb7-c950-ff05-7f2b-64b483f0a0b2"));

            migrationBuilder.DeleteData(
                table: "asp_net_roles",
                keyColumn: "id",
                keyValue: new Guid("0195d4c4-6334-c620-1444-7f8b9928ee56"));

            migrationBuilder.DeleteData(
                table: "asp_net_roles",
                keyColumn: "id",
                keyValue: new Guid("0195d4c6-1ae2-f9fb-13fb-12411040064a"));

            migrationBuilder.DeleteData(
                table: "asp_net_user_roles",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { new Guid("0195cf65-c3df-cb1f-ad9c-9f5cf959aa50"), new Guid("0195cf7a-9180-12ef-9b45-8635127e6629") });

            migrationBuilder.DeleteData(
                table: "asp_net_roles",
                keyColumn: "id",
                keyValue: new Guid("0195cf65-c3df-cb1f-ad9c-9f5cf959aa50"));

            migrationBuilder.DeleteData(
                table: "asp_net_users",
                keyColumn: "id",
                keyValue: new Guid("0195cf7a-9180-12ef-9b45-8635127e6629"));

            migrationBuilder.DropColumn(
                name: "description",
                table: "asp_net_roles");

            migrationBuilder.DropColumn(
                name: "level",
                table: "asp_net_roles");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "asp_net_user_tokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                table: "asp_net_user_tokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "provider_key",
                table: "asp_net_user_logins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "login_provider",
                table: "asp_net_user_logins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
