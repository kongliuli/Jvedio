using Jvedio.Entity;

namespace Jvedio.Core.Metadata
{
    public interface IMetadataWriter
    {
        void WriteVideo(Video video, string nfoPath);
    }

    public sealed class NfoMetadataWriterAdapter : IMetadataWriter
    {
        public static IMetadataWriter Default { get; } = new NfoMetadataWriterAdapter();

        public void WriteVideo(Video video, string nfoPath) => NfoMetadataWriter.WriteVideo(video, nfoPath);
    }
}
