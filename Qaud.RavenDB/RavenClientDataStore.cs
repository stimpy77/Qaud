using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;

namespace Qaud.RavenDB
{
    public class RavenClientDataStore<T> : IDataStore<T>, IDisposable
    {
        private readonly IDocumentStore _docStore;
        private readonly EntityMemberResolver<T> _memberResolver;
        private IDocumentSession _session;

        private RavenClientDataStore()
        {
            _memberResolver = new EntityMemberResolver<T>();
        }

        public RavenClientDataStore(IDocumentStore documentStore) : this()
        {
            _docStore = documentStore;
        }

        public virtual T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual void Add(T item)
        {
            GetSession().Store(item, GetItemKey(item));
            if (AutoSave) SaveChanges();
        }

        public virtual void Add(T item, out T result)
        {
            GetSession().Store(item, GetItemKey(item));
            SaveChanges();
            result = item;
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            IDocumentSession session = GetSession();
            foreach (T item in items)
            {
                session.Store(item);
            }
            if (AutoSave) SaveChanges();
        }

        public virtual IQueryable<T> Query
        {
            get { return GetSession().Query<T>(); }
        }

        public virtual T FindMatch(T lookup)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();;
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            return GetSession().Load<T>(keyprops.Single().GetValue(lookup).ToString());
        }

        public virtual T Find(params object[] keyvalue)
        {
            var key = keyvalue.Single().ToString();
            return GetSession().Load<T>(key);
        }

        public virtual void Update(T item)
        {
            string key = GetItemKey(item);
            IDocumentSession session = GetSession();
            var current = session.Load<T>(key);
            _memberResolver.ApplyChanges(current, item);
            session.Store(current);
            if (AutoSave)
            {
                SaveChanges();
            }
        }

        public virtual void UpdateRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Update(item);
            }
        }

        public virtual T UpdatePartial(object changes)
        {
            if (!_memberResolver.KeyPropertyMembers.Any())
            {
                throw new InvalidOperationException("The type does not have a KeyAttribute: " +
                                                    changes.GetType().FullName);
            }
            var key = _memberResolver.GetKeyPropertyValues(changes).Single().ToString();
            if (string.IsNullOrEmpty(key))
            {
                throw new MissingPrimaryKeyException("Invalid key in " + changes.GetType().FullName);
            }
            IDocumentSession session = GetSession();
            var target = session.Load<T>(key);
            _memberResolver.ApplyPartial(target, changes);
            session.Store(target);
            if (AutoSave) SaveChanges();
            return target;
        }

        public virtual void Delete(T item)
        {
            IDocumentSession session = GetSession();
            var key = GetItemKey(item);
            item = session.Load<T>(key);
            session.Delete(item);
            if (AutoSave) SaveChanges();
        }

        public virtual void DeleteByKey(params object[] keyvalue)
        {
            Delete(Find(keyvalue));
        }

        public virtual void DeleteRange(IEnumerable<T> items)
        {
            foreach (T item in items) Delete(item);
        }

        public virtual bool AutoSave { get; set; }

        bool IDataStore<T>.SupportsNestedRelationships
        {
            get { return false; }
        }

        bool IDataStore<T>.SupportsTransactionScope
        {
            get { return true; }
        }

        public virtual void SaveChanges()
        {
            if (_session == null) return;

            _session.SaveChanges();
            _session.Dispose();
            _session = null;
        }

        /// <summary>
        ///     Returns the IDocumentStore responsible for document storage.
        /// </summary>
        /// <remarks>This is "protected" for convenience not safety.</remarks>
        protected IDocumentStore DataSetImplementation
        {
            get { return _docStore; }
        }

        /// <summary>
        ///     Returns the IDocumentStore responsible for document storage.
        /// </summary>
        object IDataStore<T>.DataSetImplementation
        {
            get { return DataSetImplementation; }
        }

        /// <summary>
        ///     Returns the session object that may or may not contain pending changes.
        /// </summary>
        /// <remarks>This is "protected" for convenience not safety.</remarks>
        protected IDocumentSession DataContextImplementation
        {
            get { return GetSession(); }
        }

        /// <summary>
        ///     Returns the session object that may or may not contain pending changes.
        /// </summary>
        object IDataStore<T>.DataContextImplementation
        {
            get { return DataContextImplementation; }
        }


        bool IDataStore<T>.SupportsComplexStructures
        {
            get { return true; }
        }

        public virtual void Dispose()
        {
            if (_session != null) _session.Dispose();
            if (_docStore != null) _docStore.Dispose();
        }

        protected virtual string GetItemKey(T item)
        {
            if (!_memberResolver.KeyPropertyMembers.Any())
            {
                throw new InvalidOperationException("The type does not have a KeyAttribute: " + item.GetType().FullName);
            }
            var key = _memberResolver.GetKeyPropertyValues(item).Single().ToString();
            if (string.IsNullOrEmpty(key))
            {
                throw new MissingPrimaryKeyException("Invalid key in " + item.GetType().FullName);
            }
            return key;
        }

        protected virtual IDocumentSession GetSession()
        {
            return _session ?? (_session = _docStore.OpenSession());
        }
    }
}