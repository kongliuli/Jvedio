using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Jvedio.Entity;
using SuperControls.Style;
using SuperUtils.Framework.ORM.Wrapper;
using System.Windows;
using System.Windows.Controls;
using static Jvedio.App;
using static Jvedio.MapperManager;

namespace Jvedio.Core.UI
{
    public static class MediaDetailsFactory
    {
        public static Window CreateDetailsWindow(long dataId, WrapperEventArg<Video> wrapperArg, DataType? dataType = null)
        {
            DataType libraryType = dataType ?? LibraryContext.Current.DataType;
            return MediaUIHost.GetProfile(libraryType).CreateDetailsWindow(dataId, wrapperArg);
        }
    }

    public sealed class Window_MediaDetailsReadOnly : SuperControls.Style.BaseWindow
    {
        public Window_MediaDetailsReadOnly(long dataId, DataType dataType)
        {
            Title = $"{LangManager.GetValueByKey("Detail")} [{DataTypeDisplay.GetLabel(dataType)}]";
            Width = 520;
            Height = 360;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var wrapper = new SelectWrapper<MetaData>();
            wrapper.Eq("DataID", dataId);
            MetaData meta = metaDataMapper.SelectById(wrapper);
            if (meta == null) {
                Content = new TextBlock { Text = "未找到条目", Margin = new Thickness(20) };
                return;
            }

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(MakeRow("Title", meta.Title));
            panel.Children.Add(MakeRow("Path", meta.Path));
            panel.Children.Add(MakeRow("Size", meta.Size.ToString()));
            panel.Children.Add(MakeRow("Grade", meta.Grade.ToString()));
            panel.Children.Add(MakeRow("ViewDate", meta.ViewDate));
            panel.Children.Add(MakeRow("LastScanDate", meta.LastScanDate));
            Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }

        private static TextBlock MakeRow(string label, string value)
        {
            return new TextBlock {
                Text = $"{label}: {value ?? string.Empty}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8),
            };
        }
    }
}
