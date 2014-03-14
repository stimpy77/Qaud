﻿using System.Collections.Generic;
using System.Linq;

namespace Qaud
{
    /// <summary>
    /// Describes a data store that supports CRUD operations -- or rather, QAUD operations (Query, Add, Update, Delete).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataStore<T>
    {

        /// <summary>
        /// When implemented, creates a new instance of type <typeparamref name="T"/>, or derived from type <typeparamref name="T"/>. 
        /// </summary>
        /// <returns></returns>
        T Create();

        /// <summary>
        /// When implemented, inserts a <typeparamref name="T"/> object to this data store, or queues the insertion.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void Add(T item);

        /// <summary>
        /// When implemented, inserts a <typeparamref name="T"/> object to this data store and outputs the item with any mutations that occurred during the insertion.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        /// <param name="result">The item to be inserted.</param>
        /// <returns>The inserted item, along with any mutations that occurred during the insertion (such as adding identity keys).</returns>
        void Add(T item, out T result);

        /// <summary>
        /// When implemented, inserts a set of <typeparamref name="T"/> objects to this data store.
        /// </summary>
        /// <param name="items">The inserted items, along with any mutations that occurred during the insertion (such as adding identity keys).</param>
        /// <returns></returns>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// When implemented, returns an IQueryable&lt;<typeparamref name="T"/>&gt;.
        /// </summary>
        IQueryable<T> Query { get; }

        /// <summary>
        /// When implemented, performs a lookup based on the given item's key column(s). If the item is found as locally cached in a modified state, the modified cached item is returned.
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns>A matching entity, or null</returns>
        T FindMatch(T lookup);

        /// <summary>
        /// When implemented, performs a lookup based on the given item's key column(s). If the item is found as locally cached in a modified state, the modified cached item is returned.
        /// </summary>
        /// <param name="keyvalue">The key column value(s) to perform the lookup.</param>
        /// <returns>A matching entity, or null.</returns>
        T Find(params object[] keyvalue);

        /// <summary>
        /// When implemented, applies all changes to the specified <typeparamref name="T"/> object to this data store.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void Update(T item);

        /// <summary>
        /// When implemented, applies all changes to the specified <typeparamref name="T"/> objects to this data store.
        /// </summary>
        /// <param name="items">The items to be inserted.</param>
        void UpdateRange(IEnumerable<T> items);

        /// <summary>
        /// When implemented, applies changes to the specified <typeparamref name="T"/> object to this data store. Only properties included in the object are modified.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A fully populated instance of <typeparamref name="T"/>.</returns>
        T UpdatePartial(object item);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> object from this data store.
        /// </summary>
        /// <param name="item">The items to be deleted. Depending on how this is implemented, it is likely that only the identifying properties will be used.</param>
        void Delete(T item);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> object from this data store.
        /// </summary>
        /// <param name="keyvalue">The key(s) of the item to be deleted.</param>
        void DeleteByKey(params object[] keyvalue);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> objects from this data store.
        /// <remarks>If a "virtual delete" is to be used instead, expose a Deleted property and use <see cref="Update"/> instead.</remarks>
        /// </summary>
        /// <param name="items">The items to be deleted. Depending on how this is implemented, it is likely that only the identifying properties will be used.</param>
        void DeleteRange(IEnumerable<T> items);

        /// <summary>
        /// When implemented, gets or sets whether Add, Update, or Delete is immediately applied or if the change is deferred.
        /// </summary>
        bool AutoSave { get; set; }

        /// <summary>
        /// Gets whether the implementation supports nested relationships and Add or Update might 
        /// propagate related data, such as navigation properties in the case of Entity Framework.
        /// When false, data associated with this data store would only act as records or documents
        /// in a singular table or document store.
        /// </summary>
        bool SupportsNestedRelationships { get; }

        /// <summary>
        /// When implemented, gets whether a single property can be deserialized as a complete complex type automatically, whether
        /// via <see cref="SupportsNestedRelationships"/> (navigation properties) or via tree-based document storage.
        /// Returns false if the document store only supports flat table structures, with no relationships.
        /// </summary>
        bool SupportsComplexStructures { get; }

        /// <summary>
        /// When implemented, gets whether the data store implementation supports transaction scopes
        /// such as when using <code>using (var transaction = new TransactionScope()) { .. }</code>
        /// </summary>
        bool SupportsTransactionScope { get; }

        /// <summary>
        /// When implemented, gets whether the data store implementation supports 
        /// <see cref="System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute"/>, particularly
        /// <see cref="System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity"/>
        /// </summary>
        bool SupportsGeneratedKeys { get; }

        /// <summary>
        /// When implemented, applies changes made using <see cref="Add"/>, <see cref="Update"/>, and/or <see cref="Delete"/>.
        ///  This method should have no effect if <see cref="AutoSave"/> is set to <value>true</value>, in which case the changes would have already been applied.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// When implemented, returns the underlying data structure responsible for managing this data store.
        /// </summary>
        object DataSet { get; }

        /// <summary>
        /// When implemented, and if provided, returns the data context associated with the underlying data structure responsible for managing this data store.
        /// </summary>
        object DataContext { get; }
    }
}
