namespace Jvedio.Core.UI
{
    public enum MediaListColumn
    {
        Operate = 0,
        Vid = 1,
        Title = 2,
        Grade = 3,
        Path = 4,
        Size = 5,
        Duration = 6,
        ReleaseDate = 7,
        ViewCount = 8,
        ViewDate = 9,
        LastScanDate = 10,
        Director = 11,
        Studio = 12,
        CreateDate = 13,
        UpdateDate = 14,
        Genre = 15,
        Series = 16,
    }

    public static class MediaListColumnPolicy
    {
        public const int ColumnCount = 17;

        public static bool IsVisible(MediaListMode mode, MediaListColumn column)
        {
            switch (mode) {
                case MediaListMode.Video:
                    return column != MediaListColumn.Genre && column != MediaListColumn.Series;
                case MediaListMode.Picture:
                    return column == MediaListColumn.Operate
                        || column == MediaListColumn.Path
                        || column == MediaListColumn.Size
                        || column == MediaListColumn.LastScanDate
                        || column == MediaListColumn.CreateDate;
                default:
                    return true;
            }
        }
    }
}
