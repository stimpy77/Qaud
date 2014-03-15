using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Qaud.MongoDB
{
    public class MongoDbDataStore<T> : IDataStore<T> 
    {
        private MongoDatabase _db;
        private MongoCollection<T> _collection;

        public MongoDbDataStore(MongoDatabase db)
            : this(db, ResolveCollectionName(typeof(T)))
        {
        }

        public MongoDbDataStore(MongoDatabase db, string collectionName)
        {
            _db = db;
            _collection = db.GetCollection<T>(collectionName);
        }

        public T Create()
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public T Find(params object[] key)
        {
            throw new NotImplementedException();
        }

        public void Update(T item)
        {
            throw new NotImplementedException();
        }

        public void Delete(params object[] key)
        {
            throw new NotImplementedException();
        }

        public T FindMatch(T lookup)
        {
            throw new NotImplementedException();
        }

        public void Add(T item, out T result)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public T UpdatePartial(object item)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(T item)
        {
            throw new NotImplementedException();
        }

        public void DeleteRange(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Query
        {
            get { return _collection.AsQueryable(); }
        }

        bool IDataStore<T>.AutoSave
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        bool IDataStore<T>.CanQueueChanges
        {
            get { throw new NotImplementedException(); }
        }

        bool IDataStore<T>.SupportsNestedRelationships
        {
            get { throw new NotImplementedException(); }
        }

        bool IDataStore<T>.SupportsComplexStructures
        {
            get { throw new NotImplementedException(); }
        }

        bool IDataStore<T>.SupportsTransactionScope
        {
            get { throw new NotImplementedException(); }
        }

        bool IDataStore<T>.SupportsGeneratedKeys
        {
            get { throw new NotImplementedException(); }
        }

        object IDataStore<T>.DataSet
        {
            get { return _collection; }
        }

        object IDataStore<T>.DataContext
        {
            get { return _db; }
        }
        private static string ResolveCollectionName(Type type)
        {
            // todo: use any clues from attributes or fluent descriptors that might define the collection name
            return type.Name.Substring(0, 1).ToLower() + type.Name.Substring(1);
        }
    }
}
