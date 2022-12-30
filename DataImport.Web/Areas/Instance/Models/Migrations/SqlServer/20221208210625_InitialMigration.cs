#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Web.Areas.Instance.Models.Migrations.SqlServer
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Logged = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Port = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ServerAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RemoteAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstanceAllowUserRegistration = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailableCmdlets = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportPSModules = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseVersion",
                columns: table => new
                {
                    VersionString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "IngestionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EducationOrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Process = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndPointUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HttpStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OdsResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApiServerName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Started = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Completed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceTable = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scripts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ScriptType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScriptContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequireOdsApiAccess = table.Column<bool>(type: "bit", nullable: false),
                    HasAttribute = table.Column<bool>(type: "bit", nullable: false),
                    ExecutablePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutableArguments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scripts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiServers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TokenUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AuthUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApiVersionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiServers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiServers_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ResourcePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApiVersionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BootstrapDatas_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiSection = table.Column<int>(type: "int", nullable: false),
                    ApiVersionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ResourcePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Map = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ColumnHeaders = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiVersionId = table.Column<int>(type: "int", nullable: false),
                    FileProcessorScriptId = table.Column<int>(type: "int", nullable: true),
                    Attribute = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataMaps_ApiVersions_ApiVersionId",
                        column: x => x.ApiVersionId,
                        principalTable: "ApiVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataMaps_Scripts_FileProcessorScriptId",
                        column: x => x.FileProcessorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AgentTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AgentAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Port = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Directory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastExecuted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowProcessorScriptId = table.Column<int>(type: "int", nullable: true),
                    FileGeneratorScriptId = table.Column<int>(type: "int", nullable: true),
                    ApiServerId = table.Column<int>(type: "int", nullable: true),
                    RunOrder = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Agents_ApiServers_ApiServerId",
                        column: x => x.ApiServerId,
                        principalTable: "ApiServers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Scripts_FileGeneratorScriptId",
                        column: x => x.FileGeneratorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Agents_Scripts_RowProcessorScriptId",
                        column: x => x.RowProcessorScriptId,
                        principalTable: "Scripts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDataApiServers",
                columns: table => new
                {
                    BootstrapDataId = table.Column<int>(type: "int", nullable: false),
                    ApiServerId = table.Column<int>(type: "int", nullable: false),
                    ProcessedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDataApiServers", x => new { x.BootstrapDataId, x.ApiServerId });
                    table.ForeignKey(
                        name: "FK_BootstrapDataApiServers_ApiServers_ApiServerId",
                        column: x => x.ApiServerId,
                        principalTable: "ApiServers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BootstrapDataApiServers_BootstrapDatas_BootstrapDataId",
                        column: x => x.BootstrapDataId,
                        principalTable: "BootstrapDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentSchedules_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BootstrapDataAgents",
                columns: table => new
                {
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    BootstrapDataId = table.Column<int>(type: "int", nullable: false),
                    ProcessingOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BootstrapDataAgents", x => new { x.BootstrapDataId, x.AgentId });
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BootstrapDataAgents_BootstrapDatas_BootstrapDataId",
                        column: x => x.BootstrapDataId,
                        principalTable: "BootstrapDatas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataMapAgents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataMapId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    ProcessingOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMapAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataMapAgents_DataMaps_DataMapId",
                        column: x => x.DataMapId,
                        principalTable: "DataMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rows = table.Column<int>(type: "int", nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdateDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Files_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_ApiServerId",
                table: "Agents",
                column: "ApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_FileGeneratorScriptId",
                table: "Agents",
                column: "FileGeneratorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_RowProcessorScriptId",
                table: "Agents",
                column: "RowProcessorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentSchedules_AgentId",
                table: "AgentSchedules",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_ApiVersionId",
                table: "ApiServers",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiServers_Name",
                table: "ApiServers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiVersions_ApiVersion",
                table: "ApiVersions",
                column: "ApiVersion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLogs_Logged",
                table: "ApplicationLogs",
                column: "Logged");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataAgents_AgentId",
                table: "BootstrapDataAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDataApiServers_ApiServerId",
                table: "BootstrapDataApiServers",
                column: "ApiServerId");

            migrationBuilder.CreateIndex(
                name: "IX_BootstrapDatas_ApiVersionId",
                table: "BootstrapDatas",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_AgentId",
                table: "DataMapAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMapAgents_DataMapId",
                table: "DataMapAgents",
                column: "DataMapId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMaps_ApiVersionId",
                table: "DataMaps",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_DataMaps_FileProcessorScriptId",
                table: "DataMaps",
                column: "FileProcessorScriptId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AgentId",
                table: "Files",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_CreateDate",
                table: "Files",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Date",
                table: "IngestionLogs",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionLogs_Result",
                table: "IngestionLogs",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_SourceTable_Key",
                table: "Lookups",
                columns: new[] { "SourceTable", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ApiVersionId",
                table: "Resources",
                column: "ApiVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Path_ApiVersionId",
                table: "Resources",
                columns: new[] { "Path", "ApiVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_Name_ScriptType",
                table: "Scripts",
                columns: new[] { "Name", "ScriptType" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentSchedules");

            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BootstrapDataAgents");

            migrationBuilder.DropTable(
                name: "BootstrapDataApiServers");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DatabaseVersion");

            migrationBuilder.DropTable(
                name: "DataMapAgents");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "IngestionLogs");

            migrationBuilder.DropTable(
                name: "JobStatus");

            migrationBuilder.DropTable(
                name: "Lookups");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BootstrapDatas");

            migrationBuilder.DropTable(
                name: "DataMaps");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "ApiServers");

            migrationBuilder.DropTable(
                name: "Scripts");

            migrationBuilder.DropTable(
                name: "ApiVersions");
        }
    }
}
