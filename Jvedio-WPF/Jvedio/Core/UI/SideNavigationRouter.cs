using Jvedio.ViewModel;

namespace Jvedio.Core.UI
{
    public static class SideNavigationRouter
    {
        public static void Handle(VieModel_Main vm, object command)
        {
            if (vm == null || command == null)
                return;
            MediaUIHost.GetProfile(vm.CurrentLibraryType).CreateSideNavigation().Handle(vm, command);
        }
    }
}
