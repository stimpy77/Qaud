Qaud
====

Interface and implementations for: Query, Add, Update, Delete. It's ICrud, basically. (Qaud is not a word. Don't fix it.)

The interface itself is IDataStore<T>:

https://github.com/stimpy77/Qaud/blob/master/Qaud/IDataStore.cs

---

* **EntityFramework is passing initial basic tests.**
* **RavenDB is passing initial basic tests.**
* A proof-of-concept DataTable implementation is implemented and is passing basic tests.

---

* A cleaner in-memory implementation is yet to be implemented.
* Other ORMs and data repositories are yet to be added.

_____

A few usage notes:

1. IDataStore<<T> is a generic interface to a repository as a *table structure*, *document structure*, or similar, not a schema / database.
2. The [Key] attribute is required on all data models whose type is passed in as T in IDataStore<T>.
3. Any provider must support LINQ. The purpose of Qaud is to add Add, Update, and Delete commands (plus some helpful touches such as UpdatePartial) to IQueryable in a common and useful interface.
4. Provider implementations should ideally "hide" support members: DataSetImplementation, DataContextImplementation, SupportsNestedRelationships, SupportsTransactionScope, and SupportsComplexStructures. These are not interesting repository properties/methods for a repository interface consumer, but they are on the interface for identifying implementation behaviors, when you need to know them.
5. Some features require reflection, such as `UpdatePartial()`, which is like `Update()` but takes any object that has the same key field(s) and that has only the properties potentially containing changes. If you do not want to utilize any feature that uses reflection, do not use data provider implementations that impose it, and avoid these special interface member(s).
