using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Qaud
{
    /// <summary>
    /// Describes a data store that supports CRUD operations -- or rather, QAUD operations (Query, Add, Update, Delete).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataStore<T> : ICrudEx<T>
    {

        /// <summary>
        /// When implemented, gets or sets whether Add, Update, or Delete is immediately applied or if the change is 
        /// deferred.
        /// </summary>
        bool AutoSave { get; set; }

        /// <summary>
        /// When implemented, applies changes made using <see cref="Add"/>, <see cref="Update"/>, and/or 
        /// <see cref="DeleteItem"/>.
        ///  This method should have no effect if <see cref="AutoSave"/> is set to <value>true</value>, in which case 
        /// the changes would have already been applied.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// When implemented, indicates whether setting <see cref="AutoSave"/> to <value>false</value> has any effect.
        /// </summary>
        bool CanQueueChanges { get; }

        /// <summary>
        /// Gets whether the implementation supports nested relationships and Add or Update might 
        /// propagate related data, such as navigation properties in the case of Entity Framework.
        /// When false, data associated with this data store would only act as records or documents
        /// in a singular table or document store.
        /// </summary>
        bool SupportsNestedRelationships { get; }

        /// <summary>
        /// When implemented, gets whether a single property can be deserialized as a complete complex type 
        /// automatically, whether via <see cref="SupportsNestedRelationships"/> (navigation properties) or via 
        /// tree-based document storage. Returns false if the document store only supports flat table structures, with 
        /// no relationships.
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
        /// When implemented, returns the underlying data structure responsible for managing this data store.
        /// </summary>
        object DataSet { get; }

        /// <summary>
        /// When implemented, and if provided, returns the data context associated with the underlying data structure 
        /// responsible for managing this data store.
        /// </summary>
        object DataContext { get; }
    }

    
}
