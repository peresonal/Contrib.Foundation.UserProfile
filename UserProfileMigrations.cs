using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Core.Contents.Extensions;
using Contrib.Foundation.UserProfile.Models;
using System;

namespace Contrib.Foundation.UserProfile
{
    public class UserProfileMigrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(UserProfilePartRecord).Name,
                                        table => table
                                                        .ContentPartRecord()
                                                        .Column<string>("FirstName")
                                                        .Column<string>("LastName")
                                                        .Column<bool>("ShowEmail")
                                                        .Column<string>("WebSite")
                                                        .Column<string>("Location")
                                                        .Column<string>("Bio")
            );

            ContentDefinitionManager.AlterPartDefinition(typeof(UserProfilePart).Name, part => part
                .WithDescription("Adds a Profile Part to any Content Type."));

            ContentDefinitionManager.AlterPartDefinition(typeof(UserProfilePart).Name, cfg => cfg.Attachable());

            // attach user profile part to User Content Type
            ContentDefinitionManager.AlterTypeDefinition("User",
            cfg => cfg
                .WithPart(typeof(UserProfilePart).Name)
                    .WithSetting("TypeIndexing.Included", "True")

            );

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(typeof(UserProfilePartRecord).Name,
               table => table
                   .AddColumn<string>("FBusername")
               );
            SchemaBuilder.AlterTable(typeof(UserProfilePartRecord).Name,
               table => table
                   .AddColumn<string>("FBemail")
               );
            SchemaBuilder.AlterTable(typeof(UserProfilePartRecord).Name,
               table => table
                   .AddColumn<string>("FBtoken")
               );
            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable(typeof(UserApplicationRecord).Name,
                    table => table
                                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                                    .Column<DateTime>("RegistrationStart")
                                    .Column<DateTime>("RegistrationEnd")
                                    .Column<int>("UserProfilePartRecord_id")
                                    .Column<int>("ApplicationRecord_id")
            );

            return 3;
        }
        public int UpdateFrom3()
        {
            SchemaBuilder.CreateTable(typeof(UserUserRoleRecord).Name,
                    table => table
                                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                                    .Column<int>("UserProfilePartRecord_id")
                                    .Column<int>("UserRoleRecord_id")
            );

            return 4;
        }
    }
}