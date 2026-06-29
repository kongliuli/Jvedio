using Jvedio.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class PictureScan : ScanJobBase
    {
        public Dictionary<string, List<string>> pathDict;
        public DataType dataType = DataType.Picture;

        private readonly PictureScanPipeline _pipeline;

        public PictureScan(List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null, DataType dataType = DataType.Picture)
            : base(new ScanContext {
                ScanPaths = scanPaths,
                FilePaths = filePaths,
                FileExt = fileExt != null ? NormalizeExtensions(fileExt) : null,
                DataType = dataType,
            })
        {
            pathDict = new Dictionary<string, List<string>>();
            this.dataType = dataType;
            _pipeline = new PictureScanPipeline();
        }

        internal PictureScan(ScanContext context, IPictureScanStore store = null) : base(context)
        {
            pathDict = new Dictionary<string, List<string>>();
            dataType = context.DataType;
            _pipeline = new PictureScanPipeline(store);
        }

        public override void DoWork()
        {
            Task.Run(() => _pipeline.Execute(this, null));
        }
    }
}
