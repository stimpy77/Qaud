using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Wrappers;

namespace Qaud.MongoDB
{
    public class MongoDbDataStore<T> : IDataStore<T> 
    {
        private MongoDatabase _db;
        private MongoCollection<T> _collection;
        private EntityMemberResolver<T> _memberResolver;
        private bool autoGenKey;

        public MongoDbDataStore(MongoDatabase db)
            : this(db, ResolveCollectionName(typeof(T)))
        {
        }

        public MongoDbDataStore(MongoDatabase db, string collectionName)
        {
            _db = db;
            _collection = db.GetCollection<T>(collectionName);
            _memberResolver = new EntityMemberResolver<T>();
            if (this.AutoConfig && !BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    var idKeys = _memberResolver.KeyPropertyMembers.ToArray();
                    var idkey = idKeys.SingleOrDefault();
                    if (idkey == null)
                    {
                        if (idKeys.Length > 0)
                        {
                            throw new InvalidOperationException(
                                "The MongoDb implementation of IDataStore<T> does not support multiple key members.");
                        }
                        throw new InvalidOperationException(
                            string.Format(
                                "System.ComponentModel.DataAnnotations.KeyAttribute must be associated with at least one member on {0} (even if BsonId is already used).",
                                typeof (T).Name));
                    }
                    cm.SetIdMember(cm.GetMemberMap(idkey.Name));
                    if (_memberResolver.IsAutoIdentity(idkey.Name))
                    {
                        if (idkey.PropertyType == typeof (string))
                        {
                            cm.IdMemberMap.SetIdGenerator(global::MongoDB.Bson.Serialization.IdGenerators.StringObjectIdGenerator.Instance);
                            autoGenKey = true;
                        }
                        if (idkey.PropertyType == typeof (Guid))
                        {
                            cm.IdMemberMap.SetIdGenerator(global::MongoDB.Bson.Serialization.IdGenerators.GuidGenerator.Instance);
                            autoGenKey = true;
                        }
                        if (idkey.PropertyType == typeof (ObjectId))
                        {
                            cm.IdMemberMap.SetIdGenerator(global::MongoDB.Bson.Serialization.IdGenerators.ObjectIdGenerator.Instance);
                            autoGenKey = true;
                        }
                        if (!autoGenKey)
                        {
                            throw new NotImplementedException("Auto generated key for type " + idkey.Name + " not yet supported.");
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Gets whether this implementation should resolve key-related configuration
        /// details and apply them to the database mappings. Override and return false
        /// if your model (<typeparamref name="T"/>) is preconfigured with Bson-related
        /// attributes or you have applied a BsonClassMap.
        /// </summary>
        protected virtual bool AutoConfig => true;

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        private void AutoSaveIfAutoSaveEnabled()
        {
            if (((IDataStore<T>)this).AutoSave)
            {
                ((IDataStore<T>)this).SaveChanges();
            }
        }

        public virtual void Add(T item)
        {
            _collection.Insert(item);
            AutoSaveIfAutoSaveEnabled();
        }

        public void Add(T item, out T result)
        {
            var writeresult = _collection.Insert(item);
            AutoSaveIfAutoSaveEnabled();
            result = item;
            if (((IDataStore<T>)this).SupportsGeneratedKeys && autoGenKey)
            {
                var idval = writeresult.Upserted;
                var idKeys = _memberResolver.KeyPropertyMembers.ToArray();
                var idkey = idKeys.SingleOrDefault();
                var idvalnatv = BsonSerializer.Deserialize(idval.ToBsonDocument(), idkey.PropertyType);
                idkey.SetValue(result, idvalnatv);
            }
        }

        private string GetElementName(string memberName)
        {
            return
                BsonClassMap.GetRegisteredClassMaps().First(cm => cm.ClassType == typeof(T))
                    .GetMemberMap(memberName).ElementName;
        }

        private IMongoQuery CreateQueryByKey(params object[] key)
        {
            var keyProps = _memberResolver.KeyPropertyMembers.ToArray();
            if (keyProps.Length != key.Length)
            {
                throw new ArgumentException("Key field count mismatch", "key");
            }
            var dic = new Dictionary<string, object>();
            for (var i = 0; i < keyProps.Length; i++)
            {
                var id = keyProps[i].Name;
                dic[id] = key[i];
            }
            var query = global::MongoDB.Driver.Builders.Query.And(
                dic.Select(kvp => global::MongoDB.Driver.Builders.Query.EQ(
                    GetElementName(kvp.Key), BsonValue.Create(kvp.Value))));
            return query;
        }

        public virtual T Get(params object[] key)
        {
            var query = CreateQueryByKey(key);
            return _collection.FindOne(query);
        }

        public void Update(T item)
        {
            var orig = Get(item);
            _memberResolver.ApplyChanges(orig, item);
            _collection.Save(orig);
            AutoSaveIfAutoSaveEnabled();
        }

        public void Delete(params object[] key)
        {
            var query = CreateQueryByKey(key);
            var result = _collection.Remove(query);
            if (result.DocumentsAffected == 0)
            {
                throw new KeyNotFoundException("Could not find item to delete.");
            }
            AutoSaveIfAutoSaveEnabled();
        }

        public void DeleteItem(T item)
        {
            var query = CreateQueryByKey(_memberResolver.GetKeyPropertyValues(item).ToArray());
            var result = _collection.Remove(query);
            if (result.DocumentsAffected == 0)
            {
                throw new KeyNotFoundException("Could not find item to delete.");
            }
            AutoSaveIfAutoSaveEnabled();
        }

        public void DeleteRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                DeleteItem(item);
            }
            AutoSaveIfAutoSaveEnabled();
        }

        public T Get(T lookup)
        {
            var query = CreateQueryByKey(_memberResolver.GetKeyPropertyValues(lookup).ToArray());
            return _collection.FindOne(query);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
            AutoSaveIfAutoSaveEnabled();
        }

        public void UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items) Update(item);
            AutoSaveIfAutoSaveEnabled();
        }

