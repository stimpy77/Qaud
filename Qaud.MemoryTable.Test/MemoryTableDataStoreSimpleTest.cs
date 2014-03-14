using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qaud.Test;

namespace Qaud.MemoryTable.Test
{
    [TestClass]
    public class MemoryTableDataStoreSimpleTest : DataStoreSimpleTest
    {
        public class TestMemoryTableDataStore : DataTableDataStore<FooModel>
        {
            
        }
        public MemoryTableDataStoreSimpleTest()
            : base(new TestMemoryTableDataStore())
        {
        }

        private DataTable DataTable
        {
            get { return (DataTable) base.DataStore.DataSetImplementation; }
        }

        protected override void AddItemToStore(FooModel item)
        {
            // cheating; should serialize to this.DataTable
            base.DataStore.Add(item);
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            this.DataTable.Rows.Clear();
        }

        protected override FooModel GetItemById(long id)
        {
            // cheating; should return deserialized from this.DataTable
            return base.DataStore.Query.SingleOrDefault(item => item.ID == id);
        }

        [TestMethod]
        public void MemoryTableDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void MemoryTableDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void MemoryTableDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void MemoryTableDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void MemoryTableDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void MemoryTableDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void MemoryTableDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void MemoryTableDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void MemoryTableDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void MemoryTableDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
