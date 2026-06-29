using Jvedio.Entity;

namespace Jvedio.Core.Metadata
{
    public interface IMetadataReader
    {
        Movie TryReadMovie(string nfoPath);
    }

    public sealed class NfoMetadataReaderAdapter : IMetadataReader
    {
        public static IMetadataReader Default { get; } = new NfoMetadataReaderAdapter();

        public Movie TryReadMovie(string nfoPath) => NfoMetadataReader.TryReadMovie(nfoPath);
    }
}
