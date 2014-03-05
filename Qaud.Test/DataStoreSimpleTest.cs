using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Qaud.Test
{
    [TestClass]
    public abstract class DataStoreSimpleTest
    {
        private IDataStore<FooModel> _dataStore;

        protected DataStoreSimpleTest(IDataStore<FooModel> dataStore)
        {
            _dataStore = dataStore;
        }

        [TestMethod]
        public virtual void DataStore_Create_Instantiates_T()
        {
            // Arrange
            // nothing to do

            // Act
            var result = _dataStore.Create();

            // Assert
            Assert.IsNotNull(result);
            result.AutoPopulate();
        }

        [TestMethod]
        public virtual void DataStore_Add_Item_Adds_Item()
        {
            // Arrange
            FooModel item = _dataStore.Create();
            item.AutoPopulate();

            // Act
            _dataStore.Add(item);
            _dataStore.SaveChanges();
            var result = GetItemById(item.ID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(item.ID, result.ID);
            Assert.AreEqual(item.Title, result.Title);
            Assert.AreEqual(item.CreateDate, result.CreateDate);
            Assert.AreEqual(item.Content, result.Content);
            if (_dataStore.SupportsComplexStructures)
            {
                Assert.IsNotNull(result.Comments);
                Assert.AreEqual(item.Comments.Count, result.Comments.Count);
                Assert.AreEqual(item.Comments.FirstOrDefault(), result.Comments.FirstOrDefault());
            }

            // cleanup
            CleanOutItemFromStore(item);
        }

        [TestMethod]
        public virtual void DataStore_Query_For_Item_Returns_Result()
        {
            // Arrange
            FooModel item = _dataStore.Create();
            item.AutoPopulate();
            AddItemToStore(item);
            SaveChanges();

            // Act
            var result = _dataStore.Query.SingleOrDefault(it => it.ID == item.ID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(item.ID, result.ID);
            Assert.AreEqual(item.Title, result.Title);
            Assert.AreEqual(item.CreateDate, result.CreateDate);
            Assert.AreEqual(item.Content, result.Content);
            if (_dataStore.SupportsComplexStructures)
            {
                Assert.IsNotNull(result.Comments);
                Assert.AreEqual(item.Comments.Count, result.Comments.Count);
                Assert.AreEqual(item.Comments.FirstOrDefault(), result.Comments.FirstOrDefault());
            }

            // cleanup
            CleanOutItemFromStore(item);
        }

        [TestMethod]
        public virtual void DataStore_Query_For_All_Returns_All()
        {
            // Arrange
            var items = new List<FooModel>();
            for (var i = 0; i < 25; i++)
            {
                var item = _dataStore.Create();
                item.AutoPopulate();
                item.ID = i+1;
                items.Add(item);
                AddItemToStore(item);
            }
            SaveChanges();

            // Act
            var result = _dataStore.Query.OrderBy(item=>item.ID).ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 25);
            for (var i = 0; i < 25; i++)
            {
                Assert.IsNotNull(result[i]);
                Assert.AreEqual(result[i].ID, items[i].ID);
                Assert.AreEqual(result[i].Title, items[i].Title);
                Assert.AreEqual(result[i].CreateDate, items[i].CreateDate);
                Assert.AreEqual(result[i].Content, items[i].Content);
                if (_dataStore.SupportsComplexStructures)
                {
                    Assert.IsNotNull(result[i].Comments);
                    Assert.AreEqual(result[i].Comments.Count, items[i].Comments.Count);
                    Assert.AreEqual(result[i].Comments.FirstOrDefault(), items[i].Comments.FirstOrDefault());
                }
            }
            
            // cleanup
            foreach (var item in items)
                CleanOutItemFromStore(item);
        }

        [TestMethod]
        public virtual void DataStore_Update_Modifies_Item()
        {
            // Arrange
            var item = _dataStore.Create();
            item.AutoPopulate();
            AddItemToStore(item);
            SaveChanges();

            // Act
            item.Content = "Modified";
            _dataStore.Update(item);
            _dataStore.SaveChanges();
            var result = GetItemById(item.ID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(item.ID, result.ID);
            Assert.AreEqual(item.Content, result.Content);

            // cleanup
            CleanOutItemFromStore(result);
        }

        [TestMethod]
        public virtual void DataStore_Partial_Update_Modifies_Item()
        {
            // Arrange
            var item = _dataStore.Create();
            item.AutoPopulate();
            AddItemToStore(item);
            SaveChanges();

            // Act
            var modified = new
            {
                ID = item.ID,
                Content = "Modified"
            };
            _dataStore.UpdatePartial(modified);
            _dataStore.SaveChanges();
            var result = GetItemById(item.ID);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(modified.ID, result.ID);
            Assert.AreEqual(modified.Content, result.Content);
            // all else is the same ..
            Assert.AreEqual(item.Title, result.Title);
            Assert.AreEqual(item.CreateDate, result.CreateDate);
            if (_dataStore.SupportsComplexStructures)
            {
                Assert.IsNotNull(result.Comments);
                Assert.AreEqual(item.Comments.Count, result.Comments.Count);
                Assert.AreEqual(item.Comments.FirstOrDefault(), result.Comments.FirstOrDefault());
            }

            // cleanup
            CleanOutItemFromStore(result);
        }

        [TestMethod]
        public virtual void DataStore_DeleteByKey_Removes_Item()
        {
            // Arrange 
            var item = _dataStore.Create();
            AddItemToStore(item);
            SaveChanges();

            // Act
            _dataStore.DeleteByKey(item.ID);
            _dataStore.SaveChanges();
            FooModel result = null;
            FooModel queryresult = null;
            try
            {
                result = GetItemById(item.ID);
                queryresult = _dataStore.Query.SingleOrDefault(v => v.ID == item.ID);
            }
            catch { }

            // Assert
            Assert.IsNull(result);
            Assert.IsNull(queryresult);
        }

        [TestMethod]
        public virtual void DataStore_Delete_Item_Removes_Item()
        {
            // Arrange 
            var item = _dataStore.Create();
            AddItemToStore(item);
            SaveChanges();

            // Act
            _dataStore.Delete(item);
            _dataStore.SaveChanges();
            FooModel result = null;
            FooModel queryresult = null;
            try
            {
                result = GetItemById(item.ID);
                queryresult = _dataStore.Query.SingleOrDefault(v => v.ID == item.ID);
            }
            catch { }

            // Assert
            Assert.IsNull(result);
            Assert.IsNull(queryresult);
        }

        [TestMethod]
        public virtual void DataStore_Delete_Item_Range_Single_Removes_Item()
        {
            // Arrange 
            var item = _dataStore.Create();
            AddItemToStore(item);
            SaveChanges();

            // Act
            _dataStore.DeleteRange(new[] { item });
            _dataStore.SaveChanges();
            FooModel result = null;
            FooModel queryresult = null;
            try
            {
                result = GetItemById(item.ID);
                queryresult = _dataStore.Query.SingleOrDefault(v => v.ID == item.ID);
            }
            catch { }

            // Assert
            Assert.IsNull(result);
            Assert.IsNull(queryresult);
        }

        [TestMethod]
        public virtual void DataStore_Delete_Item_Range_Removes_Many_Items()
        {
            // Arrange
            var items = new List<FooModel>();
            for (var i = 0; i < 25; i++)
            {
                var item = _dataStore.Create();
                item.ID = i + 1;
                items.Add(item);
                AddItemToStore(item);
            }
            SaveChanges();

            // Act
            var items2 = items.Select(item => GetItemById(item.ID)).ToList();
            _dataStore.DeleteRange(items2);
            _dataStore.SaveChanges();

            // Assert
            for (var i = 0; i < 25; i++)
            {
                FooModel result = null;
                try { 
                    result = GetItemById(items[i].ID);
                }
                catch { }
                Assert.IsNull(result);
            }
        }

        /// <summary>
        /// Item is added directly to the storage mechanism via <see cref="DataStore"/>.DataSetImplementation.
        /// </summary>
        /// <param name="item"></param>
        protected abstract void AddItemToStore(FooModel item);
        /// <summary>
        /// Item is removed directly from the storage mechanism via <see cref="DataStore"/>.DataSetImplementation
        /// if the item exists.
        /// </summary>
        /// <param name="item"></param>
        protected abstract void CleanOutItemFromStore(FooModel item);
        /// <summary>
        /// Item is returned directly from the storage mechanism via <see cref="DataStore"/>.DataSetImplementation.
        /// A <value>null</value> value is returned if the item does not exist.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected abstract FooModel GetItemById(long id);
        /// <summary>
        /// Returns the <see cref="IDataStore{FooModel}"/> being tested.
        /// </summary>
        protected IDataStore<FooModel> DataStore { get { return _dataStore; } }
        /// <summary>
        /// Save changes made via <see cref="AddItemToStore"/>, <see cref="CleanOutItemFromStore"/>, or <see cref="GetItemById"/>.
        /// </summary>
        protected virtual void SaveChanges()
        {
            _dataStore.SaveChanges();
        }
    }
}
