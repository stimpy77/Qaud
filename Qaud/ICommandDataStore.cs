using System.Collections.Generic;
using System.Linq;

namespace Qaud
{
    /*
     * 
     * 
     * This is just a placeholder for code design work in progress. Its intention is to facilitate 
     * an interface for CQS or CQRS based repositories.
     * 
     * 
     * 
     */
    public interface ICommandDataStore<T>
    {

        /// <summary>
        /// When implemented, queues the insertion of a <typeparamref name="T"/> object to this data store.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        /// <typeparam name="TReq">A request ID.</typeparam>
        TReq EnqueueAdd<TReq>(T item);

        /// <summary>
        /// When implemented, queues the insertion of a set of <typeparamref name="T"/> objects to this data store.
        /// </summary>
        /// <param name="items">The items to be inserted.</param>
        /// <returns></returns>
        TReq EnqueueAddRange<TReq>(IEnumerable<T> items);

        /// <summary>
        /// Initiates an asynchronous query with the given expression and returns a request ID for followup tracking using <see cref="GetQueueItemResult{TReq,TRes}"/>.
        /// </summary>
        /// <typeparam name="TReq">The request ID for followup tracking.</typeparam>
        /// <param name="expression">The query to execute.</param>
        /// <returns></returns>
        TReq EnqueueQuery<TReq>(IQueryable<T> expression);

        /// <summary>
        /// Attempts to get a result back from a queued request. Returns <value>null</value> if the queued
        /// item has not yet been fully processed.
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="requestId"></param>
        /// <returns></returns>
        TRes GetQueueItemResult<TReq, TRes>(TReq requestId);

        /// <summary>
        /// When implemented, applies all changes to the specified <typeparamref name="T"/> object to this data store.
        /// </summary>
        /// <param name="item">The item to be inserted.</param>
        void EnqueueUpdate(T item);

        /// <summary>
        /// When implemented, applies changes to the specified <typeparamref name="T"/> object to this data store. Only properties included in the object are modified.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        void EnqueueUpdatePartial(object item);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> object from this data store.
        /// <remarks>If a "virtual delete" is to be used instead, expose a Deleted property and use <see cref="Update"/> instead.</remarks>
        /// </summary>
        /// <param name="item">The items to be deleted. Depending on how this is implemented, it is likely that only the identifying properties will be used.</param>
        void EnqueueDelete(T item);

        /// <summary>
        /// When implemented, deletes and destroys the specified <typeparamref name="T"/> objects from this data store.
        /// <remarks>If a "virtual delete" is to be used instead, expose a Deleted property and use <see cref="Update"/> instead.</remarks>
        /// </summary>
        /// <param name="items">The items to be deleted. Depending on how this is implemented, it is likely that only the identifying properties will be used.</param>
        void EnqueueDeleteRange(IEnumerable<T> items);
    }
}
