using System.ComponentModel.DataAnnotations;

namespace Ratatosk.Senders;

/// <summary>
/// Defines the contract for validating sender entities before persistence.
/// </summary>
/// <typeparam name="TSender">
/// The type of sender entity, which must implement <see cref="ISender"/>.
/// </typeparam>
public interface ISenderValidator<TSender> : IEntityValidator<TSender>
     where TSender : class, ISender
{
}