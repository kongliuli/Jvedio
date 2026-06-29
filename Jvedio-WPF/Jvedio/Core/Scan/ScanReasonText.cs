using Jvedio.Core.Enums;
using SuperControls.Style;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public static class ScanReasonText
    {
        private static readonly Dictionary<NotImportReason, string> Map = new Dictionary<NotImportReason, string> {
            { NotImportReason.NotInExtension, LangManager.GetValueByKey("NotSupportedExt") },
            { NotImportReason.RepetitiveVideo, LangManager.GetValueByKey("RepeatedVideo") },
            { NotImportReason.RepetitiveVID, LangManager.GetValueByKey("RepeatedVID") },
            { NotImportReason.SizeTooSmall, LangManager.GetValueByKey("FileSizeTooSmall") },
            { NotImportReason.SizeTooLarge, LangManager.GetValueByKey("FileSizeTooBig") },
        };

        public static string Get(NotImportReason reason)
        {
            if (Map.TryGetValue(reason, out string text) && !string.IsNullOrEmpty(text))
                return text;
            return reason.ToString();
        }
    }
}
