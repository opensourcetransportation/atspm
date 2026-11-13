#region license
// Copyright 2026 Utah Departement of Transportation
// for SqlDatabaseProvider - Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations/20260521170321_5_3.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class _5_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "WatchDogIgnoreEvents",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "WatchDogIgnoreEvents",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Timestamp",
                table: "UsageEntries",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "UsageEntries",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "UsageEntries",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Routes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Routes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "RouteLocations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "RouteLocations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "RouteDistances",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "RouteDistances",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Regions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Regions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Products",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Products",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "MenuItems",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "MenuItems",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "MeasureType",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "MeasureType",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "MeasureOptions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "MeasureOptions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "MeasureOptionPresets",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "MeasureOptionPresets",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "MeasureComments",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "MeasureComments",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "LocationTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "LocationTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Locations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Locations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Jurisdictions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Jurisdictions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Faqs",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Faqs",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "DirectionTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "DirectionTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Devices",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Devices",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "DeviceConfigurations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "DeviceConfigurations",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Detectors",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Detectors",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "DetectorComments",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "DetectorComments",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "DetectionTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "DetectionTypes",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Areas",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Areas",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Modified",
                table: "Approaches",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Created",
                table: "Approaches",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransitSignalPriorityNumber",
                table: "Approaches",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "DetectionTypes",
                columns: new[] { "Id", "Abbreviation", "Description", "DisplayOrder", "Modified", "ModifiedBy" },
                values: new object[] { 12, "PP", "Priority and Preemption", 12, null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 92,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 117,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 118,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 119,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                values: new object[] { 121, 5, null, null, "combineThruRight", "FALSE" });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "DisplayOrder", "Modified", "ModifiedBy", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[,]
                {
                    { 39, "TSPS", 133, null, null, "Transit Signal Priority Summary", false, true },
                    { 40, "TSPD", 134, null, null, "Transit Signal Priority Details", false, true }
                });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                values: new object[,]
                {
                    { 122, 39, null, null, "binSize", "15" },
                    { 123, 40, null, null, "binSize", "15" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DropColumn(
                name: "TransitSignalPriorityNumber",
                table: "Approaches");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "WatchDogIgnoreEvents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "WatchDogIgnoreEvents",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "UsageEntries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "UsageEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "UsageEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Routes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Routes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "RouteLocations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RouteLocations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "RouteDistances",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "RouteDistances",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Regions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Regions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Products",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Products",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "MenuItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "MenuItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "MeasureType",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "MeasureType",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "MeasureOptions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "MeasureOptions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "MeasureOptionPresets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "MeasureOptionPresets",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "MeasureComments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "MeasureComments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "LocationTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "LocationTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Locations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Locations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Jurisdictions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Jurisdictions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Faqs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Faqs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "DirectionTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "DirectionTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Devices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Devices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "DeviceConfigurations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "DeviceConfigurations",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Detectors",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Detectors",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "DetectorComments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "DetectorComments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "DetectionTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "DetectionTypes",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Areas",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Areas",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Modified",
                table: "Approaches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Approaches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 0,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "DirectionTypes",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Faqs",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 69,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 71,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 72,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 73,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 76,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 79,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 80,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 83,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 85,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 92,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 113,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 114,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 116,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 117,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 118,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 119,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 120,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "Created", "Modified" },
                values: new object[] { null, null });
        }
    }
}
