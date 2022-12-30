#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Web.Areas.Instance.Models.Migrations.SqlServer
{
    public partial class CorrectedMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //these stored procedure sql commands were added manually
            migrationBuilder.AlterColumn<bool>(
                name: "Archived",
                table: "Agents",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "ApiServers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ApiServers",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ProcessingOrder",
                table: "BootstrapDataAgents",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BootstrapDatas",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "BootstrapDatas",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "DataMaps",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "Resources",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ApiSection",
                table: "Resources",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "HasAttribute",
                table: "Scripts",
                nullable: false,
                defaultValue: false);


            migrationBuilder.Sql(
                @"CREATE PROCEDURE [dbo].[ApplicationLog_AddEntry]
    @machineName [nvarchar](200),
    @logged [datetimeoffset](7),
    @level [varchar](5),
    @userName [nvarchar](200),
    @message [nvarchar](max),
    @logger [nvarchar](300),
    @properties [nvarchar](max),
    @serverName [nvarchar](200),
    @port [nvarchar](100),
    @url [nvarchar](2000),
    @serverAddress [nvarchar](100),
    @remoteAddress [nvarchar](100),
    @exception [nvarchar](max)
AS
BEGIN
    INSERT INTO [dbo].[ApplicationLogs] (
    [MachineName],
    [Logged],
    [Level],
    [UserName],
    [Message],
    [Logger],
    [Properties],
    [ServerName],
    [Port],
    [Url],
    [ServerAddress],
    [RemoteAddress],
    [Exception]
    ) VALUES (
    @machineName,
    @logged,
    @level,
    @userName,
    @message,
    @logger,
    @properties,
    @serverName,
    @port,
    @url,
    @serverAddress,
    @remoteAddress,
    @exception
    );
END"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //these stored procedure sql commands were added manually
            migrationBuilder.AlterColumn<bool>(
                name: "Archived",
                table: "Agents",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "ApiServers",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ApiServers",
                maxLength: 255,
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ProcessingOrder",
                table: "BootstrapDataAgents",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BootstrapDatas",
                maxLength: 255,
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "BootstrapDatas",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "DataMaps",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ApiVersionId",
                table: "Resources",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<int>(
                name: "ApiSection",
                table: "Resources",
                nullable: false,
                defaultValue: null);

            migrationBuilder.AlterColumn<bool>(
                name: "HasAttribute",
                table: "Scripts",
                nullable: false,
                defaultValue: null);
        }
    }
}
