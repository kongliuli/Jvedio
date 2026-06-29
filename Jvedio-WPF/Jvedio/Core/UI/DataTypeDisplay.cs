using Jvedio.Core.Enums;
using SuperControls.Style;

namespace Jvedio.Core.UI
{
    public static class DataTypeDisplay
    {
        public static string GetLabel(DataType dataType)
        {
            if (dataType == DataType.Picture)
                return LangManager.GetValueByKey("Picture");
            return LangManager.GetValueByKey("Video");
        }
    }
}
