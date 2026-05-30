//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using Microsoft.Extensions.Logging;

namespace Ratatosk
{
    /// <summary>
    /// An in-memory implementation of <see cref="IRepository{T}"/>
    /// for <see cref="Sender"/>, useful for testing and development.
    /// </summary>
    public class InMemorySenderStore : InMemoryRepository<Sender>
    {
        /// <summary>
        /// Constructs the store with an optional initial set of senders.
        /// </summary>
        /// <param name="senders">
        /// An optional initial set of senders to seed the store.
        /// </param>
        public InMemorySenderStore(IEnumerable<Sender>? senders = null)
            : base(senders ?? Array.Empty<Sender>(), new ReflectionFieldMapper<Sender>())
        {
        }

        /// <summary>
        /// Constructs the store with an optional initial set of senders
        /// and a custom field mapper.
        /// </summary>
        /// <param name="fieldMapper">The field mapper for querying.</param>
        /// <param name="senders">
        /// An optional initial set of senders to seed the store.
        /// </param>
        public InMemorySenderStore(IFieldMapper<Sender> fieldMapper, IEnumerable<Sender>? senders = null)
            : base(senders ?? Array.Empty<Sender>(), fieldMapper)
        {
        }
    }
}
