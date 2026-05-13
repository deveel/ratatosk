using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Deveel.Messaging
{
    internal static class ConnectorSchemaHelper
    {
        internal static IChannelSchema DiscoverConnectorSchema(
            IServiceProvider services, Type connectorType)
        {
            var attribute = connectorType.GetCustomAttribute<ChannelSchemaAttribute>();
            if (attribute == null)
                throw new ArgumentException(
                    $"Connector type '{connectorType.Name}' must be decorated with " +
                    $"{nameof(ChannelSchemaAttribute)}.",
                    nameof(connectorType));

            try
            {
                return CreateSchema(services, attribute.SchemaType);
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new InvalidOperationException(
                    $"Failed to create schema for connector type '{connectorType.Name}': " +
                    $"{ex.Message}", ex);
            }
        }

        internal static IChannelSchema CreateSchema(IServiceProvider services, Type schemaType)
        {
            if (typeof(IChannelSchemaFactory).IsAssignableFrom(schemaType))
            {
                var factory = ActivatorUtilities.CreateInstance(services, schemaType)
                    as IChannelSchemaFactory
                    ?? throw new InvalidOperationException(
                        $"Failed to create instance of schema factory '{schemaType.Name}'.");
                return factory.CreateSchema();
            }

            if (typeof(IChannelSchema).IsAssignableFrom(schemaType))
            {
                var schemaInstance = ActivatorUtilities.CreateInstance(services, schemaType)
                    as IChannelSchema
                    ?? throw new InvalidOperationException(
                        $"Failed to create instance of schema '{schemaType.Name}'.");
                return schemaInstance;
            }

            throw new InvalidOperationException(
                $"Type '{schemaType.Name}' is not a valid schema factory or schema type.");
        }
    }
}
