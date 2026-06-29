using Jvedio.Core.Enums;

namespace Jvedio.Core.Library
{
    /// <summary>兼容层；新代码/XAML 请用 <see cref="LibraryContextBinding"/>。</summary>
    [System.Obsolete("Use LibraryContextBinding or LibraryContext.Current")]
    public static class LibraryRuntime
    {
        public static DataType CurrentDataType { get; set; } = DataType.Video;

        public static void SetCurrent(DataType dataType)
        {
            CurrentDataType = dataType;
        }
    }
}
