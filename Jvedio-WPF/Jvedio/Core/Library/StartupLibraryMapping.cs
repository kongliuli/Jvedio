using Jvedio.Core.Enums;

namespace Jvedio.Core.Library
{
    /// <summary>启动页侧栏索引与 <see cref="DataType"/> 显式映射，避免 SideIdx 与 enum 整型值隐式耦合。</summary>
    public static class StartupLibraryMapping
    {
        private static readonly DataType[] SideIndexToDataType = {
            DataType.Video,
            DataType.Picture,
            DataType.Game,
            DataType.Comics,
        };

        public static DataType DataTypeFromSideIndex(int sideIndex)
        {
            if (sideIndex >= 0 && sideIndex < SideIndexToDataType.Length)
                return SideIndexToDataType[sideIndex];
            return DataType.Video;
        }

        public static int SideIndexFromDataType(DataType dataType)
        {
            for (int i = 0; i < SideIndexToDataType.Length; i++) {
                if (SideIndexToDataType[i] == dataType)
                    return i;
            }
            return 0;
        }

        public static DataType ResolveDataType(long sideIdx, int storedDataType = -1)
        {
            if (storedDataType >= 0 && System.Enum.IsDefined(typeof(DataType), storedDataType))
                return (DataType)storedDataType;
            return DataTypeFromSideIndex((int)sideIdx);
        }

        public static int ToStorageValue(DataType dataType) => (int)dataType;
    }
}
