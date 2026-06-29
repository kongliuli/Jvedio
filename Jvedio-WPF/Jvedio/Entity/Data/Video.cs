using Google.Protobuf.WellKnownTypes;
using Jvedio.Core.Enums;
using Jvedio.Core.Global;
using Jvedio.Core.Media;
using Jvedio.Core.Metadata;
using Jvedio.Core.Scan;
using Jvedio.Entity.CommonSQL;
using Jvedio.Mapper;
using Newtonsoft.Json;
using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.Framework.ORM.Attributes;
using SuperUtils.Framework.ORM.Enums;
using SuperUtils.Framework.ORM.Wrapper;
using SuperUtils.IO;
using SuperUtils.Media;
using SuperUtils.NetWork;
using SuperUtils.Reflections;
using SuperUtils.Time;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using static Jvedio.App;

namespace Jvedio.Entity
{
    // todo 检视
    [Table(tableName: "metadata_video")]
    public partial class Video : MetaData
    {
        #region "事件"

        #endregion


        #region "静态属性"

        public static string[] HDV { get; set; } =
            new string[] { "hd", "high_definition", "high definition", "高清", "2k", "4k", "8k", "16k", "32k" };


        #endregion


        #region "属性"


        private long _MVID;

        [TableId(IdType.AUTO)]
        public long MVID {
            get { return _MVID; }
            set {
                _MVID = value;
                RaisePropertyChanged();
            }
        }


        /// <summary>
        /// 此处不可删除，需要保证 select 查出 id
        /// </summary>
        public long _DataID;
        public new long DataID {
            get { return _DataID; }
            set {
                _DataID = value;
                RaisePropertyChanged();
            }
        }

        private string _VID;
        public string VID {
            get { return _VID; }
            set {
                _VID = value;
                RaisePropertyChanged();
            }
        }


        private string _Series;

