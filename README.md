Qaud
====

Interface and implementations for: Query, Add, Update, Delete. It's ICrud, basically. (Qaud is not a word. Don't fix it.)

The interface itself is IDataStore:

https://github.com/stimpy77/Qaud/blob/master/Qaud/IDataStore.cs

---

* The implementation is not optimized, and the tests are not detailed.
* **EntityFramework is passing initial basic tests.**
* **RavenDB is passing initial basic tests.**
* A proof-of-concept DataTable implementation is implemented and is passing basic tests.
* A cleaner in-memory implementation is yet to be implemented.
* Other ORMs and data repositories are yet to be added.
