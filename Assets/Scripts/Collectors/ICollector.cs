using System.IO;
using YamlDotNet.Serialization;

namespace RPGTest.Collectors
{
    public abstract class ICollector
    {
        static string ResourcePath { get; set; }

        public static T Collect<T>(string content)
        {
            var deserializer = new Deserializer();

            return deserializer.Deserialize<T>(content);
        }
    }
}
