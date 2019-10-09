using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Qaud.Test;

namespace Qaud.EntityFramework.Test
{
    [TestClass]
    public class EntityFrameworkDataStoreSimpleTest : DataStoreSimpleTest
    {
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", context.TestDeploymentDir);
        }

        private bool _init;
        [TestInitialize]
        public void TestInitialize()
        {
            if (_init) return;
            SetDataSore(new TestEFDataStoreGenerator().Create());
            _dbset = (DbSet<FooModel>) DataStore.DataSet;
            _init = true;
        }

        private DbSet<FooModel> _dbset;
        public EntityFrameworkDataStoreSimpleTest()
            : base(null)
        {
        }

        protected override void AddItemToStore(FooModel item)
        {
            _dbset.Add(item);
        }

        protected override void CleanOutItemFromStore(FooModel item)
        {
            try
            {
                _dbset.Remove(item);
            }
            catch {}
        }
    

        protected override FooModel GetItemById(long id)
        {
            return _dbset.SingleOrDefault(item => item.ID == id);
        }

        [TestMethod]
        public void EFDataSetDataStore_Add_Item_Adds_Item()
        {
            base.DataStore_Add_Item_Adds_Item();
        }

        [TestMethod]
        public void EFDataSetDataStore_Create_Instantiates_T()
        {
            base.DataStore_Create_Instantiates_T();
        }

        [TestMethod]
        public void EFDataSetDataStore_DeleteByKey_Removes_Item()
        {
            base.DataStore_DeleteByKey_Removes_Item();
        }

        [TestMethod]
        public void EFDataSetDataStore_Delete_Item_Range_Removes_Many_Items()
        {
            base.DataStore_Delete_Item_Range_Removes_Many_Items();
        }

        [TestMethod]
        public void EFDataSetDataStore_Delete_Item_Range_Single_Removes_Item()
        {
            base.DataStore_Delete_Item_Range_Single_Removes_Item();
        }

        [TestMethod]
        public void EFDataSetDataStore_Delete_Item_Removes_Item()
        {
            base.DataStore_Delete_Item_Removes_Item();
        }

        [TestMethod]
        public void EFDataSetDataStore_Partial_Update_Modifies_Item()
        {
            base.DataStore_Partial_Update_Modifies_Item();
        }

        [TestMethod]
        public void EFDataSetDataStore_Query_For_All_Returns_All()
        {
            base.DataStore_Query_For_All_Returns_All();
        }

        [TestMethod]
        public void EFDataSetDataStore_Query_For_Item_Returns_Result()
        {
            base.DataStore_Query_For_Item_Returns_Result();
        }

        [TestMethod]
        public void EFDataSetDataStore_Update_Modifies_Item()
        {
            base.DataStore_Update_Modifies_Item();
        }
    }
}
