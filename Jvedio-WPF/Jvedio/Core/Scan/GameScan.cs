using Jvedio.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jvedio.Core.Scan
{
    public class GameScan : ScanJobBase
    {
        public Dictionary<string, List<string>> pathDict;

        private readonly GameScanPipeline _pipeline;

        public GameScan(List<string> scanPaths, IEnumerable<string> fileExt = null)
            : base(new ScanContext { ScanPaths = scanPaths, FileExt = fileExt != null ? NormalizeExtensions(fileExt) : null, DataType = DataType.Game })
        {
            pathDict = new Dictionary<string, List<string>>();
            _pipeline = new GameScanPipeline();
        }

        internal GameScan(ScanContext context, IGameScanStore store = null) : base(context)
        {
            pathDict = new Dictionary<string, List<string>>();
            _pipeline = new GameScanPipeline(store);
        }

        public override void DoWork()
        {
            Task.Run(() => _pipeline.Execute(this, null));
        }
    }
}
