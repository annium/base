using Annium.Net.Types;
using Annium.Net.Types.Internal;
using Annium.Net.Types.Internal.Config;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Internal.Referrers;

// ReSharper disable CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddModelMapper(this IServiceContainer container)
    {
        container.Add<MapperConfig>().AsSelf().Singleton();
        container.Add<IMapperConfigInternal>(sp =>
        {
            var config = sp.Resolve<MapperConfig>();
            config.SetBaseTypes();
            config.Ignore();
            config.RegisterArrays();
            config.RegisterRecords();

            return config;
        }).AsSelf().Singleton();
        container.Add<IMapperConfig>(sp => sp.Resolve<IMapperConfigInternal>()).AsSelf().Singleton();
        container.Add<IModelMapper, ModelMapper>().Transient();
        container.Add<IMapperProcessingContext, ProcessingContext>().Transient();

        // add processors
        container.Add<Processor>().AsSelf().Singleton();
        container.Add<IProcessor, GenericTypeProcessor>().Singleton();
        container.Add<IProcessor, IgnoredProcessor>().Singleton();
        container.Add<IProcessor, ExcludedProcessor>().Singleton();
        container.Add<IProcessor, NullableProcessor>().Singleton();
        container.Add<IProcessor, GenericParameterProcessor>().Singleton();
        container.Add<IProcessor, BaseTypeProcessor>().Singleton();
        container.Add<IProcessor, EnumProcessor>().Singleton();
        container.Add<IProcessor, RecordProcessor>().Singleton();
        container.Add<IProcessor, ArrayProcessor>().Singleton();
        container.Add<IProcessor, InterfaceProcessor>().Singleton();
        container.Add<IProcessor, StructProcessor>().Singleton();

        // add referrers
        container.Add<Referrer>().AsSelf().Singleton();
        container.Add<IReferrer, NullableReferrer>().Singleton();
        container.Add<IReferrer, ExcludedReferrer>().Singleton();
        container.Add<IReferrer, GenericParameterReferrer>().Singleton();
        container.Add<IReferrer, BaseTypeReferrer>().Singleton();
        container.Add<IReferrer, EnumReferrer>().Singleton();
        container.Add<IReferrer, SpecialReferrer>().Singleton();
        container.Add<IReferrer, RecordReferrer>().Singleton();
        container.Add<IReferrer, ArrayReferrer>().Singleton();
        container.Add<IReferrer, InterfaceReferrer>().Singleton();
        container.Add<IReferrer, StructReferrer>().Singleton();

        return container;
    }
}