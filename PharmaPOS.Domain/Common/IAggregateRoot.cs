namespace PharmaPOS.Domain.Common.Entities;

/// <summary>
/// Marker interface identifying an entity as an aggregate root.
/// Only aggregate roots should have corresponding repositories.
/// </summary>
public interface IAggregateRoot;
