﻿using System;
using System.IO;
using System.Threading.Tasks;
using ADPasswordSecureCache.Policies;

namespace ADPasswordSecureCache
{
    /// <summary>
    /// Defines methods, properties and events used to manage a disk-based cache.
    /// </summary>
    /// <typeparam name="TKey">The type of keys used in the cache.</typeparam>
    public interface IDiskCache<TKey> : IDisposable
    {
        /// <summary>
        /// The maximum size that the cache can contain.
        /// </summary>
        ulong MaximumStorageCapacity { get; }

        /// <summary>
        /// The cache eviction policy that evaluates which entries should be removed from the cache.
        /// </summary>
        ICachePolicy<TKey> Policy { get; }

        /// <summary>
        /// Occurs when an entry has been added to the cache.
        /// </summary>
        event EventHandler<ICacheEntry<TKey>> EntryAdded;

        /// <summary>
        /// Occurs when an entry has been updated in the cache.
        /// </summary>
        event EventHandler<ICacheEntry<TKey>> EntryUpdated;

        /// <summary>
        /// Occurs when an entry has been removed or evicted from the cache.
        /// </summary>
        event EventHandler<ICacheEntry<TKey>> EntryRemoved;

        /// <summary>
        /// Empties the cache of all values that it is currently tracking.
        /// </summary>
        void Clear();

        /// <summary>
        /// Determines whether the <see cref="IDiskCache{T}"/> contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns><c>true</c> if the cache contains the key; otherwise <c>false</c>.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Gets the value associated with a key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>A stream of data from the cache.</returns>
        Stream GetValue(TKey key);

        /// <summary>
        /// Gets the value associated with a key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <param name="stream">A stream of data from the cache. Will be <c>null</c> when <paramref name="key"/> does not exist in the cache.</param>
        /// <returns><c>true</c> if the cache contains the key; otherwise <c>false</c>.</returns>
        bool TryGetValue(TKey key, out Stream stream);

        /// <summary>
        /// Gets the value associated with a key.
        /// </summary>
        /// <param name="key">The key to locate in the cache.</param>
        /// <returns>A tuple of two values. A boolean determines whether <paramref name="key"/> is present in the cache. If <paramref name="key"/> is present, the <see cref="Stream"/> value will be provided, otherwise it will be <c>null</c>.</returns>
        (bool hasValue, Stream stream) TryGetValue(TKey key);

        /// <summary>
        /// Stores a value associated with a key.
        /// </summary>
        /// <param name="key">The key used to locate the value in the cache.</param>
        /// <param name="value">A stream of data to store in the cache.</param>
        void SetValue(TKey key, Stream value);

        /// <summary>
        /// Stores a value associated with a key.
        /// </summary>
        /// <param name="key">The key used to locate the value in the cache.</param>
        /// <param name="value">A stream of data to store in the cache.</param>
        /// <returns><c>true</c> if the data was able to be stored without error; otherwise <c>false</c>.</returns>
        bool TrySetValue(TKey key, Stream value);
    }
}