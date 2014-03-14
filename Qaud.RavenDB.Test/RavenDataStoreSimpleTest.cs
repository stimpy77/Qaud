using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qaud.Test;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Qaud.RavenDB.Test
{
    [TestClass]
    public class RavenDataStoreSimpleTest : DataStoreSimpleTest
    {
        public class TestRavenDataStore : RavenClientDataStore<FooModel>, IDisposable
        {
            public TestRavenDataStore() : base(CreateDocumentStore())
            {
            }

            private static DocumentStore CreateDocumentStore()
            {
                var ret = new EmbeddableDocumentStore()
                {
                    Configuration =
                    {
                        RunInUnreliableYetFastModeThatIsNotSuitableForProduction = true,
                        RunInMemory = true,
                    }
                };
                ret.Initialize();
                return ret;
            }

            void IDisposable.Dispose()
            {
                ((DocumentStore)((IDataStore<FooModel>)this).DataSetImplementation).Dispose();
                base.Dispose();
            }
        }
        public RavenDataStoreSimpleTest() : base(new TestRavenDataStore())
        {
        }

        private DocumentStore DocumentStore
        {
            get { return (DocumentStore)base.DataStore.DataSetImplementation; }
        }

        protected override void AddItemToStore(FooModel item)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(item, item.ID.ToString());
                session.SaveChanges();
            }
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            using (var session = DocumentStore.OpenSession())
            {
                var refitem = session.Load<FooModel>(item.ID.ToString());
                session.Delete(refitem);
                session.SaveChanges();
            }
        }

        protected override FooModel GetItemById(long id)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Load<FooModel>(id.ToString());
            }
        }

        [TestMethod]
        public void RavenDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void RavenDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void RavenDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void RavenDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void RavenDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void RavenDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void RavenDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void RavenDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void RavenDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void RavenDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
