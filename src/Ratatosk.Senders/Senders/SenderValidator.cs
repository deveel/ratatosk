//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

using Kista;

namespace Ratatosk.Senders
{
    /// <summary>
    /// Validates <see cref="ISender"/> instances before they are
    /// persisted by the <see cref="SenderManager{TSender}"/>.
    /// </summary>
    public class SenderValidator<TSender> : IEntityValidator<TSender>
        where TSender : class, ISender
    {
        /// <inheritdoc />
        public virtual async IAsyncEnumerable<ValidationResult> ValidateAsync(EntityManager<TSender> manager, TSender entity, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                yield return new ValidationResult("The sender name is required.", new[] { nameof(entity.Name) });

            if (string.IsNullOrWhiteSpace(entity.DisplayName))
                yield return new ValidationResult("The display name is required.", new[] { nameof(entity.DisplayName) });

            if (entity.Type == EndpointType.Any)
                yield return new ValidationResult("The endpoint type is required.", new[] { "EndpointType" });

            if (string.IsNullOrWhiteSpace(entity.Address))
                yield return new ValidationResult("The address is required.", new[] { nameof(entity.Address) });

            await Task.CompletedTask;
        }
    }
}
