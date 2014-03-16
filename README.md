QAUD (Query Add Update Delete) v0.2
==================================
##<sub><sup>(QAUD is not a word. Don't fix it.)</sup></sub>
QAUD is an interface plus implementations for QAUD, or CRUD, operations. 
It's ICrud, basically, intended for extending IQueryable-supporting data repositories with the promise of a 
basic set of alteration operations.

## Supported Implementations 

* An **Entity Framework** implementation is passing initial very basic tests.
* A **RavenDB client** implementation is passing initial very basic tests.
* A **MongoDB client** implementation is passing initial very basic tests.
* An in-memory **DataTable** implementation is passing initial very basic tests.
* An in-memory **SortedDictionary** implementation is passing initial very basic tests.
* Other ORMs and data repositories are yet to be added.

## Background

The objective behind QAUD is to facilitate a DAL (Data Access Layer) into prototype applications that do not know
the full measure of the technology behind the DAL. Most DALs start with key-based table or document structures, so this 
solution builds upon that premise.

For example, someone implementing a dynamic web site, such as an online store, might use `IDataStore<T>` for all
data storage operations because he might not know whether the deployed solution will build upon MongoDB, MySQL, 
SQL Server, or some other server. By using `IDataStore<T>` for all DAL, the prototype solution can switch the 
underlying database, NoSQL, or in-memory data storage implementation by only changing the IoC/DI initializer, 
assuming `IDataStore<T>` has been generically implemented for the chosen actual database implementation.

Building applications upon `IDataStore<T>` would only be recommended for accelerating **prototype application 
development** or for applications where **DAL targeting versatility is a priority concern**.
An enterprise environment that is heavily dependent upon the Microsoft stack, including SQL Server, would not
benefit from `IDataStore<T>` as much as a development team or individual that is focused on building an 
application with **an unknown or wide range of target environments and/or storage dependencies**.

Another situation is where an application that builds entirely upon `IDataStore<T>`, or at least upon 
`ICrud<T>`/`ICrudEx<T>`, can utilize in-memory implementations when implementing functional tests.

## `ICrud<T>` has come at last.

The base interface is `ICrud<T>`; the following summarizes it:

        public interface ICrud<T> : ICreate<T>, 
                                    IAddItem<T>, 
                                    IFind<T>, 
                                    IUpdate<T>, 
                                    IDelete
        {
        /* implemented by above declaration:

            T     Create ();
            void  Add    (T item);
            T     Find   (params object[] key);
            void  Update (T item);
            void  Delete (params object[] key);
        */
        }

.. plus extensions in `ICrudEx<T>` for common variations and convenience operations.

## Full Generic Repository Interface

The complete interface for a repository is `IDataStore<T>`; the following summarizes it:

        public interface IDataStore<T> : ICrudEx<T>, IHasQueryable<T>
        {

        /*  ICrudEx<T> includes these:

            T Create();
            void Add(T item);
            void Add(T item, out T result);
            void AddRange(IEnumerable<T> items);
            T FindMatch(T lookup);
            T Find(params object[] keyvalue);
            void Update(T item);
            void UpdateRange(IEnumerable<T> items);
            T UpdatePartial(object item);
            void Delete(params object[] keyvalue);
            void DeleteItem(T item);
            void DeleteRange(IEnumerable<T> items);

        */

        /*  IHasQueryable<T> includes this:

            IQueryable<T> Query { get; }
        */
            

            // if false, defers changes; false not always supported, see CanQueueChanges
            bool AutoSave { get; set; }

            // apply changes; noop if AutoSave == true
            void SaveChanges();

            // indicates whether AutoSave can be set to false
            void CanQueueChanges();

            // indicates support for "navigation properties" as with EF
            bool SupportsNestedRelationships { get; } 

            // indicates support for multilevel object graphs in one entry as with RavenDB
            bool SupportsComplexStructures { get; }

            // indicates support for [DatabaseGenerated(Identity)] 
            bool SupportsGeneratedKeys { get; }

            // indicates support for using(var scope = new TransactionScope()) { .. }
            bool SupportsTransactionScope { get; }

            // gets the underlying data table, data set, dictionary, or whatever is doing the work
            object DataSet { get; }

            // gets the object that contains the connection, if any, to the database
            object DataContext { get; }
        }


Note: Most of these non-CRUD support members explicitly declared on `IDataStore<T>` should be implemented 
explicitly, to conveniently hide from consumer code (i.e. from intellisense).

_____

A few usage notes:

1. IDataStore[T] is a generic interface to a repository as a *table structure*, *document structure*, or similar, not a schema / database.
2. ***The [System.ComponentModel.DataAnnotations.Key] attribute is required on at least one member of T in IDataStore[T].***
3. Any provider must support LINQ. The purpose of Qaud is to add `Add`, `Update`, and `Delete*` commands (plus some helpful touches such as `UpdatePartial()`) to IQueryable in a common and useful interface.
4. Provider implementations should ideally "hide" support members: `DataSetImplementation`, `DataContextImplementation`, `SupportsNestedRelationships`, `SupportsTransactionScope`, and `SupportsComplexStructures`. These are not interesting repository properties/methods for a repository interface consumer, but they are on the interface for identifying implementation behaviors, when you need to know them.
5. Some features require reflection, such as `UpdatePartial()`, which is like `Update()` but takes any object that has the same key field(s) and that has only the properties potentially containing changes. If you do not want to utilize any feature that uses reflection, do not use data provider implementations that impose it, and avoid these special interface member(s).

