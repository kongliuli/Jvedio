using Jvedio.Core.Enums;
using SuperControls.Style;

namespace Jvedio.Core.UI
{
    public static class DataTypeDisplay
    {
        public static string GetLabel(DataType dataType)
        {
            switch (dataType) {
                case DataType.Picture:
                    return LangManager.GetValueByKey("Picture");
                case DataType.Comics:
                    return LangManager.GetValueByKey("Comics");
                case DataType.Game:
                    return LangManager.GetValueByKey("Game");
                default:
                    return LangManager.GetValueByKey("Video");
            }
        }
    }
}
