using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qaud.Test;

namespace Qaud.MemoryTable.Test
{
    [TestClass]
    public class DictionaryDataStoreSimpleTest : DataStoreSimpleTest
    {
        public DictionaryDataStoreSimpleTest() : base(new DictionaryDataStore<FooModel>())
        {
        }

        protected override void AddItemToStore(FooModel item)
        {
            ((IDictionary<string, FooModel>)DataStore.DataSet).Add(item.ID.ToString(), item);
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            ((IDictionary<string, FooModel>)DataStore.DataSet).Remove(item.ID.ToString());
        }

        protected override FooModel GetItemById(long id)
        {
            var dic = ((IDictionary<string, FooModel>) DataStore.DataSet);
            return dic[id.ToString()];
        }

        ///////////////////////////////////////////////////////////////

        [TestMethod]
        public void DictionaryDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void DictionaryDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void DictionaryDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void DictionaryDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void DictionaryDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void DictionaryDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void DictionaryDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void DictionaryDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void DictionaryDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void DictionaryDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
