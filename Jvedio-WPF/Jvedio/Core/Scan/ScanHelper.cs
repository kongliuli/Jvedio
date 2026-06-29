using Jvedio.Core.Enums;
using Jvedio.Core.Metadata;
using Jvedio.Core.Scan;
using Jvedio.Entity;
using QueryEngine;
using SuperUtils.Common;
using SuperUtils.IO;
using SuperUtils.Security;
using SuperUtils.Time;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using static Jvedio.App;

namespace Jvedio.Core.Scan
{
    public class VideoParser : IScan
    {
        private const string CHARACTERS = "abcdefghijklmn";
        public const int DEFAULT_MIN_FILESIZE = 0 * 1024 * 1024;   // 1MB

        private Action<string> Log { get; set; }

        public VideoParser(Action<string> log = null)
        {
            Log = log;
            Log?.Invoke("init video scan");
        }

        #region "属性"

        /// <summary>
        /// 最小文件大小（B）
        /// </summary>
        public static double MinFileSize { get; set; } = DEFAULT_MIN_FILESIZE;

        public static string SubSectionFeature { get; set; } = "-,_,cd,-cd,hd,whole";

        public static List<string> FilePattern { get; set; } = new List<string>();

        public static HashSet<string> FilePatternSet { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        #endregion

        public static void InitSearchPattern()
        {
            // 视频后缀来自 Everything (位置：搜索-管理筛选器-视频-编辑)
            FilePattern = ScanExtensions.VIDEO_EXTENSIONS_LIST;
            FilePatternSet = ScanExtensions.VIDEO_EXTENSIONS_SET;
        }

        public long GetMinFileSize()
        {
            Log?.Invoke($"min file size: {ConfigManager.ScanConfig.MinFileSize}");
            return (long)ConfigManager.ScanConfig.MinFileSize * 1024 * 1024;
        }

        public (List<Video> import, Dictionary<string, NotImportReason> notImport, List<string> failNFO)
            ParseMovie(List<string> filepaths, List<string> fileExt, CancellationToken ct,
            bool insertNFO = true, Action<string> callBack = null, long minFileSize = 0)
        {
            if (filepaths == null || filepaths.Count == 0)
                return (new List<Video>(), new Dictionary<string, NotImportReason>(), new List<string>());

            List<Video> import = new List<Video>();
            List<string> failNFO = new List<string>();
            Dictionary<string, NotImportReason> notImport = new Dictionary<string, NotImportReason>();

            List<string> nfoPaths = new List<string>();
            List<string> videoPaths = new List<string>();


            if (minFileSize <= 0) {
                minFileSize = GetMinFileSize();
                if (minFileSize < 0)
                    minFileSize = DEFAULT_MIN_FILESIZE;
            }

            HashSet<string> fileExtSet = new HashSet<string>(
                fileExt.Select(arg => arg.ToLower()),
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in filepaths) {
                if (string.IsNullOrEmpty(item))
                    continue;
                string path = item.ToLower().Trim();
                if (string.IsNullOrEmpty(path))
                    continue;

                if (path.EndsWith(".nfo"))
                    nfoPaths.Add(item);
                else {
                    if (fileExtSet.Contains(Path.GetExtension(item).ToLower()))
                        videoPaths.Add(item);
                    else
                        notImport.Add(item, NotImportReason.NotInExtension);
                }
            }

            // 1. 先识别 nfo 再导入视频，避免路径覆盖
            if (insertNFO && nfoPaths.Count > 0) {
                Dictionary<string, List<string>> dirVideoCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in nfoPaths) {
                    Movie movie = null;
                    try {
                        movie = NfoMetadataReader.TryReadMovie(item, dirVideoCache);
                    } catch (Exception ex) {
                        Logger.Error(ex);
                        continue;
                    }

                    if (movie != null) {
                        Video video = movie.toVideo();
                        video.Path = item;
                        video.LastScanDate = DateHelper.Now();
                        video.Hash = Encrypt.FasterMd5(video.Path);
                        import.Add(video);
                    } else
                        failNFO.Add(item); // 未从 nfo 中识别出有效的信息
                }
            }

            // 2. 导入视频
            if (videoPaths.Count > 0) {
                try {
                    List<Video> videos = DistinctMovie(videoPaths, ct, (list) => {
                        foreach (var key in list.Keys) {
                            notImport.Add(key, list[key]);
                        }
                    });

                    // 检查是否大于给定大小的影片
                    foreach (var item in videos.Where(arg => arg.Size < minFileSize).Select(arg => arg.Path)) {
                        if (notImport.ContainsKey(item))
                            continue;
                        notImport.Add(item, NotImportReason.SizeTooSmall);
                    }

                    videos.RemoveAll(arg => arg.Size < minFileSize);
                    import.AddRange(videos);
                } catch (OperationCanceledException) {
                    callBack?.Invoke($"{SuperControls.Style.LangManager.GetValueByKey("Cancel")}");
                } catch (Exception ex) {
                    callBack?.Invoke(ex.Message);
                }
            }


            return (import, notImport, failNFO);
        }

