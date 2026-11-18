using System.Runtime.InteropServices;

namespace UnityExplorer.Helpers
{
    public class CrossOSHelpers
    {
        [DllImport("kernel32.dll", EntryPoint = "wine_get_unix_file_name", CharSet = CharSet.Unicode)]
        static extern IntPtr wine_get_unix_file_name(string dosPath);

        public static string ToUnixPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (path[0] == '/') return path;

            try
            {
                IntPtr ptr = wine_get_unix_file_name(path);
                if (ptr != IntPtr.Zero) return Marshal.PtrToStringAnsi(ptr);
            }
            catch { }

            return path.Length > 2 && char.ToUpperInvariant(path[0]) == 'Z' && path[1] == ':'
                ? path.Substring(2).Replace('\\', '/')
                : null;
        }
        
        public static bool isWin32(string path) => path.Length > 0 && path[1] == ':';
        public static bool isUnix(string path) => path.Length > 0 && path.StartsWith("/");
        public static bool isCompatibilityLayer()
        {
#if ML
            return MelonLoader.MelonUtils.IsUnderWineOrSteamProton();
#else
            return false;
#endif
        }
    }
}