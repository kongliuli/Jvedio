using Jvedio.Core.Enums;
using Jvedio.Core.UI;
using Jvedio.Entity;
using SuperControls.Style;
using SuperControls.Style.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.App;

namespace Jvedio.Core.UserControls
{
    public partial class VideoList
    {
        private MenuItem _pictureCollectionRootMenu;
        private MenuItem _pictureRemoveFromCollectionMenu;

        private void EnsurePictureCollectionMenus(ContextMenu contextMenu)
        {
            if (_pictureCollectionRootMenu != null)
                return;

            _pictureCollectionRootMenu = new MenuItem { Header = LangManager.GetValueByKey("AddToCollection") };
            _pictureRemoveFromCollectionMenu = new MenuItem {
                Header = LangManager.GetValueByKey("RemoveFromCollection"),
                Visibility = Visibility.Collapsed,
            };
            _pictureRemoveFromCollectionMenu.Click += RemoveFromCollection_Click;

            int insertAt = 0;
            contextMenu.Items.Insert(insertAt++, _pictureCollectionRootMenu);
            contextMenu.Items.Insert(insertAt, _pictureRemoveFromCollectionMenu);
        }

        private void RefreshPictureCollectionMenus(ContextMenu contextMenu, Video video)
        {
            if (ListMode != MediaListMode.Picture || video == null || contextMenu == null)
                return;

            EnsurePictureCollectionMenus(contextMenu);
            _pictureCollectionRootMenu.Items.Clear();

            var createItem = new MenuItem { Header = LangManager.GetValueByKey("NewPictureCollection") };
            createItem.Click += (s, e) => CreateCollectionAndAdd(video);
            _pictureCollectionRootMenu.Items.Add(createItem);
            _pictureCollectionRootMenu.Items.Add(new Separator());

            foreach (PictureCollectionSummary summary in PictureCollectionService.ListCollections(ConfigManager.Main.CurrentDBId)) {
                var menu = new MenuItem { Header = summary.Name };
                long collectionId = summary.CollectionID;
                menu.Click += (s, e) => AddVideoToCollection(video, collectionId);
                _pictureCollectionRootMenu.Items.Add(menu);
            }

            string itemType = ResolvePictureItemType(video);
            long refDataId = video.DataID;
            long refFid = video.FID;
            var containing = PictureCollectionService.GetCollectionIdsContaining(itemType, refPath: null, refDataId, refFid);
            _pictureRemoveFromCollectionMenu.Visibility = containing.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            _pictureRemoveFromCollectionMenu.Tag = new PictureCollectionMenuTag(itemType, refDataId, refFid, containing);
        }

        private void AddVideoToCollection(Video video, long collectionId)
        {
            if (collectionId <= 0)
                return;
            foreach (Video selected in GetSelectedPictureVideos(video))
                AddVideoToCollectionSingle(selected, collectionId);
        }

        private void AddVideoToCollectionSingle(Video video, long collectionId)
        {
            if (video == null || video.DataID <= 0)
                return;
            string itemType = ResolvePictureItemType(video);
            if (itemType == PictureCollectionItemTypes.File)
                PictureCollectionService.AddItem(collectionId, itemType, refFid: video.FID);
            else
                PictureCollectionService.AddItem(collectionId, itemType, refDataId: video.DataID);
        }

        private void CreateCollectionAndAdd(Video video)
        {
            var input = new DialogInput(LangManager.GetValueByKey("NewPictureCollection"));
            if (input.ShowDialog(App.Current.MainWindow) != true)
                return;
            string name = input.Text?.Trim();
            if (string.IsNullOrEmpty(name))
                return;
            var created = PictureCollectionService.Create(ConfigManager.Main.CurrentDBId, name);
            if (created == null)
                return;
            foreach (Video selected in GetSelectedPictureVideos(video))
                AddVideoToCollectionSingle(selected, created.CollectionID);
        }

        private void RemoveFromCollection_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menu) || !(menu.Tag is PictureCollectionMenuTag tag))
                return;
            foreach (long collectionId in tag.CollectionIds) {
                if (tag.ItemType == PictureCollectionItemTypes.File)
                    PictureCollectionService.RemoveItem(collectionId, tag.ItemType, refFid: tag.RefFid);
                else
                    PictureCollectionService.RemoveItem(collectionId, tag.ItemType, refDataId: tag.RefDataId);
            }
        }

        private IEnumerable<Video> GetSelectedPictureVideos(Video fallback)
        {
            if (vieModel?.SelectedVideo != null && vieModel.SelectedVideo.Count > 0)
                return vieModel.SelectedVideo;
            return new[] { fallback };
        }

        private string ResolvePictureItemType(Video video)
        {
            if (PictureBrowseContext.BrowseMode == PictureBrowseMode.SingleImage && video.FID > 0)
                return PictureCollectionItemTypes.File;
            return PictureCollectionItemTypes.Album;
        }

        private sealed class PictureCollectionMenuTag
        {
            public PictureCollectionMenuTag(string itemType, long refDataId, long refFid, List<long> collectionIds)
            {
                ItemType = itemType;
                RefDataId = refDataId;
                RefFid = refFid;
                CollectionIds = collectionIds ?? new List<long>();
            }

            public string ItemType { get; }
            public long RefDataId { get; }
            public long RefFid { get; }
            public List<long> CollectionIds { get; }
        }
    }
}
