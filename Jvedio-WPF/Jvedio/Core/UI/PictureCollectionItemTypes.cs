namespace Jvedio.Core.UI
{
    public static class PictureCollectionItemTypes
    {
        public const string Folder = "folder";
        public const string File = "file";
        public const string Album = "album";
    }

    public sealed class PictureCollectionSummary
    {
        public long CollectionID { get; set; }
        public string Name { get; set; }
        public int ItemCount { get; set; }
    }
}
