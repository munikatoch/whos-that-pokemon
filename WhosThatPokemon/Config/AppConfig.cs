using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using WhosThatPokemon.Interfaces.Config;

namespace WhosThatPokemon.Config
{
    public class AppConfig : IAppConfig
    {
        private IConfigurationRoot? _configuration;

        private IConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = BuildConfiguration();
                }
                return _configuration;
            }
        }

        private IConfigurationRoot BuildConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            IConfigurationRoot configuration = configurationBuilder.Build();
            return configuration;
        }

        public T GetValue<T>(string key, T defaultValue) where T : notnull
        {
            string? value = Configuration[key];
            if (value == null)
            {
                return defaultValue;
            }
            TryConvertValue(typeof(T), value, out object? result, out Exception? error);
            if (error == null && result != null)
            {
                return (T)result;
            }
            return defaultValue;
        }

        private bool TryConvertValue(Type type, string value, out object? result, out Exception? error)
        {
            error = null;
            result = null;
            if (type == typeof(object))
            {
                result = value;
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }
                return TryConvertValue(Nullable.GetUnderlyingType(type)!, value, out result, out error);
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFromInvariantString(value);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                return true;
            }

            if (type == typeof(byte[]))
            {
                try
                {
                    result = Convert.FromBase64String(value);
                }
                catch (FormatException ex)
                {
                    error = ex;
                }
                return true;
            }

            return false;
        }
    }
}
