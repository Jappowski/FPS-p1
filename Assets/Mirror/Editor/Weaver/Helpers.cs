using System.IO;
using System.Reflection;

namespace Mirror.Weaver
{
    internal static class Helpers
    {
        // This code is taken from SerializationWeaver

        public static string UnityEngineDllDirectoryName()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            return directoryName?.Replace(@"file:\", "");
        }
    }
}