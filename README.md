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

## `ICrud<T>` has come at last.

The base interface is `ICrud<T>`; the following summarizes it:

        public interface ICrud<T> : ICreate<T>,    /* Create --
                                    IAddItem<T>,    ----------- */
                                    IQueryable<T>, /* Read --
                                    IGet<T>,        ----------- */
                                    IUpdate<T>,    /* Update -- */
                                    IDelete        /* Delete -- */
        {
        /* implemented by above declaration:

            T     Create ();
            void  Add    (T item);
            T     Get    (params object[] key);
            void  Update (T item);
            void  Delete (params object[] key);
        */
        }

.. plus extensions in `ICrudEx<T>` for common variations and convenience operations.

## Full Generic Repository Interface

The complete interface for a repository is `IDataStore<T>`; the following summarizes it:

        public interface IDataStore<T> : ICrudEx<T>
        {

        /*  ICrudEx<T> includes these:

            T Create();
            void Add(T item);
            void Add(T item, out T result);
            void AddRange(IEnumerable<T> items);
            T Get(T lookup);
            T Get(params object[] keyvalue);
            void Update(T item);
            void UpdateRange(IEnumerable<T> items);
            T UpdatePartial(object item);
            void Delete(params object[] keyvalue);
            void DeleteItem(T item);
            void DeleteRange(IEnumerable<T> items);

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

### Objectives and Background

The objective behind QAUD is have a **single interface definition for the most basic database / "document" 
storage access across disparate technology implementations**, extending IQueryable for *read* with 
interfaces for *add*, *update*, and *delete* operations. QAUD facilitates a trivial DAL (Data Access 
Layer) into prototype applications that does not know the full measure of the technology behind the DAL, 
it only ensures that expectations of CRUD (Create, Read, Update, Delete) are met with a consistent 
interface. Most DALs start with key-based table or document structures, so this solution builds upon that 
premise.

Frustrations have arisen where it has been observed that IQueryable seems to be supported everywhere, in 
practically all data access libraries and tools, which makes it very easy to read from a vast multitude 
of data sources without being concerned about the driver or provider of the data source, but no effort 
has been made to consolidate the other data access operations of adding, updating, and deleting data. 
This is what drove the initial creation of this project.

### Not A Complete Abstraction

It is important to recognize that QAUD is *not* intended to be a complete abstraction for existing 
abstractions such as Entity Framework or NHibernate. The objective of QAUD is to **complement** any of
these abstractions, as well as any database provider, with a consistent interface for basic CRUD 
operations that an application can invoke when performing these trivial tasks. All other operations 
that go beyond these basic operations must still be implemented using the existing tooling. *In some 
scenarios,* the effective footprint of consolidation of CRUD operations can achieve a very high 
percentage of the total DAL to be interchangable and generic.

The suggested use is that applications use a hybrid of basic and obvious CRUD operations through QAUD, 
while more advanced operations are handled by the underlying data provider.

Otherwise, those ideals of total abstraction are achievable, but only given a number of parameters, 
such as:

- The application is not a database-centric application, but does have trivial data to store.
- Deeply nested complex object loading with server-side joins can be avoided without severely hampering
performance; client-side joins are acceptable.
- All persistable entities can be persisted as either single table rows or as single documents.
- All database operations are limited to basic CRUD.

### Not Another O/RM (but then again ...)

O/RMs (Object-to-Relational Mappers) serve an important role in a developer's life, which is primarily
to write data access code without writing it directly in SQL. Several approaches have been made over the 
years to provide productivity tooling to developers to support interacting with a database with as little
effort as feasibly possible. The "holy grails" first appeared for .NET in NHibernate, the .NET migration
of Hibernate, followed by LINQ-to-SQL, followed by Entity Framework "Magical Unicorn Edition" (which 
supports the Code First programming model).

But what if one wants to switch from NHibernate to Entity Framework? Or what if one begins targeting SQL Server 
on Windows and get comfortable with using Entity Framework, but then decides to instead target MongoDB or 
Cassandra? If we could use IQueryable (LINQ) everywhere, we could get away with making these changes very 
quickly without much coding effort, but the reality is that we must also add, update, and delete data, 
and implementations of these operations vary wildly between O/RMs and database providers.

If one is willing to choose to use simpler conventions of data access and write all data access code through
generic interfaces such that **all data is stored either in tables or in a "document", and all entities have 
a key**, one can jump from any data provider to another without modifying any code except for initial setup 
such as in dependency injection initializers. This is the ideal; unfortunately, in typical scenarios this
ideal it simplistic and not achievable in larger applications. The goal of QAUD is to get us a number of 
steps towards it, or to achieve it entirely in trivial prototype solutions. 

In QAUD's case, versatility comes at a few costs, starting with price of relationship awareness. It would be 
entirely dependent upon the data provider or O/RM to break down related entities into their relationships. 
Fortunately, when using a relational-aware O/RM with QAUD, the expected behavior as with "navigation 
properties" can still work as before, the developer will just need to be aware of his limitations when 
switching implementations to something relationally unaware, such as Cassandra.

To the extent that the objective of this project is to support entity persistence with as minimum coding 
effort as possible using a common and consistent interface of CRUD operations, QAUD implementations fit 
the same objectives of convenience and interface consistency as O/RMs, **by relying upon them and 
consolidating the CRUD operation interfaces between them to only one**. However, to the extent that O/RMs 
also make efforts to support relationships between entitites, as well as offer other highly productive, 
performance, and/or otherwise beneficial features, QAUD does not qualify as an O/RM.

### Scenarios

Someone implementing a dynamic web site, such as an online store, might use `IDataStore<T>` for all
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

_____

A few usage notes:

1. IDataStore[T] is a generic interface to a repository as a *table structure*, *document structure*, or similar, not a schema / database.
2. ***The [System.ComponentModel.DataAnnotations.Key] attribute is required on at least one member of T in IDataStore[T].***
3. Any provider must support LINQ. The purpose of Qaud is to add `Add`, `Update`, and `Delete*` commands (plus some helpful touches such as `UpdatePartial()`) to IQueryable in a common and useful interface.
4. Provider implementations should ideally "hide" support members: `DataSetImplementation`, `DataContextImplementation`, `SupportsNestedRelationships`, `SupportsTransactionScope`, and `SupportsComplexStructures`. These are not interesting repository properties/methods for a repository interface consumer, but they are on the interface for identifying implementation behaviors, when you need to know them.
5. Some features require reflection, such as `UpdatePartial()`, which is like `Update()` but takes any object that has the same key field(s) and that has only the properties potentially containing changes. If you do not want to utilize any feature that uses reflection, do not use data provider implementations that impose it, and avoid these special interface member(s).

