using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kaylumah.Ssg.Utilities
{
    public interface IParser
    {
        T Parse<T>(string raw);
    }

    public class YamlParser : IParser
    {
        private readonly IDeserializer _deserializer;
        public YamlParser()
        {
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }

        public T Parse<T>(string raw)
        {
            return _deserializer.Deserialize<T>(raw);
        }
    }
}