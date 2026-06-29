using Jvedio.Core.Config;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Values;
using SuperUtils.WPF.Entity;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Jvedio.Core.Rename
{
    public static class RenameEngine
    {
        public static string[] BuildTargetPaths(Video video)
        {
            if (video == null)
                return Array.Empty<string>();

            bool addTag = ConfigManager.RenameConfig.AddRenameTag;
            string formatString = ConfigManager.RenameConfig.FormatString;
            FileInfo fileInfo = new FileInfo(video.Path);
            string dir = fileInfo.Directory.FullName;
            string ext = fileInfo.Extension;
            string newName = string.Empty;
            MatchCollection matches = Regex.Matches(formatString, "\\{[a-zA-Z]+\\}");
            PropertyInfo[] propertyList = typeof(Video).GetProperties();

            if (matches != null && matches.Count > 0) {
                newName = formatString;
                foreach (Match match in matches) {
                    string property = match.Value.Replace("{", string.Empty).Replace("}", string.Empty);
                    ReplaceWithValue(video, ref newName, property, propertyList);
                }
            }

            foreach (char item in Path.GetInvalidFileNameChars())
                newName = newName.Replace(item.ToString(), string.Empty);

            if (ConfigManager.RenameConfig.RemoveTitleSpace)
                newName = newName.Trim();

            if (video.HasSubSection) {
                ObservableCollection<ObservableString> sections = video.SubSectionList;
                string[] result = new string[sections.Count];
                for (int i = 0; i < sections.Count; i++) {
                    if (addTag && JvedioLib.Security.Identify.IsCHS(video.Path))
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}_{LangManager.GetValueByKey("Translated")}{ext}");
                    else
                        result[i] = Path.Combine(dir, $"{newName}-{i + 1}{ext}");
                }
                return result;
            }

            if (addTag && JvedioLib.Security.Identify.IsCHS(video.Path))
                return new string[] { Path.Combine(dir, $"{newName}_{LangManager.GetValueByKey("Translated")}{ext}") };
            return new string[] { Path.Combine(dir, $"{newName}{ext}") };
        }

        private static void ReplaceWithValue(Video video, ref string result, string property, PropertyInfo[] propertyList)
        {
            string inSplit = ConfigManager.RenameConfig.InSplit.Equals("[null]") ? string.Empty : ConfigManager.RenameConfig.InSplit;
            foreach (PropertyInfo item in propertyList) {
                if (item.Name != property)
                    continue;

                object o = item.GetValue(video);
                if (o != null) {
                    string value = o.ToString();
                    if (property == "ActorNames" || property == "Genre" || property == "Label")
                        value = value.Replace(ConstValues.SeparatorString, inSplit);

                    if (property == "VideoType") {
                        int v = 0;
                        int.TryParse(value, out v);
                        if (v == 1)
                            value = LangManager.GetValueByKey("Uncensored");
                        else if (v == 2)
                            value = LangManager.GetValueByKey("Censored");
                        else if (v == 3)
                            value = LangManager.GetValueByKey("Europe");
                    }

                    if (string.IsNullOrEmpty(value)) {
                        int idx = result.IndexOf("{" + property + "}", StringComparison.Ordinal);
                        if (idx >= 1)
                            result = result.Remove(idx - 1, 1);
                        result = result.Replace("{" + property + "}", string.Empty);
                    } else {
                        result = result.Replace("{" + property + "}", value);
                    }
                } else {
                    int idx = result.IndexOf("{" + property + "}", StringComparison.Ordinal);
                    if (idx >= 1)
                        result = result.Remove(idx - 1, 1);
                    result = result.Replace("{" + property + "}", string.Empty);
                }
                break;
            }
        }
    }
}
