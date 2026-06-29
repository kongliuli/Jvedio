using SuperControls.Style;
using SuperUtils.Common;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Jvedio.App;

namespace Jvedio.Entity.Common
{
    public enum ParseType
    {
        None,
        Str,
        StrUpper,
        StrLower,
        Int,
        Float,
        ArrayStr
    }


    /// <summary>
    /// 一(Jvedio)对多(NFO)映射
    /// </summary>
    public class NfoParse : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private const int DEFAULT_RELEASE_YEAR = 1970;

        private static Dictionary<string, (string key, NfoParse parse)> _aliasIndex =
            new Dictionary<string, (string key, NfoParse parse)>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, PropertyInfo> _moviePropertyCache =
            typeof(Movie).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(arg => arg.Name, arg => arg, StringComparer.OrdinalIgnoreCase);

        #region "属性"

        public static Dictionary<string, NfoParse> CurrentNFOParse { get; set; }

        public static Dictionary<string, NfoParse> DEFAULT_NFO_PARSE { get; set; } = new Dictionary<string, NfoParse>
        {
            // 单个
            { "id", new NfoParse(LangManager.GetValueByKey("ID"),"",ParseType.StrUpper, new List<string>(){"id","num"}) },
            { "title",new NfoParse(LangManager.GetValueByKey("Title"),"",ParseType.Str, new List<string>(){"title"})},
            { "releasedate",new NfoParse(LangManager.GetValueByKey("ReleaseDate"),"",ParseType.Str,new List<string>(){"release","releasedate"})},
            { "director", new NfoParse(LangManager.GetValueByKey("Director"),"",ParseType.Str,new List<string>(){"director"})},
            { "studio", new NfoParse(LangManager.GetValueByKey("Studio"),"",ParseType.Str, new List<string>() { "studio" })},
            { "rating",new NfoParse(LangManager.GetValueByKey("Rating"),0.0,ParseType.Float, new List<string>() { "rating" })},
            { "plot", new NfoParse(LangManager.GetValueByKey("Plot"),"",ParseType.Str, new List<string>() { "plot" })},
            { "outline", new NfoParse(LangManager.GetValueByKey("Outline"),"",ParseType.Str, new List<string>() { "outline" })},
            { "year", new NfoParse(LangManager.GetValueByKey("Year"),DEFAULT_RELEASE_YEAR,ParseType.Int, new List<string>() { "year" })},
            { "runtime", new NfoParse(LangManager.GetValueByKey("Duration"),0,ParseType.Int, new List<string>() { "runtime" })},
            { "country", new NfoParse(LangManager.GetValueByKey("Country"),"",ParseType.Str, new List<string>() { "country" })},
            { "sourceurl", new NfoParse(LangManager.GetValueByKey("Url"),"",ParseType.Str, new List<string>() { "source" })},
            
            // 多个
            { "tag",new NfoParse(LangManager.GetValueByKey("Tag"),null,ParseType.None, new List<string>() { "set","tag"  })},
            { "genre",new NfoParse(LangManager.GetValueByKey("Genre"),null,ParseType.None, new List<string>() { "genre"})},

        };


        private ParseType _ParseType;
        public ParseType ParseType {
            get { return _ParseType; }
            set {
                _ParseType = value;
                RaisePropertyChanged();
            }
        }


        private object _DefaultValue;
        public object DefaultValue {
            get { return _DefaultValue; }
            set {
                _DefaultValue = value;
                RaisePropertyChanged();
            }
        }