        public static bool IsProperMovie(string filePath)
        {
            return File.Exists(filePath) &&
                FilePatternSet.Contains(System.IO.Path.GetExtension(filePath).ToLower()) &&
                new System.IO.FileInfo(filePath).Length >= MinFileSize;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public List<string> ScanAllDrives()
        {
            List<string> result = new List<string>();
            try {
                var entries = Engine.GetAllFilesAndDirectories();
                entries.ForEach(arg => {
                    if (arg is FileAndDirectoryEntry & !arg.IsFolder) {
                        result.Add(arg.FullFileName);
                    }
                });
            } catch (Exception e) {
                Logger.Error(e);
            }

            // 扫描根目录
            StringCollection stringCollection = new StringCollection();
            foreach (var item in Environment.GetLogicalDrives()) {
                try {
                    if (Directory.Exists(item))
                        stringCollection.Add(item);
                } catch (Exception e) {
                    Logger.Error(e);
                    continue;
                }
            }

            result.AddRange(ScanTopPaths(stringCollection));
            return FilterFiles(result);
        }

        // 根据 视频后缀文件大小筛选
        public List<string> FilterFiles(List<string> filePathList, string ID = "")
        {
            if (filePathList == null || filePathList.Count == 0)
                return new List<string>();
            try {
                bool filterById = ID != string.Empty;
                string idUpper = filterById ? ID.ToUpper() : string.Empty;
                List<string> result = new List<string>();
                foreach (string s in filePathList) {
                    if (!FilePatternSet.Contains(Path.GetExtension(s).ToLower()))
                        continue;
                    if (File.Exists(s) && new FileInfo(s).Length < MinFileSize)
                        continue;
                    if (filterById) {
                        try {
                            if (JvedioLib.Security.Identify.GetVID(new FileInfo(s).Name).ToUpper() != idUpper)
                                continue;
                        } catch {
                            Logger.Warn($"错误路径：{s}");
                            continue;
                        }
                    }
                    result.Add(s);
                }
                result.Sort(StringComparer.OrdinalIgnoreCase);
                return result;
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return new List<string>();
        }

        public List<string> ScanTopPaths(StringCollection stringCollection)
        {
            List<string> result = new List<string>();
            if (stringCollection == null || stringCollection.Count == 0)
                return result;
            foreach (var item in stringCollection) {
                try {
                    foreach (var path in Directory.GetFiles(item, "*.*", SearchOption.TopDirectoryOnly))
                        result.Add(path);
                } catch {
                    continue;
                }
            }

            return result;
        }

        public static List<string> GetSubSectionFeature()
        {
            List<string> result = new List<string>();
            string splitFeature = SubSectionFeature.Replace("，", ",");
            string[] parts = splitFeature.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in parts) {
                if (!string.IsNullOrEmpty(item) && !result.Contains(item))
                    result.Add(item);
            }
            return result;
        }

        private static string _cachedSubSectionFeatureConfig;
        private static Regex _numericSubSectionRegex;
        private static Regex _alphaSubSectionRegex;

        private static void EnsureSubSectionRegex()
        {
            if (_numericSubSectionRegex != null && _cachedSubSectionFeatureConfig == SubSectionFeature)
                return;

            _cachedSubSectionFeatureConfig = SubSectionFeature;
            List<string> features = GetSubSectionFeature();
            StringBuilder builder = new StringBuilder();
            foreach (var item in features)
                builder.Append(Regex.Escape(item)).Append("|");
            if (builder.Length == 0) {
                _numericSubSectionRegex = null;
                _alphaSubSectionRegex = null;
                return;
            }
            string featurePattern = builder.ToString(0, builder.Length - 1);
            _numericSubSectionRegex = new Regex("(" + featurePattern + ")(?<num>[0-9]{1,2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _alphaSubSectionRegex = new Regex("((" + featurePattern + ")|[0-9]{1,})(?<alpha>[a-n])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private static bool HasContinuousNumericParts(List<string> fileNames, int expectedCount)
        {
            EnsureSubSectionRegex();
            if (_numericSubSectionRegex == null)
                return false;
            HashSet<int> numbers = new HashSet<int>();
            foreach (string name in fileNames) {
                foreach (Match match in _numericSubSectionRegex.Matches(name)) {
                    if (int.TryParse(match.Groups["num"].Value, out int num))
                        numbers.Add(num);
                }
            }
            if (numbers.Count < expectedCount)
                return false;
            for (int i = 1; i <= expectedCount; i++) {
                if (!numbers.Contains(i))
                    return false;
            }
            return true;
        }

        private static bool HasContinuousAlphaParts(List<string> fileNames, int expectedCount)
        {
            EnsureSubSectionRegex();
            if (_alphaSubSectionRegex == null)
                return false;
            HashSet<char> letters = new HashSet<char>();
            foreach (string name in fileNames) {
                foreach (Match match in _alphaSubSectionRegex.Matches(name)) {
                    string alpha = match.Groups["alpha"].Value.ToLower();
                    if (alpha.Length == 1)
                        letters.Add(alpha[0]);
                }
            }
            int count = Math.Min(expectedCount, CHARACTERS.Length);
            for (int i = 0; i < count; i++) {
                if (!letters.Contains(CHARACTERS[i]))
                    return false;
            }
            return letters.Count >= count;
        }

        /// <summary>
        /// 给出一组视频路径，返回否是分段视频，并返回分段的视频列表，以及不是分段的视频列表
        /// </summary>
        /// <param name="filePathList"></param>
        /// <returns></returns>

        public static (bool, List<string>, List<string>) HandleSubSection(List<string> filePathList)
        {
            if (filePathList == null || filePathList.Count == 0)
                return (false, new List<string>(), new List<string>());

            bool isSubSection = true;
            List<string> notSubSection = new List<string>();

            string fatherPath = new FileInfo(filePathList[0]).Directory.FullName;
            bool sameFatherPath = filePathList.All(arg => fatherPath.ToLower()
                .Equals(FileHelper.TryGetFullName(arg)?.ToLower()));

            if (!sameFatherPath) {
                // 并不是所有父目录都相同，提取出父目录最多的文件
                Dictionary<string, int> fatherPathDic = new Dictionary<string, int>();
                filePathList.ForEach(path => {
                    string father_path = new FileInfo(path).Directory.FullName;
                    if (fatherPathDic.ContainsKey(father_path))
                        fatherPathDic[father_path] += 1;
                    else
                        fatherPathDic.Add(father_path, 1);
                });

                object maxKey = DataHelper.TryGetMaxCountKey(fatherPathDic);
                string maxValueKey = maxKey == null ? string.Empty : maxKey.ToString();
                if (!string.IsNullOrEmpty(maxValueKey)) {
                    try {
                        var notsub = fatherPathDic.Where(arg => arg.Key != null &&
                            !maxValueKey.Equals(FileHelper.TryGetFullName(arg.Key))).ToList();
                        notsub.ForEach(arg => notSubSection.Add(arg.Key));
                    } catch (Exception e) {
                        Logger.Error(e);
                    }

                    filePathList = filePathList.Where(arg => maxValueKey.Equals(FileHelper.TryGetFullName(arg))).ToList();
                }
            }

            // 目录都相同，判断是否分段视频的特征

            // -1,-2,-3,...
            // cd1,cd2,...
            // _1,_2,...
            // fhd1,fhd2,...
            // ...
            List<string> originPaths = filePathList.Select(arg => arg).ToList();
            List<string> fileNames = originPaths
                .Select(arg => Path.GetFileNameWithoutExtension(arg).ToLower())
                .OrderBy(arg => arg)
                .ToList();

            isSubSection = HasContinuousNumericParts(fileNames, fileNames.Count);
            if (!isSubSection)
                isSubSection = HasContinuousAlphaParts(fileNames, fileNames.Count);

            // 排序文件名
            originPaths = originPaths.OrderBy(arg => arg).ToList();
            if (!isSubSection)
                return (isSubSection, new List<string>(), originPaths);

            return (isSubSection, originPaths, notSubSection);
        }

        public List<string> GetAllFilesFromFolder(string root, CancellationToken cancellationToken, string pattern = "", Action<string> callBack = null)
        {
            Queue<string> folders = new Queue<string>();
            List<string> files = new List<string>();
            folders.Enqueue(root);
            while (folders.Count != 0) {
                cancellationToken.ThrowIfCancellationRequested();
                string currentFolder = folders.Dequeue();
                try {
                    string[] filesInCurrent = System.IO.Directory.GetFiles(currentFolder, pattern == string.Empty ? "*.*" : pattern, System.IO.SearchOption.TopDirectoryOnly);
                    files.AddRange(filesInCurrent);
                    foreach (var file in filesInCurrent) {
                        callBack?.Invoke(file);
                    }
                } catch {
                }

                try {
                    string[] foldersInCurrent = System.IO.Directory.GetDirectories(currentFolder, pattern == string.Empty ? "*.*" : pattern, System.IO.SearchOption.TopDirectoryOnly);
                    foreach (string _current in foldersInCurrent) {
                        folders.Enqueue(_current);
                        callBack?.Invoke(_current);
                    }
                } catch {
                }
            }

            return files;
        }

        // todo Europe 影片
        /// <summary>
        /// 分类视频并导入
        /// </summary>
        /// <param name="videoPaths"></param>
        /// <param name="ct"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        public List<Video> DistinctMovie(List<string> videoPaths, CancellationToken ct, Action<Dictionary<string, NotImportReason>> sameVideoCallBack)
        {
            List<Video> result = new List<Video>();
            Dictionary<string, List<string>> vIDDict = new Dictionary<string, List<string>>();
            string sep = SuperUtils.Values.ConstValues.SeparatorString;
            List<string> noVIDList = new List<string>();

            // 检查无识别码的视频
            foreach (string path in videoPaths) {
                if (!File.Exists(path))
                    continue;
                string VID = string.Empty;
                if (ConfigManager.ScanConfig.FetchVID)
                    VID = JvedioLib.Security.Identify.GetVID(Path.GetFileNameWithoutExtension(path));
                if (string.IsNullOrEmpty(VID)) {
                    // 无识别码
                    noVIDList.Add(path);
                } else {
                    // 有识别码
                    // 检查分段的视频
                    if (!vIDDict.ContainsKey(VID)) {
                        List<string> pathList = new List<string> { path };
                        vIDDict.Add(VID, pathList);
                    } else {
                        vIDDict[VID].Add(path); // 每个 VID 对应一组视频路径，视频路径 >=2 的可能为分段视频
                    }
                }
            }

            // 检查分段的视频
            foreach (string vID in vIDDict.Keys) {
                List<string> paths = vIDDict[vID];
                if (paths.Count <= 1) {
                    // 一个 VID 对应 一个路径
                    string path = paths[0];

                    result.Add(ParseVideo(vID, path));
                } else {
                    // 一个 VID 对应多个路径
                    // 路径个数大于1 才为分段视频
                    (bool isSubSection, List<string> subSectionList, List<string> notSubSection) = (false, new List<string>(), new List<string>());
                    try {
                        (isSubSection, subSectionList, notSubSection) = HandleSubSection(paths);
                    } catch (Exception ex) {
                        Logger.Warn("解析分段视频错误");
                        Logger.Error(ex);
                    }

                    if (isSubSection) {
                        // 文件大小视为所有文件之和
                        long size = subSectionList.Sum(arg => FileHelper.TryGetFileLength(arg));
                        string subsection = string.Join(sep, subSectionList);
                        string firstPath = subSectionList[0];
                        result.Add(ParseVideo(vID, firstPath, subsection, size));
                    } else {
                        // 同 VID 非分段：按 Hash 去重，Hash 不同则全部保留
                        Dictionary<string, List<string>> hashGroups = new Dictionary<string, List<string>>(StringComparer.Ordinal);
                        Dictionary<string, string> pathHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (string path in notSubSection) {
                            string hash = Encrypt.FasterMd5(path);
                            pathHashes[path] = hash;
                            if (!hashGroups.TryGetValue(hash, out List<string> groupedPaths))
                                hashGroups[hash] = groupedPaths = new List<string>();
                            groupedPaths.Add(path);
                        }

                        Dictionary<string, NotImportReason> notImportList = new Dictionary<string, NotImportReason>();
                        foreach (var group in hashGroups.Values) {
                            group.Sort(StringComparer.OrdinalIgnoreCase);
                            for (int i = 1; i < group.Count; i++)
                                notImportList[group[i]] = NotImportReason.RepetitiveVID;
                            result.Add(ParseVideo(vID, group[0], hash: pathHashes[group[0]]));
                        }
                        sameVideoCallBack?.Invoke(notImportList);
                    }
                }

                ct.ThrowIfCancellationRequested();
            }

            foreach (string path in noVIDList) {
                result.Add(ParseVideo(string.Empty, path, calcHash: true));
            }

            return result;
        }

        private Video ParseVideo(string vID, string path, string subsection = "", long size = -1, bool calcHash = false, string hash = null)
        {
            FileInfo fileInfo = new FileInfo(path); // 原生的速度最快
            Video video = new Video() {
                Path = path,
                VID = vID,
                Size = fileInfo.Length,
                VideoType = (VideoType)JvedioLib.Security.Identify.GetVideoType(vID),
                FirstScanDate = DateHelper.Now(),
                CreateDate = DateHelper.ToLocalDate(fileInfo.CreationTime),
            };
            if (size >= 0)
                video.Size = size;
            if (!string.IsNullOrEmpty(subsection))
                video.SubSection = subsection;
            if (!string.IsNullOrEmpty(hash))
                video.Hash = hash;
            else if (calcHash)
                video.Hash = Encrypt.FasterMd5(path);
            return video;
        }
    }
}
