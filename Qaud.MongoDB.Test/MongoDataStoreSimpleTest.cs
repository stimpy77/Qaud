using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using Qaud.Test;

namespace Qaud.MongoDB.Test
{
    [TestClass]
    public class MongoDataStoreSimpleTest : DataStoreSimpleTest
    {
        public MongoDataStoreSimpleTest() : base(CreateMongoDataStore())
        {
        }

        private static MongoDbDataStore<FooModel> CreateMongoDataStore()
        {
            throw new NotImplementedException();
        }


        protected override void AddItemToStore(FooModel item)
        {
            throw new NotImplementedException();
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            throw new NotImplementedException();
        }

        protected override FooModel GetItemById(long id)
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void MongoDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void MongoDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void MongoDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void MongoDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void MongoDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void MongoDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void MongoDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void MongoDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void MongoDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void MongoDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
