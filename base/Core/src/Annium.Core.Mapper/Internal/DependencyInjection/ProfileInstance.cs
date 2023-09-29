namespace Annium.Core.Mapper.Internal.DependencyInjection;

internal class ProfileInstance
{
    public Profile Instance { get; }

    public ProfileInstance(Profile instance)
    {
        Instance = instance;
    }
}