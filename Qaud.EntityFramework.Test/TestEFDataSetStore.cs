using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Qaud.Test;

namespace Qaud.EntityFramework.Test
{
    public class TestEFDataStoreGenerator
    {
        public class TestEFDataStoreContext : DbContext
        {
            public TestEFDataStoreContext() : base("TestDb")
            {

            }

            public DbSet<FooModel> FooModels { get; set; }

            public void SetupDB()
            {
                //var connection = (SqlConnection) Database.Connection;
                //var dbName = connection.Database;
                //var origConnString = connection.ConnectionString;
                //var server = Regex.Match(origConnString, "Data Source=(.+?);", RegexOptions.IgnoreCase).Groups[1].Value;
                //connection = new SqlConnection("Data Source=" + server + ";");
                //connection.Open();
                if (Database.Exists() && !DBCreated) Database.Delete();
                //{
                //    var cmd = connection.CreateCommand();
                //    cmd.CommandText = "DROP DATABASE " + dbName + ";";
                //    try
                //    {
                //        cmd.ExecuteNonQuery();
                //    } catch { }
                //    connection.Close();
                //    if (File.Exists(AppDomain.CurrentDomain.GetData("DataDirectory") + @"\QAUDEFTest.mdf"))
                //        File.Delete(AppDomain.CurrentDomain.GetData("DataDirectory") + @"\QAUDEFTest.mdf");
                //}
                if (!DBCreated) Database.Create();
                //{
                //    var cmdCreate = @"CREATE DATABASE
                //            " + dbName + @"
                //        ON PRIMARY (
                //           NAME=QAUD_TEST,
                //           FILENAME = '" + AppDomain.CurrentDomain.GetData("DataDirectory") + @"\QAUDEFTest.mdf'
                //        )
                //        LOG ON (
                //            NAME=QAUD_TEST_log,
                //            FILENAME = '" + AppDomain.CurrentDomain.GetData("DataDirectory") + @"\QAUD_TEST_log.ldf'
                //        );";

                //    connection = new SqlConnection(connection.ConnectionString);
                //    connection.Open();
                //    var cmd = connection.CreateCommand();
                //    cmd.CommandText = cmdCreate;
                //    cmd.ExecuteNonQuery();

                //    cmd.CommandText = "DROP TABLE dbo.FooModel;";
                //    try { cmd.ExecuteNonQuery(); }
                //    catch { }

                //    cmdCreate = @"
                //        CREATE TABLE dbo.FooModel
	               //             (
	               //             ID bigint NOT NULL,
	               //             CreateDate datetime NULL,
	               //             Title nvarchar(500) NULL,
	               //             [Content] nvarchar(MAX) NULL
	               //             )  ON [PRIMARY]
	               //              TEXTIMAGE_ON [PRIMARY];
                            
                //            ALTER TABLE dbo.FooModel ADD CONSTRAINT
	               //             PK_FooModel PRIMARY KEY CLUSTERED 
	               //             (
	               //             ID
	               //             ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
                //        ";
                //    cmd.CommandText = cmdCreate;
                //    cmd.ExecuteNonQuery();
                //}

                //connection.Close();
                DBCreated = true;
            }
        }

        internal static bool DBCreated;
        public EFDataSetDataStore<FooModel> Create()
        {
            var context = new TestEFDataStoreContext();
            context.SetupDB();
            var dstore = new EFDataSetDataStore<FooModel>(context.FooModels, context);
            return dstore;
        }
    }
}
