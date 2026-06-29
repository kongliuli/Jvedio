using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.Enums;
using Jvedio.Core.UserControls;
using Jvedio.Entity;
using Jvedio.ViewModel;
using SuperUtils.Framework.ORM.Wrapper;
using System.Windows;
using System.Windows.Controls;

namespace Jvedio.Core.UI
{
    public interface IMediaUIProfile
    {
        DataType DataType { get; }
        SettingsSectionMask SettingsSections { get; }
        ScanCompletionPolicy ScanCompletion { get; }
        ISideNavigation CreateSideNavigation();
        ITabContentFactory CreateTabFactory();
        Window CreateDetailsWindow(long dataId, WrapperEventArg<Video> wrapperArg);
        bool UseVideoDetailsWindow { get; }
        void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel);
    }

    internal abstract class MediaUIProfileBase : IMediaUIProfile
    {
        private ITabContentFactory _tabFactory;

        public abstract DataType DataType { get; }
        public SettingsSectionMask SettingsSections => SettingsSectionMaskExtensions.ForDataType(DataType);
        public ScanCompletionPolicy ScanCompletion => ScanCompletionPolicyExtensions.ForDataType(DataType);
        public virtual ISideNavigation CreateSideNavigation() => SideNavigations.ForDataType(DataType);

        public virtual ITabContentFactory CreateTabFactory()
            => _tabFactory ?? (_tabFactory = new ProfileTabContentFactory(DataType));

        public virtual Window CreateDetailsWindow(long dataId, WrapperEventArg<Video> wrapperArg)
            => new Window_MediaDetailsReadOnly(dataId, DataType);

        public virtual bool UseVideoDetailsWindow => DataType == DataType.Video;
        public abstract void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel);
    }

    internal sealed class VideoUIProfile : MediaUIProfileBase
    {
        public override DataType DataType => DataType.Video;

        public override Window CreateDetailsWindow(long dataId, WrapperEventArg<Video> wrapperArg)
            => new Window_Details(dataId, wrapperArg);

        public override void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel)
        {
            if (sideMenuContainer == null || vieModel == null)
                return;
            VideoSideMenu sideMenu = sideMenuContainer.Child as VideoSideMenu;
            if (sideMenu == null)
                return;
            sideMenu.onSideButtonCmd = vieModel.HandleSideButtonCmd;
            vieModel.BindSideMenu(sideMenu);
        }
    }

    internal sealed class PictureUIProfile : MediaUIProfileBase
    {
        public override DataType DataType => DataType.Picture;

        public override void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel)
        {
            if (sideMenuContainer == null || vieModel == null)
                return;
            PictureSideMenu pictureSideMenu = new PictureSideMenu();
            pictureSideMenu.onSideButtonCmd = vieModel.HandleSideButtonCmd;
            sideMenuContainer.Child = pictureSideMenu;
            vieModel.BindSideMenu(pictureSideMenu);
        }
    }

    internal sealed class GameUIProfile : MediaUIProfileBase
    {
        public override DataType DataType => DataType.Game;

        public override void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel)
        {
            if (sideMenuContainer == null || vieModel == null)
                return;
            GameSideMenu gameSideMenu = new GameSideMenu();
            gameSideMenu.onSideButtonCmd = vieModel.HandleSideButtonCmd;
            sideMenuContainer.Child = gameSideMenu;
            vieModel.BindSideMenu(gameSideMenu);
        }
    }

    internal sealed class ComicUIProfile : MediaUIProfileBase
    {
        public override DataType DataType => DataType.Comics;

        public override void InitSideMenu(Border sideMenuContainer, VieModel_Main vieModel)
        {
            new PictureUIProfile().InitSideMenu(sideMenuContainer, vieModel);
        }
    }

    public static class MediaUIHost
    {
        private static readonly System.Collections.Generic.Dictionary<DataType, IMediaUIProfile> Registry =
            new System.Collections.Generic.Dictionary<DataType, IMediaUIProfile>();

        static MediaUIHost()
        {
            Register(DataType.Video, new VideoUIProfile());
            Register(DataType.Picture, new PictureUIProfile());
            Register(DataType.Game, new GameUIProfile());
            Register(DataType.Comics, new ComicUIProfile());
        }

        public static void Register(DataType dataType, IMediaUIProfile profile)
        {
            Registry[dataType] = profile;
        }

        public static IMediaUIProfile GetProfile(DataType dataType)
        {
            if (Registry.TryGetValue(dataType, out IMediaUIProfile profile))
                return profile;
            return Registry[DataType.Video];
        }

        public static void InitSideMenu(DataType dataType, Border sideMenuContainer, VieModel_Main vieModel)
        {
            if (dataType != DataType.Video && sideMenuContainer != null)
                sideMenuContainer.Child = null;
            GetProfile(dataType).InitSideMenu(sideMenuContainer, vieModel);
        }

        public static SettingsSectionMask GetSettingsSections(DataType dataType)
        {
            return GetProfile(dataType).SettingsSections;
        }

        public static ScanCompletionPolicy GetScanCompletionPolicy(DataType dataType)
        {
            return GetProfile(dataType).ScanCompletion;
        }
    }
}
