using Jvedio.Entity;
using System.Collections.Generic;

namespace Jvedio.Core.Media
{
    public interface IVideoRepository
    {
        Video GetById(long dataId);
        List<Video> GetAllByDbId(long dbId);
        void SetAssociation(ref Video video);
    }
}
