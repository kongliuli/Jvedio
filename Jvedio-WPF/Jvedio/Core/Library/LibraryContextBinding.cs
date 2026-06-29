using Jvedio.Core.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jvedio.Core.Library
{
    /// <summary>XAML 可绑定库类型（CTX-001 W13）；<see cref="LibraryContext"/> 变更时通知 UI。</summary>
    public sealed class LibraryContextBinding : INotifyPropertyChanged
    {
        public static LibraryContextBinding Instance { get; } = new LibraryContextBinding();

        static LibraryContextBinding()
        {
            LibraryContext.CurrentChanged += (s, e) => Instance.NotifyCurrentDataType();
        }

        public DataType CurrentDataType => LibraryContext.Current.DataType;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyCurrentDataType()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDataType)));
        }
    }
}
