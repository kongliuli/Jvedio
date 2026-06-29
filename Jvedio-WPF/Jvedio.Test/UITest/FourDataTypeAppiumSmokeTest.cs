using Jvedio.Core.Enums;
using Jvedio.Core.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium.Appium.Windows;
using System.Threading;

namespace Jvedio.Test.UITest
{
    /// <summary>
    /// 四库类型 WinAppDriver 冒烟（需本机安装并运行 Windows Application Driver）。
    /// 默认 Ignore；本地验证：管理员启动 WinAppDriver.exe 后去掉 Ignore 再跑。
    /// </summary>
    [TestClass]
    [Ignore("UI-013: 需要 WinAppDriver + 交互式桌面，CI 不跑")]
    [TestCategory("Appium")]
    public class FourDataTypeAppiumSmokeTest : TestBase
    {
        private const string BUTTON_NEW_DATA_BASE = "newDataBaseButton";

        [TestInitialize]
        public void TestInitialize() => Initialize();

        [TestCleanup]
        public void TestCleanup() => Cleanup();

        [ClassCleanup]
        public static void ClassCleanup() => StopWinappDriver();

        [TestMethod]
        [DataRow(0, DataType.Video)]
        [DataRow(1, DataType.Picture)]
        public void StartupSideIndex_OpensLibrary(int sideIndex, DataType expectedType)
        {
            DismissFirstRunDialogIfPresent();
            Assert.AreEqual(expectedType, StartupLibraryMapping.DataTypeFromSideIndex(sideIndex));
            // ponytail: 侧栏 AutomationId 未统一，完整点击流待补 x:Name 后再接
            Assert.IsNotNull(FindById(BUTTON_NEW_DATA_BASE));
        }

        private void DismissFirstRunDialogIfPresent()
        {
            WindowsElement firstRun = FindById(WINDOW_SKIN_LANG);
            if (firstRun == null)
                return;
            ClickByXPath("/Window[@AutomationId=\"SkinLangWindow\"]/RadioButton[@ClassName=\"RadioButton\"]");
            Thread.Sleep(SLEEP_SLOW);
            ClickByXPath("/Window[@AutomationId=\"SkinLangWindow\"]/Button[@ClassName=\"Button\"]");
            Thread.Sleep(SLEEP_SLOW);
        }
    }
}
