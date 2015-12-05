using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Core.Contents.Extensions;
using Contrib.Foundation.UserProfile.Models;
using System;

namespace Contrib.Foundation.UserProfile
{
    public class LoginsMigrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(LoginsRecord).Name,
                    table => table
                                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                                    .Column<string>("Hash")
                                    .Column<int>("UserProfilePartRecord_id")
                                    .Column<int>("ApplicationRecord_id")
                                    .Column<DateTime>("UpdatedUtc")
                                    )
                                    .AlterTable(typeof(LoginsRecord).Name,
                                        table => table.CreateIndex("LoginHash", new string[] { "Hash" })
            );

            return 1;
        }
    }
}