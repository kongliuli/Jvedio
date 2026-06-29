using Jvedio.Core.Scan;
using Jvedio.Entity;
using SuperUtils.Framework.ORM.Wrapper;
using System;

namespace Jvedio.Core.UI
{
    public static class PictureListQuery
    {
        public const string AlbumFromSql = " FROM metadata_picture JOIN metadata on metadata.DataID=metadata_picture.DataID ";

        public const string SingleImageFromSql =
            " FROM metadata_picture_file pf " +
            "JOIN metadata ON metadata.DataID=pf.DataID " +
            "JOIN metadata_picture ON metadata_picture.DataID=pf.DataID " +
            "JOIN picture_folder_node pfn ON pfn.FullPath=metadata.Path AND pfn.DBId=metadata.DBId ";

        public static readonly string[] AlbumSelectFields =
        {
            "DISTINCT metadata.DataID",
            "metadata.Grade",
            "metadata.Title",
            "metadata.Path",
            "metadata.Size",
            "metadata.LastScanDate",
            "metadata.CreateDate",
            "metadata_picture.PicCount",
            "metadata_picture.PicPaths",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID) as TagIDs ",
        };

        public static readonly string[] SingleImageSelectFields =
        {
            "DISTINCT metadata.DataID",
            "pf.FID",
            "metadata.Grade",
            "pf.FileName",
            "metadata.Path",
            "pf.RelativePath",
            "pfn.ScanRoot",
            "pf.Size as Size",
            "metadata.LastScanDate",
            "metadata.CreateDate",
            "(select group_concat(TagID,',') from metadata_to_tagstamp where metadata_to_tagstamp.DataID=metadata.DataID) as TagIDs ",
        };

        public static string ResolveFromSql(PictureBrowseMode mode)
        {
            return mode == PictureBrowseMode.SingleImage ? SingleImageFromSql : AlbumFromSql;
        }

        public static string[] ResolveSelectFields(PictureBrowseMode mode)
        {
            return mode == PictureBrowseMode.SingleImage ? SingleImageSelectFields : AlbumSelectFields;
        }

        public static void ApplyFolderScope(
            SelectWrapper<Video> wrapper,
            ref string joinSql,
            string folderPath,
            PictureBrowseMode mode,
            bool includeSubfolders)
        {
            if (wrapper == null || string.IsNullOrWhiteSpace(folderPath))
                return;

            folderPath = PicturePathHelper.NormalizeDir(folderPath);
            if (mode == PictureBrowseMode.Album) {
                joinSql += " JOIN picture_folder_node pfn_scope ON pfn_scope.FullPath=metadata.Path AND pfn_scope.DBId=metadata.DBId ";
                wrapper.Eq("pfn_scope.ParentPath", folderPath);
                return;
            }

            if (includeSubfolders) {
                wrapper.LeftBracket()
                    .Eq("metadata.Path", folderPath)
                    .Or()
                    .Like("metadata.Path", folderPath + "\\%")
                    .Or()
                    .Like("metadata.Path", folderPath + "/%")
                    .RightBracket();
            } else {
                wrapper.Eq("metadata.Path", folderPath);
            }
        }
    }
}
