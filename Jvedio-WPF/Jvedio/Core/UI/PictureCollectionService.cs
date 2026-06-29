using Jvedio.Core.Library;
using Jvedio.Entity.Data;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UI
{
    public static class PictureCollectionService
    {
        public static List<PictureCollectionSummary> ListCollections(long dbId)
        {
            var wrapper = new SelectWrapper<PictureCollection>();
            wrapper.Eq("DBId", dbId).Asc("SortOrder").Asc("Name");
            List<PictureCollection> list = pictureCollectionMapper.SelectList(wrapper);
            if (list == null || list.Count == 0)
                return new List<PictureCollectionSummary>();

            var result = new List<PictureCollectionSummary>();
            foreach (PictureCollection col in list) {
                long count = pictureCollectionItemMapper.SelectCount(
                    "SELECT COUNT(*) FROM picture_collection_item WHERE CollectionID=" + col.CollectionID);
                result.Add(new PictureCollectionSummary {
                    CollectionID = col.CollectionID,
                    Name = col.Name,
                    ItemCount = (int)count,
                });
            }
            return result;
        }

        public static PictureCollection Create(long dbId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            var entity = new PictureCollection {
                DBId = dbId,
                Name = name.Trim(),
                SortOrder = 0,
                CreateDate = DateHelper.Now(),
            };
            long id = System.Convert.ToInt64(pictureCollectionMapper.InsertAndGetID(entity));
            entity.CollectionID = id;
            LibraryEventBus.RaisePictureCollectionChanged();
            return entity;
        }

        public static bool Rename(long collectionId, string name)
        {
            if (collectionId <= 0 || string.IsNullOrWhiteSpace(name))
                return false;
            var wrapper = new SelectWrapper<PictureCollection>();
            wrapper.Eq("CollectionID", collectionId);
            pictureCollectionMapper.UpdateField("Name", name.Trim(), wrapper);
            LibraryEventBus.RaisePictureCollectionChanged();
            return true;
        }

        public static bool Delete(long collectionId)
        {
            if (collectionId <= 0)
                return false;
            pictureCollectionItemMapper.ExecuteNonQuery(
                $"DELETE FROM picture_collection_item WHERE CollectionID={collectionId}");
            pictureCollectionMapper.ExecuteNonQuery(
                $"DELETE FROM picture_collection WHERE CollectionID={collectionId}");
            if (PictureBrowseContext.SelectedCollectionId == collectionId)
                PictureBrowseContext.ClearCollection();
            LibraryEventBus.RaisePictureCollectionChanged();
            return true;
        }

        public static bool AddItem(long collectionId, string itemType, string refPath = null, long refDataId = 0, long refFid = 0)
        {
            if (collectionId <= 0 || string.IsNullOrWhiteSpace(itemType))
                return false;

            itemType = itemType.ToLowerInvariant();
            if (itemType == PictureCollectionItemTypes.Folder)
                refPath = string.IsNullOrWhiteSpace(refPath) ? null : Scan.PicturePathHelper.NormalizeDir(refPath);
            else if (itemType == PictureCollectionItemTypes.Album && refDataId <= 0)
                return false;
            else if (itemType == PictureCollectionItemTypes.File && refFid <= 0)
                return false;

            var item = new PictureCollectionItem {
                CollectionID = collectionId,
                ItemType = itemType,
                RefPath = refPath ?? string.Empty,
                RefDataID = refDataId,
                RefFID = refFid,
            };
            try {
                pictureCollectionItemMapper.Insert(item);
            } catch {
                return false;
            }
            LibraryEventBus.RaisePictureCollectionChanged();
            return true;
        }

        public static bool RemoveItem(long collectionId, string itemType, string refPath = null, long refDataId = 0, long refFid = 0)
        {
            if (collectionId <= 0)
                return false;
            itemType = itemType?.ToLowerInvariant() ?? string.Empty;
            string sql = $"DELETE FROM picture_collection_item WHERE CollectionID={collectionId} AND ItemType='{Escape(itemType)}'";
            if (itemType == PictureCollectionItemTypes.Folder)
                sql += $" AND RefPath='{Escape(refPath ?? string.Empty)}'";
            else if (itemType == PictureCollectionItemTypes.Album)
                sql += $" AND RefDataID={refDataId}";
            else if (itemType == PictureCollectionItemTypes.File)
                sql += $" AND RefFID={refFid}";
            pictureCollectionItemMapper.ExecuteNonQuery(sql);
            LibraryEventBus.RaisePictureCollectionChanged();
            return true;
        }

        public static List<long> GetCollectionIdsContaining(string itemType, string refPath = null, long refDataId = 0, long refFid = 0)
        {
            itemType = itemType?.ToLowerInvariant() ?? string.Empty;
            string sql = "SELECT DISTINCT CollectionID FROM picture_collection_item WHERE ItemType='" + Escape(itemType) + "'";
            if (itemType == PictureCollectionItemTypes.Folder)
                sql += " AND RefPath='" + Escape(refPath ?? string.Empty) + "'";
            else if (itemType == PictureCollectionItemTypes.Album)
                sql += " AND RefDataID=" + refDataId;
            else if (itemType == PictureCollectionItemTypes.File)
                sql += " AND RefFID=" + refFid;

            List<Dictionary<string, object>> rows = pictureCollectionItemMapper.Select(sql);
            if (rows == null)
                return new List<long>();
            return rows
                .Where(r => r.ContainsKey("CollectionID"))
                .Select(r => Convert.ToInt64(r["CollectionID"]))
                .ToList();
        }

        public static string BuildCollectionScopeSql(long collectionId, PictureBrowseMode mode)
        {
            if (collectionId <= 0)
                return string.Empty;

            string id = collectionId.ToString();
            if (mode == PictureBrowseMode.Album) {
                return " AND (" +
                    $"metadata.DataID IN (SELECT RefDataID FROM picture_collection_item WHERE CollectionID={id} AND ItemType='album' AND RefDataID>0)" +
                    " OR EXISTS (SELECT 1 FROM picture_collection_item pci WHERE pci.CollectionID=" + id +
                    " AND pci.ItemType='folder' AND pci.RefPath<>'' AND (" +
                    "metadata.Path=pci.RefPath OR metadata.Path LIKE pci.RefPath || '\\%' OR metadata.Path LIKE pci.RefPath || '/%'))" +
                    ") ";
            }

            return " AND (" +
                $"pf.FID IN (SELECT RefFID FROM picture_collection_item WHERE CollectionID={id} AND ItemType='file' AND RefFID>0)" +
                $" OR metadata.DataID IN (SELECT RefDataID FROM picture_collection_item WHERE CollectionID={id} AND ItemType='album' AND RefDataID>0)" +
                " OR EXISTS (SELECT 1 FROM picture_collection_item pci WHERE pci.CollectionID=" + id +
                " AND pci.ItemType='folder' AND pci.RefPath<>'' AND (" +
                "metadata.Path=pci.RefPath OR metadata.Path LIKE pci.RefPath || '\\%' OR metadata.Path LIKE pci.RefPath || '/%'))" +
                ") ";
        }

        private static string Escape(string value) => (value ?? string.Empty).Replace("'", "''");
    }
}
