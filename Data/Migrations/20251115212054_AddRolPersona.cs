using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolPersona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Localidad",
                table: "Personas");

            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Provincia",
                table: "Personas");

            migrationBuilder.DropIndex(
                name: "IX_Personas_LocalidadId",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "LocalidadId",
                table: "Personas");

            migrationBuilder.RenameColumn(
                name: "ProvinciaId",
                table: "Personas",
                newName: "LocalidadResidenciaId");

            migrationBuilder.RenameColumn(
                name: "PaisNacionalidadId",
                table: "Personas",
                newName: "NacionalidadId");

            migrationBuilder.RenameColumn(
                name: "FechaNac",
                table: "Personas",
                newName: "FechaNacimiento");

            migrationBuilder.RenameIndex(
                name: "IX_Personas_ProvinciaId",
                table: "Personas",
                newName: "IX_Personas_LocalidadResidenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Personas_PaisNacionalidadId",
                table: "Personas",
                newName: "IX_Personas_NacionalidadId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Provincias",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<int>(
                name: "PaisResidenciaId",
                table: "Personas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Personas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSDATETIME()");

            migrationBuilder.AddColumn<bool>(
                name: "EsResidenteArgentina",
                table: "Personas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte>(
                name: "Rol",
                table: "Personas",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Paises",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoIso3",
                table: "Paises",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoIso2",
                table: "Paises",
                type: "nchar(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Localidades",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_LocalidadResidencia",
                table: "Personas",
                column: "LocalidadResidenciaId",
                principalTable: "Localidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personas_LocalidadResidencia",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "EsResidenteArgentina",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Personas");

            migrationBuilder.RenameColumn(
                name: "NacionalidadId",
                table: "Personas",
                newName: "PaisNacionalidadId");

            migrationBuilder.RenameColumn(
                name: "LocalidadResidenciaId",
                table: "Personas",
                newName: "ProvinciaId");

            migrationBuilder.RenameColumn(
                name: "FechaNacimiento",
                table: "Personas",
                newName: "FechaNac");

            migrationBuilder.RenameIndex(
                name: "IX_Personas_NacionalidadId",
                table: "Personas",
                newName: "IX_Personas_PaisNacionalidadId");

            migrationBuilder.RenameIndex(
                name: "IX_Personas_LocalidadResidenciaId",
                table: "Personas",
                newName: "IX_Personas_ProvinciaId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Provincias",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "PaisResidenciaId",
                table: "Personas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaAlta",
                table: "Personas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "LocalidadId",
                table: "Personas",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Paises",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoIso3",
                table: "Paises",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldFixedLength: true,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoIso2",
                table: "Paises",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(2)",
                oldFixedLength: true,
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Localidades",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_LocalidadId",
                table: "Personas",
                column: "LocalidadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Localidad",
                table: "Personas",
                column: "LocalidadId",
                principalTable: "Localidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Provincia",
                table: "Personas",
                column: "ProvinciaId",
                principalTable: "Provincias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
