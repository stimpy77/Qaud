using System;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qaud.Test;

namespace Qaud.DbProvider.Test
{
    [TestClass]
    public class SqlDbProviderDataStoreSimpleTest : DataStoreSimpleTest
    {
        private const string TEST_DATABASE_NAME = "QAUD_TEST";
        private const string TEST_CONNECTION_STRING = "Data Source=(localdb)\\v11.0;Integrated Security=true;Database=" + TEST_DATABASE_NAME + ";";
        public SqlDbProviderDataStoreSimpleTest()
            : base(new DbProviderDataStore<FooModel>(DbProviderFactories.GetFactory("System.Data.SqlClient"), TEST_CONNECTION_STRING))
        {
            RecreateTable();
        }

        private SqlConnection NewConnection(bool autoOpen = true)
        {
            var conn = new SqlConnection(TEST_CONNECTION_STRING);
            if (autoOpen) conn.Open();
            return conn;
        }

        protected virtual void RecreateTable()
        {
            var cmdCreateDatabase = "CREATE DATABASE QAUD_TEST";
            var cmdDrop = @"DROP TABLE FooModel";

            var cmdCreate = @"CREATE TABLE dbo.FooModel
	                            (
	                            ID bigint NOT NULL,
	                            CreateDate datetime NULL,
	                            Title nvarchar(500) NULL,
	                            [Content] nvarchar(MAX) NULL
	                            )  ON [PRIMARY]
	                             TEXTIMAGE_ON [PRIMARY];
                            
                            ALTER TABLE dbo.FooModel ADD CONSTRAINT
	                            PK_FooModel PRIMARY KEY CLUSTERED 
	                            (
	                            ID
	                            ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                            ";
            try
            {
                var newdb = NewConnection(false);
                newdb.ConnectionString = TEST_CONNECTION_STRING.Substring(0, TEST_CONNECTION_STRING.IndexOf("Database="));
                newdb.Open();
                var cmd = new SqlCommand(cmdCreateDatabase, newdb);
                cmd.ExecuteNonQuery();
            }
            catch {}
            try
            {
                var cmd2 = new SqlCommand(cmdDrop, NewConnection());
                cmd2.ExecuteNonQuery();
            }
            catch { }
            var cmd3 = new SqlCommand(cmdCreate, NewConnection());
            cmd3.ExecuteNonQuery();
        }

        protected override void AddItemToStore(FooModel item)
        {
            var conn = NewConnection();
            var sql = @"INSERT INTO FooModel
                        (
                            ID, 
                            CreateDate,
                            Title,
                            Content
                        ) VALUES (
                            @ID,
                            @CreateDate,
                            @Title,
                            @Content
                        )";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@ID", item.ID);
            cmd.Parameters.Add("@CreateDate", item.CreateDate);
            cmd.Parameters.Add("@Title", item.Title);
            cmd.Parameters.Add("@Content", item.Content);
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            using (var conn = NewConnection())
            {
                var cmdtext = "DELETE FROM FooModel WHERE ID = @ID";
                var cmd = new SqlCommand(cmdtext, conn);
                cmd.ExecuteNonQuery();
            }
        }

        protected override FooModel GetItemById(long id)
        {
            using (var conn = NewConnection())
            {
                var cmdtext = "SELECT * FROM FooModel WHERE ID = @ID";
                var cmd = new SqlCommand(cmdtext, conn);
                var dr = cmd.ExecuteReader();
                var hydrator = new EntityMemberResolver<FooModel>();
                dr.Read();
                var item = new FooModel();
                hydrator.HydrateFromDictionary(item,
                    hydrator.ConvertToDictionary(dr));
                return item;
            }
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void SqlDbProviderDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
