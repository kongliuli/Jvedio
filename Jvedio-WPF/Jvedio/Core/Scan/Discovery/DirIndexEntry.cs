namespace Jvedio.Core.Scan.Discovery
{
    public sealed class DirIndexEntry
    {
        public int DbId { get; set; }
        public string DirPath { get; set; }
        public long MtimeUtcTicks { get; set; }
        public int EntryCount { get; set; }
        public string ChildrenChecksum { get; set; }
        public string LastScanUtc { get; set; }
    }
}
