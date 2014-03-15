QAUD (Query Add Update Delete) v0.2
==================================
<sub><sup>(QAUD is not a word. Don't fix it.)</sup></sub>
QAUD is an interface plus implementations for QAUD, or CRUD, operations. It's ICrud, basically.

The base interface is `ICrud<T>`:

        public interface ICrud<T> : ICreate<T>, 
                                    IAddItem<T>, 
                                    IFind<T>, 
                                    IUpdate<T>, 
                                    IDelete
        {
            T     Create ();
            void  Add    (T item);
            T     Find   (params object[] key);
            void  Update (T item);
            void  Delete (params object[] key);
        }

.. plus extensions in `ICrudEx<T>` for common variations and convenience operations.

The complete interface for a repository is `IDataStore<T>`:

        public interface IDataStore<T> : ICrudEx<T>, IHasQueryable<T>
        {
        /*ICrudEx<T> includes these:
            T Create();
            void Add(T item);
            void Add(T item, out T result);
            void AddRange(IEnumerable<T> items);
            IQueryable<T> Query { get; }
            T FindMatch(T lookup);
            T Find(params object[] keyvalue);
            void Update(T item);
            void UpdateRange(IEnumerable<T> items);
            T UpdatePartial(object item);
            void Delete(params object[] keyvalue);
            void DeleteItem(T item);
            void DeleteRange(IEnumerable<T> items);
        */
            
            // metadata (most should be implemented explicitly on the interface, to conveniently hide from consumer code)
            bool AutoSave { get; set; }               // if false, defers changes; some implementations force AutoSave=true
            void SaveChanges();                       // apply changes; noop if AutoSave == true
            bool SupportsNestedRelationships { get; } // indicates support for "navigation properties" as with EF
            bool SupportsComplexStructures { get; }   // indicates support for multilevel object graphs in one entry as with RavenDB
            bool SupportsGeneratedKeys { get; }       // indicates support for [DatabaseGenerated(Identity)] 
            bool SupportsTransactionScope { get; }    // indicates support for using(var scope = new TransactionScope()) { .. }
            object DataSet { get; }                   // gets the underlying data table, data set, dictionary, or whatever is doing the work
            object DataContext { get; }               // gets the object that contains the connection, if any, to the database
        }

---

* **EntityFramework is passing initial very basic tests.**
* **RavenDB is passing initial very basic tests.**
* **A DataTable implementation is passing initial very basic tests.**
* **A SortedDictionary implementation is passing initial very basic tests.**
* Other ORMs and data repositories are yet to be added.

_____

A few usage notes:

1. IDataStore[T] is a generic interface to a repository as a *table structure*, *document structure*, or similar, not a schema / database.
2. ***The [System.ComponentModel.DataAnnotations.Key] attribute is required on at least one member of T in IDataStore[T].***
3. Any provider must support LINQ. The purpose of Qaud is to add `Add`, `Update`, and `Delete*` commands (plus some helpful touches such as `UpdatePartial()`) to IQueryable in a common and useful interface.
4. Provider implementations should ideally "hide" support members: `DataSetImplementation`, `DataContextImplementation`, `SupportsNestedRelationships`, `SupportsTransactionScope`, and `SupportsComplexStructures`. These are not interesting repository properties/methods for a repository interface consumer, but they are on the interface for identifying implementation behaviors, when you need to know them.
5. Some features require reflection, such as `UpdatePartial()`, which is like `Update()` but takes any object that has the same key field(s) and that has only the properties potentially containing changes. If you do not want to utilize any feature that uses reflection, do not use data provider implementations that impose it, and avoid these special interface member(s).
