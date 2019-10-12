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
        private const string TEST_DATABASE_NAME = "QAUD_DBTEST_DBPROVIDER";
        private const string TEST_CONNECTION_STRING = "Data Source=(localdb)\\mssqllocaldb;Integrated Security=true;Database=" + TEST_DATABASE_NAME + ";";
        public SqlDbProviderDataStoreSimpleTest()
            : base(getDataStoreProvider())
        {
            RecreateTable();
        }

        private static IDataStore<FooModel> getDataStoreProvider()
        {
            var factory = SqlClientFactory.Instance;
            var dataStore = new DbProviderDataStore<FooModel>(factory, TEST_CONNECTION_STRING);
            return dataStore;
        }

        private SqlConnection NewConnection(bool autoOpen = true)
        {
            var conn = new SqlConnection(TEST_CONNECTION_STRING);
            if (autoOpen) conn.Open();
            return conn;
        }

        protected virtual void RecreateTable()
        {
            var cmdCreateDatabase = "CREATE DATABASE QAUD_DBTEST_DBPROVIDER";
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
                using (var newdb = NewConnection(false))
                {
                    newdb.ConnectionString = TEST_CONNECTION_STRING.Substring(0, TEST_CONNECTION_STRING.IndexOf("Database="));
                    newdb.Open();
                    using(var cmd = new SqlCommand(cmdCreateDatabase, newdb))
                        cmd.ExecuteNonQuery();
                }
            }
            catch {} // if db exists that's fine
            try
            {
                using (var cmd2 = new SqlCommand(cmdDrop, NewConnection()))
                    cmd2.ExecuteNonQuery();
            }
            catch { }
            using( var cmd3 = new SqlCommand(cmdCreate, NewConnection()))
                cmd3.ExecuteNonQuery();
        }

        protected override void AddItemToStore(FooModel item)
        {
            using (var conn = NewConnection())
            {
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
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", item.ID);
                    cmd.Parameters.AddWithValue("@CreateDate", item.CreateDate);
                    cmd.Parameters.AddWithValue("@Title", (object)item.Title ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Content", (object)item.Content ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            using (var conn = NewConnection())
            {
                var cmdtext = "DELETE FROM FooModel WHERE ID = @ID";
                var cmd = new SqlCommand(cmdtext, conn);
                cmd.Parameters.AddWithValue("@ID", item.ID);
                cmd.ExecuteNonQuery();
            }
        }

        protected override FooModel GetItemById(long id)
        {
            using (var conn = NewConnection())
            {
                var cmdtext = "SELECT * FROM FooModel WHERE ID = @ID";
                var cmd = new SqlCommand(cmdtext, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                var dr = cmd.ExecuteReader();
                var hydrator = new EntityMemberResolver<FooModel>();
                dr.Read();
                var item = new FooModel();
                hydrator.HydrateFromDictionary(item,
                    dr.RowAsDictionary());
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