        public string Series {
            get { return _Series; }

            set {
                _Series = value;
                SeriesList = new ObservableCollection<ObservableString>();
                if (!string.IsNullOrEmpty(value))
                    foreach (var item in value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries))
                        SeriesList.Add(new ObservableString(item));

                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ObservableString> _SeriesList;
        [TableField(exist: false)]
        public ObservableCollection<ObservableString> SeriesList {
            get { return _SeriesList; }
            set {
                _SeriesList = value;
                RaisePropertyChanged();
            }
        }



        private VideoType _VideoType;

        public VideoType VideoType {
            get { return _VideoType; }

            set {
                _VideoType = value;
                RaisePropertyChanged();
            }
        }

        public string Director { get; set; }

        public string Studio { get; set; }

        public string Publisher { get; set; }

        public string Plot { get; set; }

        public string Outline { get; set; }

        public int Duration { get; set; }


        private ObservableCollection<ObservableString> _SubSectionList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<ObservableString> SubSectionList {
            get { return _SubSectionList; }
            set {
                _SubSectionList = value;
                RaisePropertyChanged();
            }
        }

        private string _SubSection = string.Empty;

        public string SubSection {
            get { return _SubSection; }

            set {
                _SubSection = value;
                SubSectionList = SubSectionToList(value);
                if (SubSectionList.Count >= 2)
                    HasSubSection = true;
                else
                    HasSubSection = false;
                RaisePropertyChanged();
            }
        }


        private bool _HasSubSection;
        [TableField(exist: false)]
        public bool HasSubSection {
            get { return _HasSubSection; }
            set {
                _HasSubSection = value;
                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public ObservableCollection<string> PreviewImagePathList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<BitmapSource> PreviewImageList { get; set; }

        public string ImageUrls { get; set; }

        public string WebType { get; set; }

        public string WebUrl { get; set; }

        public string ExtraInfo { get; set; }

        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage {
            get { return _smallimage; }

            set {
                _smallimage = value;
                RaisePropertyChanged();
            }
        }

        private BitmapSource _bigimage;

        [TableField(exist: false)]
        public BitmapSource BigImage {
            get { return _bigimage; }

            set {
                _bigimage = value;
                RaisePropertyChanged();
            }
        }
        private string _BigImagePath;

        [TableField(exist: false)]
        public string BigImagePath {
            get { return _BigImagePath; }

            set {
                _BigImagePath = value;
                RaisePropertyChanged();
            }
        }

        private Uri _GifUri;

        [TableField(exist: false)]
        public Uri GifUri {
            get { return _GifUri; }

            set {
                _GifUri = value;
                RaisePropertyChanged();
            }
        }

        private string _ActorNames;

        [TableField(exist: false)]
        public string ActorNames {
            get { return _ActorNames; }

            set {
                _ActorNames = value;
                if (!string.IsNullOrEmpty(value))
                    ActorNameList = value.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();

                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> ActorNameList { get; set; }

        /// <summary>
        /// 旧数据库的 actorID 列表
        /// </summary>
        [TableField(exist: false)]
        public string OldActorIDs { get; set; }

        private List<ActorInfo> _ActorInfos;

        [TableField(exist: false)]
        public List<ActorInfo> ActorInfos {
            get { return _ActorInfos; }

            set {
                _ActorInfos = value;
                if (value != null) {
                    ActorNames = string.Join(SuperUtils.Values.ConstValues.SeparatorString,
                        value.Select(arg => arg.ActorName).ToList());
                }

                RaisePropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<Magnet> Magnets { get; set; }


        private bool _HasAssociation;

        [TableField(exist: false)]
        public bool HasAssociation {

            get { return _HasAssociation; }


            set {
                _HasAssociation = value;
                RaisePropertyChanged();
            }

        }


        private ObservableCollection<long> _AssociationList;

        [TableField(exist: false)]
        public ObservableCollection<long> AssociationList {
            get { return _AssociationList; }
            set {
                _AssociationList = value;
                RaisePropertyChanged();
            }
        }

        // 仅用于 NFO 导入的时候的图片地址
        [TableField(exist: false)]
        public List<string> ActorThumbs { get; set; }

        #endregion



        public Video() : this(true)
        {
        }

        public Video(bool _initDefaultImage = true)
        {
            if (_initDefaultImage)
                InitDefaultImage();
        }

        #region "对外静态方法"


        public static void RefreshTagStamp(ref Video video, long newTagID, bool deleted)
        {
            if (video == null || newTagID <= 0)
                return;
            string tagIDs = video.TagIDs;
            if (!deleted && string.IsNullOrEmpty(tagIDs)) {
                video.TagStamp = new ObservableCollection<TagStamp>();
                video.TagStamp.Add(Jvedio.Entity.CommonSQL.TagStamp.TagStamps.Where(arg => arg.TagID == newTagID).FirstOrDefault());
                video.TagIDs = newTagID.ToString();
            } else {
                List<string> list = tagIDs.Split(',').ToList();
                if (!deleted && !list.Contains(newTagID.ToString()))
                    list.Add(newTagID.ToString());
                if (deleted && list.Contains(newTagID.ToString()))
                    list.Remove(newTagID.ToString());
                video.TagIDs = string.Join(",", list);
                video.TagStamp = new ObservableCollection<TagStamp>();
                foreach (var arg in list) {
                    long.TryParse(arg, out long id);
                    video.TagStamp.Add(Jvedio.Entity.CommonSQL.TagStamp.TagStamps.Where(item => item.TagID == id).FirstOrDefault());
                }
            }
        }


        public void OpenWeb()
        {
            string url = WebUrl;
            if (string.IsNullOrEmpty(url))
                return;
            if (url.IsProperUrl())
                FileHelper.TryOpenUrl(url);
        }


        public static OpenPathType StringToImageType(string type)
        {
            if (type.Equals(SuperControls.Style.LangManager.GetValueByKey("Movie"))) {
                return OpenPathType.Video;
            } else if (type.Equals(SuperControls.Style.LangManager.GetValueByKey("Poster"))) {
                return OpenPathType.Poster;
            } else if (type.Equals(SuperControls.Style.LangManager.GetValueByKey("Thumbnail"))) {
                return OpenPathType.Thumnail;
            } else if (type.Equals(SuperControls.Style.LangManager.GetValueByKey("Preview"))) {
                return OpenPathType.Preview;
            } else if (type.Equals(SuperControls.Style.LangManager.GetValueByKey("ScreenShot"))) {
                return OpenPathType.ScreenShot;
            } else if (type.ToUpper().Equals("GIF")) {
                return OpenPathType.Gif;
            }
            return OpenPathType.Video;
        }

        public void OpenPath(OpenPathType type)
        {
            string target = "";
            if (type == OpenPathType.Video) {
                target = Path;
                if (!File.Exists(Path)) {
                    MessageCard.Error(SuperControls.Style.LangManager.GetValueByKey("Message_FileNotExist") + ": " + Path);
                } else {
                    FileHelper.TryOpenSelectPath(Path);
                }
            } else if (type == OpenPathType.Poster) {
                FileHelper.TryOpenSelectPath(GetBigImage());
            } else if (type == OpenPathType.Thumnail) {
                FileHelper.TryOpenSelectPath(GetSmallImage());
            } else if (type == OpenPathType.Preview) {
                FileHelper.TryOpenSelectPath(GetExtraImage());
            } else if (type == OpenPathType.ScreenShot) {
                FileHelper.TryOpenSelectPath(GetScreenShot());
            } else if (type == OpenPathType.Gif) {
                FileHelper.TryOpenSelectPath(GetGifPath());
            }
        }

        #endregion

        /// <summary>
        /// 延迟加载图片
        /// </summary>
        public void InitDefaultImage()
        {
            SmallImage = MetaData.DefaultSmallImage;
            BigImage = MetaData.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            PreviewImageList = new ObservableCollection<BitmapSource>();
        }

        /// <summary>
        /// 设置标签戳
        /// </summary>
        /// <param name="video"></param>
        public static void SetTagStamps(ref Video video)
        {
            video.TagStamp = new ObservableCollection<TagStamp>();
            if (video == null || string.IsNullOrEmpty(video.TagIDs))
                return;
            List<long> list = video.TagIDs.Split(',').Select(arg => long.Parse(arg)).ToList();
            if (list != null && list.Count > 0) {
                foreach (var item in Jvedio.Entity.CommonSQL.TagStamp.TagStamps.Where(arg => list.Contains(arg.TagID)).ToList())
                    video.TagStamp.Add(item);
            }
        }

        /// <summary>
        /// 设置标题和发行日期
        /// </summary>
        /// <param name="video"></param>
        public static void SetTitleAndDate(ref Video video)
        {
            if (ConfigManager.VideoConfig.ShowFileNameIfTitleEmpty
                && !string.IsNullOrEmpty(video.Path) && string.IsNullOrEmpty(video.Title))
                video.Title = System.IO.Path.GetFileNameWithoutExtension(video.Path);
            if (ConfigManager.VideoConfig.ShowCreateDateIfReleaseDateEmpty
                && !string.IsNullOrEmpty(video.LastScanDate) && string.IsNullOrEmpty(video.ReleaseDate))
                video.ReleaseDate = DateHelper.ToLocalDate(video.LastScanDate);

            //if (string.IsNullOrEmpty(video.VID) && !string.IsNullOrEmpty(video.Title))
            //    video.VID = video.Title;

        }


        public bool ToDownload()
        {
            return NeedsMetadataDownload(Title, WebUrl, ImageUrls);
        }

        public static bool NeedsMetadataDownload(string title, string webUrl, string imageUrls)
        {
            if (ConfigManager.Settings.DownloadWhenTitleNull) {
                return string.IsNullOrEmpty(title);
            }
            return string.IsNullOrEmpty(title) ||
                string.IsNullOrEmpty(webUrl) ||
                string.IsNullOrEmpty(imageUrls);
        }

        public override string ToString()
        {
            return ClassUtils.ToString(this);
        }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }

        public Dictionary<string, object> ToDictionary()
        {
            List<string> fields = new List<string> { "VideoType", "DataID", "VID", "Size", "Path", "Hash", "DataType" };
            Dictionary<string, object> dict = new Dictionary<string, object>();
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos) {
                if (fields.Contains(info.Name)) {
                    object value = info.GetValue(this);
                    if (value == null)
                        value = string.Empty;
                    dict.Add(info.Name, value);
                }
            }

            return dict;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Video video = obj as Video;
            return video != null && (video.DataID == this.DataID || video.MVID == this.MVID);
        }

        public bool ParseDictInfo(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos) {
                if (dict.ContainsKey(info.Name)) {
                    object value = dict[info.Name];
                    if (value == null)
                        continue;
                    if (value is List<string> list) {
                        info.SetValue(this, string.Join(SuperUtils.Values.ConstValues.SeparatorString, list));
                    } else if (value is string str) {
                        if (info.PropertyType == typeof(string)) {
                            info.SetValue(this, str);
                        } else if (info.PropertyType == typeof(int)) {
                            int.TryParse(str, out int val);
                            info.SetValue(this, val);
                        }
                    }
                }
            }

            // 图片地址
            ImageUrls = ParseImageUrlFromDict(dict);
            return true;
        }

        private string ParseImageUrlFromDict(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
                return string.Empty;
            Dictionary<string, object> result = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(ImageUrls);
            if (result == null)
                result = new Dictionary<string, object>();
            if (dict.ContainsKey("SmallImageUrl"))
                result["SmallImageUrl"] = dict["SmallImageUrl"];
            if (dict.ContainsKey("BigImageUrl"))
                result["BigImageUrl"] = dict["BigImageUrl"];
            if (dict.ContainsKey("ExtraImageUrl"))
                result["ExtraImageUrl"] = dict["ExtraImageUrl"];
            if (dict.ContainsKey("ActressImageUrl"))
                result["ActressImageUrl"] = dict["ActressImageUrl"];
            if (dict.ContainsKey("ActorNames"))
                result["ActorNames"] = dict["ActorNames"];
            return JsonConvert.SerializeObject(result);
        }

        public void SaveNfo()
        {
            MetadataSaver.Default.SaveNfo(this);
        }

        [Obsolete("Use Jvedio.Core.Metadata.NfoMetadataWriter.WriteVideo")]
        public static void SaveToNFO(Video video, string nfoPath)
        {
            NfoMetadataWriter.WriteVideo(video, nfoPath);
        }

        public static string ToSqlField(string content)
        {
            if (content == SuperControls.Style.LangManager.GetValueByKey("ID")) {
                return "VID";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Title")) {
                return "Title";
            }

              // else if (content == SuperControls.Style.LangManager.GetValueByKey("TranslatedTitle"))
              // {
              //    return "chinesetitle";
              // }
              else if (content == SuperControls.Style.LangManager.GetValueByKey("VideoType")) {
                return "VideoType";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Tag")) {
                return "Series";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("ReleaseDate")) {
                return "ReleaseDate";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Year")) {
                return "ReleaseYear";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Duration")) {
                return "Duration";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Country")) {
                return "Country";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Director")) {
                return "Director";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Genre")) {
                return "Genre";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Label")) {
                return "Label";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Actor")) {
                return "ActorNames";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Studio")) {
                return "Studio";
            } else if (content == SuperControls.Style.LangManager.GetValueByKey("Rating")) {
                return "Rating";
            } else {
                return content;
            }
        }

        public string[] ToFileName()
        {
            return Jvedio.Core.Rename.RenameEngine.BuildTargetPaths(this);
        }


        public bool IsHDV()
        {
            return JvedioLib.Security.Identify.IsHDV(Size) ||
                   JvedioLib.Security.Identify.IsHDV(Path) ||
                   Genre?.IndexOfAnyString(Main.TagStringHD) >= 0 ||
                   Series?.IndexOfAnyString(Main.TagStringHD) >= 0 ||
                   Label?.IndexOfAnyString(Main.TagStringHD) >= 0;
        }

        public bool IsCHS()
        {
            return JvedioLib.Security.Identify.IsCHS(Path) ||
                   Genre?.IndexOfAnyString(Main.TagStringTranslated) >= 0 ||
                   Series?.IndexOfAnyString(Main.TagStringTranslated) >= 0 ||
                   Label?.IndexOfAnyString(Main.TagStringTranslated) >= 0;
        }

        public static void SetImage(ref Video video, string imgPath)
        {
            if (video == null)
                return;
            BitmapImage image =
                ImageCache.Get(imgPath, Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
            if (image == null)
                image = MetaData.DefaultBigImage;
            video.ViewImage = image;
        }

        private static string PickMiddleScreenShotPath(string[] arr)
        {
            return arr[arr.Length / 2];
        }


        private static void SetScreenShotImage(ref Video video)
        {

            if (ConfigManager.Settings.AutoGenScreenShot) {
                // 检查有无截图
                string path = video.GetScreenShot();
                if (Directory.Exists(path)) {
                    string[] array = FileHelper.TryScanDIr(path, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                    if (array.Length > 0) {
                        string targetPath = PickMiddleScreenShotPath(array);
                        Video.SetImage(ref video, targetPath);
                        video.BigImagePath = targetPath;
                    }
                }
            }
        }


        public static void SetImage(ref Video video, int imageMode = 0)
        {
            video.BigImagePath = video.GetBigImage();

            BitmapImage smallimage = ImageCache.Get(video.GetSmallImage(), Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);
            BitmapImage bigimage = ImageCache.Get(video.BigImagePath, Jvedio.Core.WindowConfig.Main.MAX_IMAGE_WIDTH);

            bool findScreenShot = false;

            video.SmallImage = null;
            video.BigImage = null;
            if (smallimage == null) {
                SetScreenShotImage(ref video);
                findScreenShot = true;
                if (video.ViewImage != null)
                    video.SmallImage = video.ViewImage;
                else
                    video.SmallImage = DefaultSmallImage;
            } else {
                video.SmallImage = smallimage;
            }

            if (bigimage == null) {
                if (!findScreenShot)
                    SetScreenShotImage(ref video);

                if (video.ViewImage != null)
                    video.BigImage = video.ViewImage;
                else
                    video.BigImage = DefaultBigImage;
            } else {
                video.BigImage = bigimage;
            }
        }


        public override int GetHashCode()
        {
            int hashCode = -485885450;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DataID.GetHashCode();
            return hashCode;
        }

        private bool CanSubSectionSortByNum(List<string> list)
        {
            if (list == null || list.Count == 0)
                return false;

            bool[] result = new bool[list.Count];

            for (int i = 0; i < list.Count; i++) {
                for (int j = 0; j < list.Count; j++) {
                    string value = $"-{(i + 1)}";
                    if (list[j].IndexOf(value) >= 0) {
                        result[i] = true;
                        break;
                    }
                }
            }
            return result.All(arg => arg);
        }

        private ObservableCollection<ObservableString> SubSectionToList(string subSection)
        {
            ObservableCollection<ObservableString> result = new ObservableCollection<ObservableString>();

            if (string.IsNullOrEmpty(subSection) || subSection.IndexOf(SuperUtils.Values.ConstValues.Separator) <= 0)
                return result;

            List<string> list =
                subSection.Split(new char[] { SuperUtils.Values.ConstValues.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> orderList;

            // 大于 10 才需要排序，其他的由用户添加顺序决定
            if (list.Count >= 10 && CanSubSectionSortByNum(list)) {
                orderList = list.OrderBy(arg => arg, new SubSectionComparer()).ToList();
            } else {
                orderList = list;
            }

            foreach (var item in orderList) {
                if (item == null || string.IsNullOrEmpty(item.Trim()))
                    continue;
                result.Add(new ObservableString(item));
            }
            return result;
        }
    }


    /// <summary>
    /// 仅支持形如 -1, -2, -3, ... 的分段视频
    /// </summary>
    public class SubSectionComparer : IComparer<string>
    {

        // ABCD-123-1
        // ABCD-123-2
        // ABCD-123-3
        // ABCD-123-11

        public int Compare(string path1, string path2)
        {
            string name1 = Path.GetFileNameWithoutExtension(path1);
            string name2 = Path.GetFileNameWithoutExtension(path2);
            string vid = JvedioLib.Security.Identify.GetVID(name1);

            int idx1 = name1.IndexOf(vid) + vid.Length + "-".Length;
            int idx2 = name2.IndexOf(vid) + vid.Length + "-".Length;

            string v1 = name1.Substring(idx1);
            string v2 = name2.Substring(idx2);
            int.TryParse(v1, out int c1);
            int.TryParse(v2, out int c2);
            return c1 - c2;
        }
    }
}
