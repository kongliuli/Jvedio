using Jvedio.Core.Config.Base;

namespace Jvedio.Core.WindowConfig
{
    public class StartUp : AbstractConfig
    {
        private StartUp() : base($"WindowConfig.StartUp")
        {
            Tile = false;
            ShowHideItem = false;
            SideIdx = 0;
            SortType = string.Empty;
            Sort = true;
        }

        private static StartUp _instance = null;

        public static StartUp CreateInstance()
        {
            if (_instance == null)
                _instance = new StartUp();

            return _instance;
        }

        public bool Tile { get; set; }

        public bool ShowHideItem { get; set; }

        public long SideIdx { get; set; }

        /// <summary>-1 表示沿用 SideIdx 映射。</summary>
        public int LibraryDataType { get; set; } = -1;

        public string SortType { get; set; }

        public bool Sort { get; set; }
    }
}
