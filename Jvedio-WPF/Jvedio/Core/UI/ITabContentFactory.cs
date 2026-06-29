using Jvedio.Core.Enums;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using SuperUtils.Framework.ORM.Wrapper;

namespace Jvedio.Core.UI
{
    public interface ITabContentFactory
    {
        IMediaListTab CreateListTab(TabItemEx tabItem, SelectWrapper<Video> wrapper, MediaListMode listMode);
    }

    internal sealed class ProfileTabContentFactory : ITabContentFactory
    {
        private readonly DataType _dataType;

        public ProfileTabContentFactory(DataType dataType)
        {
            _dataType = dataType;
        }

        public IMediaListTab CreateListTab(TabItemEx tabItem, SelectWrapper<Video> wrapper, MediaListMode listMode)
        {
            var list = new Core.UserControls.VideoList(wrapper, tabItem);
            if (listMode == default)
                listMode = MediaListModeExtensions.FromDataType(_dataType);
            list.ListMode = listMode;
            list.ApplyListModeColumns();
            return list;
        }
    }
}