        private string _Name;
        public string Name {
            get { return _Name; }
            set {
                _Name = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ObservableString> _ParseValues;
        public ObservableCollection<ObservableString> ParseValues {
            get { return _ParseValues; }
            set {
                _ParseValues = value;
                RaisePropertyChanged();
            }
        }


        #endregion


        static NfoParse()
        {
            InitCurrentNFOParse();
        }

        public NfoParse()
        {
        }

        public NfoParse(NfoParse data)
        {
            this.ParseType = data.ParseType;
            this.DefaultValue = data.DefaultValue;
            this.Name = data.Name;
            this.ParseValues = new ObservableCollection<ObservableString>();
            if (data.ParseValues?.Count > 0) {
                foreach (var item in data.ParseValues) {
                    this.ParseValues.Add(new ObservableString(item.Value));
                }
            }
        }
        public NfoParse(string name, object defaultValue, ParseType type, List<string> values)
        {
            this.DefaultValue = defaultValue;
            this.ParseType = type;
            this.Name = name;
            this.ParseValues = new ObservableCollection<ObservableString>();
            if (values != null) {
                foreach (var item in values) {
                    this.ParseValues.Add(new ObservableString(item));
                }
            }

        }


        /// <summary>
        /// 初始化当前的 NFO 解析规则
        /// </summary>
        public static void InitCurrentNFOParse()
        {
            RestoreDefault();

            if (string.IsNullOrEmpty(ConfigManager.ScanConfig.NFOParseConfig))
                return;

            Dictionary<string, List<string>> list =
                JsonUtils.TryDeserializeObject<Dictionary<string, List<string>>>(ConfigManager.ScanConfig.NFOParseConfig);
            if (list == null || list.Count == 0)
                return;

            // 读取数据
            foreach (var item in list) {
                string key = item.Key;
                List<string> values = item.Value;
                if (!CurrentNFOParse.ContainsKey(key) || values == null || values.Count == 0)
                    continue;

                CurrentNFOParse[key].ParseValues = new ObservableCollection<ObservableString>();
                foreach (var value in values) {
                    CurrentNFOParse[key].ParseValues.Add(new ObservableString(value));
                }
            }

            RebuildAliasIndex();
        }

        private static void RebuildAliasIndex()
        {
            _aliasIndex = new Dictionary<string, (string key, NfoParse parse)>(StringComparer.OrdinalIgnoreCase);
            if (CurrentNFOParse == null)
                return;
            foreach (var item in CurrentNFOParse) {
                NfoParse parse = item.Value;
                if (parse?.ParseValues == null)
                    continue;
                foreach (ObservableString alias in parse.ParseValues) {
                    if (string.IsNullOrEmpty(alias?.Value))
                        continue;
                    _aliasIndex[alias.Value.Trim().ToLower()] = (item.Key, parse);
                }
            }
        }



        /// <summary>
        /// 多对一映射，查找唯一的一个
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static (string, NfoParse) SearchTarget(string target)
        {
            if (string.IsNullOrEmpty(target) || _aliasIndex == null)
                return (null, null);
            if (_aliasIndex.TryGetValue(target.Trim().ToLower(), out (string key, NfoParse parse) found))
                return found;
            return (null, null);
        }

        public static void Parse(ref Movie movie, string name, string value)
        {
            if (string.IsNullOrEmpty(name))
                return;
            name = name.Trim().ToLower();

            (string key, NfoParse nfoParse) = SearchTarget(name);
            if (string.IsNullOrEmpty(key) || nfoParse == null)
                return;

            if (!_moviePropertyCache.TryGetValue(key, out PropertyInfo propertyInfo) || propertyInfo == null)
                return;

            ParseType parseType = nfoParse.ParseType;
            object defaultValue = nfoParse.DefaultValue;
            value = value.Trim();
            switch (parseType) {
                case ParseType.Str:
                    propertyInfo.SetValue(movie, value);
                    break;
                case ParseType.StrUpper:
                    propertyInfo.SetValue(movie, value.ToUpper());
                    break;

                case ParseType.StrLower:
                    propertyInfo.SetValue(movie, value.ToLower());
                    break;

                case ParseType.Int:
                    if (int.TryParse(value, out int valueInt))
                        propertyInfo.SetValue(movie, valueInt);
                    else
                        propertyInfo.SetValue(movie, defaultValue);
                    break;
                case ParseType.Float:
                    if (float.TryParse(value, out float valueFloat))
                        propertyInfo.SetValue(movie, valueFloat);
                    else
                        propertyInfo.SetValue(movie, defaultValue);
                    break;

                default:
                    break;
            }
        }

        public static void RestoreDefault()
        {
            CurrentNFOParse = new Dictionary<string, NfoParse>();

            foreach (var item in DEFAULT_NFO_PARSE) {
                NfoParse origin = item.Value;
                NfoParse nfoParse = new NfoParse(origin);
                CurrentNFOParse.Add(item.Key, nfoParse);
            }

            RebuildAliasIndex();
            Logger.Warn("恢复默认");

        }

        public static Dictionary<string, NfoParse> LoadData()
        {
            InitCurrentNFOParse();
            return CurrentNFOParse;
        }

        public static void SaveData(Dictionary<string, NfoParse> NfoParseRules)
        {
            CurrentNFOParse = NfoParseRules;

            // 构造
            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
            foreach (var item in CurrentNFOParse) {
                data.Add(item.Key, item.Value.ParseValues.Select(arg => arg.Value).ToList());
            }

            ConfigManager.ScanConfig.NFOParseConfig = JsonUtils.TrySerializeObject(data);
            ConfigManager.ScanConfig.Save();
            RebuildAliasIndex();
            Logger.Debug(ConfigManager.ScanConfig.NFOParseConfig);
        }
    }
}
