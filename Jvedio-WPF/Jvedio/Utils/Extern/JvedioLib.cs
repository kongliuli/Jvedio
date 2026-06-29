using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

/* 命名空间不能修改 */
namespace JvedioLib.Security
{
    internal static class JvedioLibLoader
    {
        internal static readonly Assembly Assembly;

        static JvedioLibLoader()
        {
            Assembly = System.Reflection.Assembly.LoadFrom("JvedioLib.dll");
        }

        internal static Type GetType(string typeName)
        {
            return Assembly.GetTypes().FirstOrDefault(arg => arg.Name.Equals(typeName));
        }
    }

    /// <summary>
    /// 该类是加密混淆后的 dll 方法反射加载，避免 aes key 暴露
    /// </summary>
    public static class Encrypt
    {
        private static readonly Type type = JvedioLibLoader.GetType("Encrypt");

        private static object InvokeMethod(object[] _params, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(callerName) || type == null)
                return null;
            MethodInfo methodInfo = type.GetMethod(callerName);
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(null, _params);
        }

        public static string AesEncrypt(string str, string key)
        {
            object result = InvokeMethod(new object[] { str, key });
            return result == null ? string.Empty : result.ToString();
        }

        public static string AesDecrypt(string str, string key)
        {
            object result = InvokeMethod(new object[] { str, key });
            return result == null ? string.Empty : result.ToString();
        }

        public static string AesEncrypt(string str, int key = 0)
        {
            object result = InvokeMethod(new object[] { str, key }, "AesEncryptByIndex");
            return result == null ? string.Empty : result.ToString();
        }

        public static string AesDecrypt(string str, int key = 0)
        {
            object result = InvokeMethod(new object[] { str, key }, "AesDecryptByIndex");
            return result == null ? string.Empty : result.ToString();
        }
    }

    public static class Identify
    {
        private static readonly Type type = JvedioLibLoader.GetType("Identify");
        private static readonly MethodInfo getVidMethod = type?.GetMethod("GetVID", new[] { typeof(string) });
        private static readonly MethodInfo getVideoTypeMethod = type?.GetMethod("GetVideoType", new[] { typeof(string) });
        private static readonly MethodInfo isChsMethod = type?.GetMethod("IsCHS", new[] { typeof(string) });
        private static readonly MethodInfo isHdvStringMethod = type?.GetMethod("IsHDV", new[] { typeof(string) });
        private static readonly MethodInfo isHdvLongMethod = type?.GetMethod("IsHDV", new[] { typeof(long) });

        private static object InvokeMethod(object[] _params, Type[] types = null, [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(callerName) || type == null)
                return null;
            MethodInfo methodInfo = types != null
                ? type.GetMethod(callerName, types)
                : type.GetMethod(callerName);
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(null, _params);
        }

        public static bool IsCHS(string filepath)
        {
            object result = isChsMethod != null
                ? isChsMethod.Invoke(null, new object[] { filepath })
                : InvokeMethod(new object[] { filepath });
            if (result == null)
                return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }

        public static bool IsHDV(string filepath)
        {
            object result = isHdvStringMethod != null
                ? isHdvStringMethod.Invoke(null, new object[] { filepath })
                : InvokeMethod(new object[] { filepath }, new Type[] { typeof(string) });
            if (result == null)
                return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }

        public static bool IsHDV(long filesize)
        {
            object result = isHdvLongMethod != null
                ? isHdvLongMethod.Invoke(null, new object[] { filesize })
                : InvokeMethod(new object[] { filesize }, new Type[] { typeof(long) });
            if (result == null)
                return false;
            bool.TryParse(result.ToString(), out bool v);
            return v;
        }

        public static int GetVideoType(string vID)
        {
            object result = getVideoTypeMethod != null
                ? getVideoTypeMethod.Invoke(null, new object[] { vID })
                : InvokeMethod(new object[] { vID });
            if (result == null)
                return 0;
            int.TryParse(result.ToString(), out int v);
            return v;
        }

        public static string GetEuVID(string str)
        {
            object result = InvokeMethod(new object[] { str });
            return result == null ? string.Empty : result.ToString();
        }

        public static string GetVID(string str)
        {
            object result = getVidMethod != null
                ? getVidMethod.Invoke(null, new object[] { str })
                : InvokeMethod(new object[] { str });
            return result == null ? string.Empty : result.ToString();
        }
    }
}
