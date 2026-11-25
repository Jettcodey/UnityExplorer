using Tomlet.Models;

namespace UnityExplorer.Config
{
    public static class TomlExtensions
    {
        public static string[] ToStringArray(this TomlArray array)
        {
            return array.ArrayValues
                .OfType<TomlString>()
                .Select(x => x.Value)
                .ToArray();
        }
    }
}