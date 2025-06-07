namespace Annium.Data.Models;

/// <summary>
/// Added when extending EF, but generally might be appropriate for any materialization case
/// </summary>
public interface IMaterializable
{
    /// <summary>
    /// Called when the entity has been materialized from a data source
    /// </summary>
    void OnMaterialized();
}
