using Jvedio.Core.Enums;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public class ComicScan : PictureScan
    {
        public ComicScan(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
            : base(scanPaths, filePaths, fileExt, DataType.Comics)
        {
        }

        internal ComicScan(ScanContext context) : base(context)
        {
            dataType = DataType.Comics;
        }
    }
}
