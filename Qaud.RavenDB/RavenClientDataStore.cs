using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Raven.Abstractions.Data;
using Raven.Client;

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

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public void Add(T item)
        {
            GetSession().Store(item, GetItemKey(item));
            if (AutoSave) SaveChanges();
        }

        public void Add(T item, out T result)
        {
            GetSession().Store(item, GetItemKey(item));
            SaveChanges();
            result = item;
        }

        public void AddRange(IEnumerable<T> items)
        {
            IDocumentSession session = GetSession();
            foreach (T item in items)
            {
                session.Store(item);
            }
            if (AutoSave) SaveChanges();
        }

        public IQueryable<T> Query
        {
            get { return GetSession().Query<T>(); }
        }

        public T FindMatch(T lookup)
        {
            var keyprops = _memberResolver.KeyPropertyMembers.ToArray();;
            if (!keyprops.Any()) throw new InvalidOperationException("Type does not have key columns: " + typeof(T).FullName);
            return GetSession().Load<T>(keyprops.Single().GetValue(lookup).ToString());
        }

        public T Find(params object[] keyvalue)
        {
            var key = keyvalue.Single().ToString();
            return GetSession().Load<T>(key);
        }

        public void Update(T item)
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

        public void UpdateRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Update(item);
            }
        }

        public void UpdatePartial(object changes)
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
        }

        public void Delete(T item)
        {
            IDocumentSession session = GetSession();
            var key = GetItemKey(item);
            item = session.Load<T>(key);
            session.Delete(item);
            if (AutoSave) SaveChanges();
        }

        public void DeleteByKey(params object[] keyvalue)
        {
            Delete(Find(keyvalue));
        }

        public void DeleteRange(IEnumerable<T> items)
        {
            foreach (T item in items) Delete(item);
        }

        public bool AutoSave { get; set; }

        public bool SupportsNestedRelationships
        {
            get { return false; }
        }

        public bool SupportsTransactionScope
        {
            get { return true; }
        }

        public void SaveChanges()
        {
            if (_session == null) return;

            _session.SaveChanges();
            _session.Dispose();
            _session = null;
        }

        /// <summary>
        ///     Returns the IDocumentStore responsible for document storage.
        /// </summary>
        public object DataSetImplementation
        {
            get { return _docStore; }
        }

        /// <summary>
        ///     Returns the session object that may or may not contain pending changes.
        /// </summary>
        public object DataContextImplementation
        {
            get { return _session; }
        }

        public void Dispose()
        {
            if (_session != null) _session.Dispose();
            if (_docStore != null) _docStore.Dispose();
        }

        private string GetItemKey(T item)
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

        private IDocumentSession GetSession()
        {
            return _session ?? (_session = _docStore.OpenSession());
        }


        public bool SupportsComplexStructures
        {
            get { return true; }
        }
    }
}