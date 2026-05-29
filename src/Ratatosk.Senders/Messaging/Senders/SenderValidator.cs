//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Ratatosk
{
    /// <summary>
    /// Validates <see cref="SenderEntity"/> instances before they are
    /// persisted by the <see cref="SenderManager"/>.
    /// </summary>
    public class SenderValidator : IEntityValidator<SenderEntity>
    {
        /// <inheritdoc />
        public async IAsyncEnumerable<ValidationResult> ValidateAsync(EntityManager<SenderEntity> manager, SenderEntity entity, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                yield return new ValidationResult("The sender name is required.", new[] { nameof(entity.Name) });

            if (string.IsNullOrWhiteSpace(entity.DisplayName))
                yield return new ValidationResult("The display name is required.", new[] { nameof(entity.DisplayName) });

            if (string.IsNullOrWhiteSpace(entity.EndpointType))
                yield return new ValidationResult("The endpoint type is required.", new[] { nameof(entity.EndpointType) });

            if (string.IsNullOrWhiteSpace(entity.Address))
                yield return new ValidationResult("The address is required.", new[] { nameof(entity.Address) });

            await Task.CompletedTask;
        }
    }
}
