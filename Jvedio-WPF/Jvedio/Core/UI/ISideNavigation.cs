using Jvedio.ViewModel;

namespace Jvedio.Core.UI
{
    /// <summary>侧栏按钮 → Tab 打开策略（由 SideNavigationRouter 实现）。</summary>
    public interface ISideNavigation
    {
        void Handle(VieModel_Main vm, object command);
    }

    public sealed class SideNavigationRouterAdapter : ISideNavigation
    {
        public void Handle(VieModel_Main vm, object command) => SideNavigationRouter.Handle(vm, command);
    }
}
