namespace Annium.Data.Models;

/// <summary>
/// Added when extending EF, but generally might be appropriate for any materialization case
/// </summary>
public interface IMaterializable
{
    void OnMaterialized();
}