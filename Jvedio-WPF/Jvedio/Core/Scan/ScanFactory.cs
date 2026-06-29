using Jvedio.Core.Enums;
using Jvedio.Core.Scan.Discovery;
using SuperUtils.Framework.Tasks;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.Scan
{
    public static class ScanFactory
    {
        private static readonly Dictionary<DataType, Func<ScanContext, ScanJobBase>> Registry =
            new Dictionary<DataType, Func<ScanContext, ScanJobBase>>();

        static ScanFactory()
        {
            Register(DataType.Video, ctx => new ScanTask(ctx));
            Register(DataType.Game, ctx => new GameScan(ctx));
            Register(DataType.Picture, ctx => {
                if (ctx.FileExt == null)
                    ctx.FileExt = ScanExtensions.PICTURE_EXTENSIONS_LIST;
                return new PictureScan(ctx);
            });
            Register(DataType.Comics, ctx => {
                if (ctx.FileExt == null)
                    ctx.FileExt = ScanExtensions.PICTURE_EXTENSIONS_LIST;
                ctx.DataType = DataType.Comics;
                return new ComicScan(ctx);
            });
        }

        public static void Register(DataType type, Func<ScanContext, ScanJobBase> factory)
        {
            Registry[type] = factory;
        }

        public static ScanJobBase ProduceScanner(DataType dataType, List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null, DiscoveryMode discoveryMode = DiscoveryMode.Full, ScanMode scanMode = ScanMode.FullImport)
        {
            ScanContext context = new ScanContext {
                ScanPaths = scanPaths,
                FilePaths = filePaths,
                FileExt = fileExt != null ? ScanJobBase.NormalizeExtensions(fileExt) : null,
                DataType = dataType,
                DiscoveryMode = discoveryMode,
                Mode = scanMode,
            };

            if (Registry.TryGetValue(dataType, out Func<ScanContext, ScanJobBase> factory))
                return factory(context);
            return new ScanTask(context);
        }

        public static AbstractTask ProduceTask(DataType dataType, List<string> scanPaths, List<string> filePaths, IEnumerable<string> fileExt = null)
        {
            return ProduceScanner(dataType, scanPaths, filePaths, fileExt);
        }
    }
}