        public T UpdatePartial(object item)
        {
            var query = CreateQueryByKey(_memberResolver.GetKeyPropertyValues(item).ToArray());
            var orig = _collection.FindOne(query);
            var changed = orig;
            _memberResolver.ApplyPartial(changed, item);
            _collection.Save(orig);
            AutoSaveIfAutoSaveEnabled();
            return changed;
        }

        protected virtual IQueryable<T> Query => _collection.AsQueryable();

        public virtual IEnumerator<T> GetEnumerator()
        {
            return this.Query.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        Type IQueryable.ElementType => Query.ElementType;

        System.Linq.Expressions.Expression IQueryable.Expression => Query.Expression;

        IQueryProvider IQueryable.Provider => Query.Provider;

        bool IDataStore<T>.AutoSave
        {
            get => true;
            set => throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            //throw new NotImplementedException();
        }

        bool IDataStore<T>.CanQueueChanges => false;

        bool IDataStore<T>.SupportsNestedRelationships => false;

        bool IDataStore<T>.SupportsComplexStructures => true;

        bool IDataStore<T>.SupportsTransactionScope => false;

        bool IDataStore<T>.SupportsGeneratedKeys => true;

        object IDataStore<T>.DataSet => _collection;

        object IDataStore<T>.DataContext => _db;

        public string StoreName
        {
            get
            {
                // temporary implementation
                var tableAttr = typeof(T).GetCustomAttribute<TableAttribute>();
                if (tableAttr != null) return tableAttr.Name;
                return typeof(T).Name;
            }
        }

        private static string ResolveCollectionName(Type type)
        {
            // todo: use any clues from attributes or fluent descriptors that might define the collection name
            return type.Name.Substring(0, 1).ToLower() + type.Name.Substring(1);
        }
    }
}
