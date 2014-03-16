using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Qaud.Test;

namespace Qaud.MongoDB.Test
{
    [TestClass]
    public class MongoDataStoreSimpleTest : DataStoreSimpleTest
    {
        private static MongoClient client;
        private static MongoServer server;
        private static MongoDatabase db;

        public MongoDataStoreSimpleTest() : base(CreateDataStore())
        {
            
        }

        internal static MongoDbDataStore<FooModel> CreateDataStore()
        {
            client = new MongoClient();
            server = client.GetServer();
            server.Connect();
            db = server.GetDatabase("qaudtest");
            db.DropCollection("fooModel");
            var dataStore = new MongoDbDataStore<FooModel>(db);
            return dataStore;
        }

        
        protected override void AddItemToStore(FooModel item)
        {
            var collection = (MongoCollection<FooModel>) ((IDataStore<FooModel>) DataStore).DataSet;
            collection.Insert(item);
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            var collection = (MongoCollection<FooModel>)((IDataStore<FooModel>)DataStore).DataSet;
            var query = global::MongoDB.Driver.Builders.Query.EQ(
                GetElementName<FooModel>("ID"), new BsonInt64(item.ID));
            collection.Remove(query);
        }

        protected override FooModel GetItemById(long id)
        {
            var collection = (MongoCollection<FooModel>)((IDataStore<FooModel>)DataStore).DataSet;
            var query = global::MongoDB.Driver.Builders.Query.EQ(
                GetElementName<FooModel>("ID"), new BsonInt64(id));
            return collection.Find(query).FirstOrDefault();
        }

        private string GetElementName<T>(string memberName)
        {
            return
                BsonClassMap.GetRegisteredClassMaps().First(cm => cm.ClassType == typeof (T))
                    .GetMemberMap(memberName).ElementName;
        }

        private new MongoDbDataStore<FooModel> DataStore
        {
            get { return (MongoDbDataStore<FooModel>)base.DataStore; }
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
