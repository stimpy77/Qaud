﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qaud
{
    /// <summary>
    /// A CRUD interface with support for an extended set of more versatile variations of CRUD operations. 
    /// </summary>
    /// <seealso cref="ICrud{T}"/>
    /// <seealso cref="IGetEx{T}"/>
    /// <seealso cref="IAddItemEx{T}"/>
    /// <seealso cref="IUpdateEx{T}"/>
    /// <seealso cref="IDeleteEx{T}"/>
    /// <seealso cref="ICreate{T}"/>
    /// <seealso cref="IAddItem{T}"/>
    /// <seealso cref="IGet{T}"/>
    /// <seealso cref="IUpdate{T}"/>
    /// <seealso cref="IDelete"/>
    /// <typeparam name="T"></typeparam>
    public interface ICrudEx<T> : ICrud<T>, IGetEx<T>, IAddItemEx<T>, IUpdateEx<T>, IDeleteEx<T>
    {
    }

    /// <summary>
    /// A data repository that supports Create (and Add), read (Get), Update, and Delete. 
    /// </summary>
    /// <seealso cref="ICreate{T}"/>
    /// <seealso cref="IAddItem{T}"/>
    /// <seealso cref="IGet{T}"/>
    /// <seealso cref="IUpdate{T}"/>
    /// <seealso cref="IDelete"/>
    /// <typeparam name="T"></typeparam>
    public interface ICrud<T> : ICreate<T>, IAddItem<T>, IGet<T>, IUpdate<T>, IDelete, IQueryable<T>
    {
    }

    /// <summary>
    /// An extended data repository that supports Create (and Add), read (Get), Update, and Delete, 
    /// plus additional interfaces to add versatility to these operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICreate<T>
    {
        /// <summary>
        /// When implemented, creates a new instance of type <typeparamref name="T"/>, or derived from type 
        /// <typeparamref name="T"/>. 
        /// </summary>
        /// <returns></returns>
        T Create();
    }

    
    /// <summary>
    /// Indicates that the object is a container that supports the <see cref="Add"/> operation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAddItem<T>
    {


        /// <summary>
        /// When implemented, inserts a <typeparamref name="T"/> object to this data store, or queues the insertion.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void Add(T item);
    }

    /// <summary>
    /// Indicates that the implementation is a container that supports an extension of the <see cref="Add"/> operation.
    /// </summary>
    /// <seealso cref="IAddItem{T}"/>
    /// <typeparam name="T"></typeparam>
    public interface IAddItemEx<T> : IAddItem<T>
    {

        /// <summary>
        /// When implemented, inserts a <typeparamref name="T"/> object to this data store and outputs the item with 
        /// any mutations that occurred during the insertion.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        /// <param name="result">The item to be inserted.</param>
        /// <returns>The inserted item, along with any mutations that occurred during the insertion (such as adding 
        /// identity keys).</returns>
        void Add(T item, out T result);

        /// <summary>
        /// When implemented, inserts a set of <typeparamref name="T"/> objects to this data store.
        /// </summary>
        /// <param name="items">The inserted items, along with any mutations that occurred during the insertion 
        /// (such as adding identity keys).</param>
        /// <returns></returns>
        void AddRange(IEnumerable<T> items);
    }

    /// <summary>
    /// Indicates that the implementation is a container or search interface that can <see cref="Get"/> an item
    /// given a key. The key can be a composite key of multiple values; if the implementation does not support
    /// composite keys, it may combine the key values into a single value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGet<T>
    {
        /// <summary>
        /// When implemented, performs a lookup based on the given item's key column(s). If the item is found as 
        /// locally cached in a modified state, the modified cached item is returned.
        /// </summary>
        /// <param name="key">The key column value(s) to perform the lookup.</param>
        /// <returns>A matching entity, or null.</returns>
        T Get(params object[] key);
    }

    /// <summary>
    /// Indicates that the implementation is a container or search interface that can Get an item
    /// given a key. The key can be pulled out of another instance of <typeparamref name="T"></typeparamref> using
    /// <see cref="Get"/>.
    /// </summary>
    /// <seealso cref="IGet{T}"/>
    /// <typeparam name="T"></typeparam>
    public interface IGetEx<T> : IGet<T>
    {
        /// <summary>
        /// When implemented, performs a lookup based on the given item's key column(s). If the item is found as 
        /// locally cached in a modified state, the modified cached item is returned.
        /// </summary>
        /// <param name="lookup"></param>
        /// <returns>A matching entity, or null</returns>
        T Get(T lookup);

    }

    /// <summary>
    /// Indicates that the implementation supports updating an item in a repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUpdate<T>
    {
        /// <summary>
        /// When implemented, applies all changes to the specified <typeparamref name="T"/> object to this data store.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void Update(T item);
    }

    /// <summary>
    /// Indicates that the implementation supports updating an item in a repository. It also has convenience extensions
    /// to update multiple items as well as to update an item given a partial type.
    /// 
    /// <example>
    /// For example, if updating only the FirstName and LastName properties of a detailed Contact, the type of the 
    /// object passed into <see cref="UpdatePartial"/> might contain only ID, FirstName, and LastName as its members.
    /// </example> 
    /// </summary>
    /// <seealso cref="IUpdate{T}"/>
    /// <typeparam name="T"></typeparam>
    public interface IUpdateEx<T> : IUpdate<T>
    {
        /// <summary>
        /// When implemented, applies all changes to the specified <typeparamref name="T"/> objects to this data store.
        /// </summary>
        /// <param name="items">The items to be inserted.</param>
        void UpdateRange(IEnumerable<T> items);

        /// <summary>
        /// When implemented, applies changes to the specified <typeparamref name="T"/> object to this data store. 
        /// Only properties included in the object are modified. The declaration of <typeparamref name="T" /> must 
        /// have at least one member with <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/>.
        /// 
        /// <example>
        /// For example, if updating only the FirstName and LastName properties of a detailed Contact, the type of the 
        /// object passed into <see cref="UpdatePartial"/> might contain only ID, FirstName, and LastName as its members.
        /// </example> 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A fully populated instance of <typeparamref name="T"/>.</returns>
        T UpdatePartial(object item);
    }

    /// <summary>
    /// Indicates that the implementation is a container that supports removing an item given its key.
    /// </summary>
    public interface IDelete
    {
        /// <summary>
        /// When implemented, deletes and destroys the object having the specified key from this data store.
        /// </summary>
        /// <param name="key">The key(s) of the item to be deleted.</param>
        void Delete(params object[] key);
    }

    /// <summary>
    /// Indicates that the implementation is a container that supoprts removing an item given a key, or given one
    /// or more instances of items. The declaration of <typeparamref name="T" /> must have at least one member
    /// with <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/>.
    /// </summary>
    /// <seealso cref="IDelete"/>
    public interface IDeleteEx<T> : IDelete
    {
        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> object from this data store.
        /// The declaration of <typeparamref name="T" /> must have at least one member with 
        /// <see cref="System.ComponentModel.DataAnnotations.KeyAttribute"/>.
        /// </summary>
        /// <param name="item">The items to be deleted. Depending on how this is implemented, it is likely that only 
        /// the identifying properties will be used.</param>
        void DeleteItem(T item);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> objects from this data store.
        /// <remarks>If a "virtual delete" is to be used instead, expose a Deleted property and use <see cref="IUpdate{T}.Update"/> 
        /// instead.</remarks>
        /// </summary>
        /// <param name="items">The items to be deleted. Depending on how this is implemented, it is likely that only 
        /// the identifying properties will be used.</param>
        void DeleteRange(IEnumerable<T> items);
    }
}
