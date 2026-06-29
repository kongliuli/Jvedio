using Jvedio.Entity;
using System.Collections.Generic;

namespace Jvedio.Core.Media
{
    public sealed class MapperVideoRepository : IVideoRepository
    {
        public static MapperVideoRepository Instance { get; } = new MapperVideoRepository();

        public Video GetById(long dataId) => Video.GetById(dataId);

        public List<Video> GetAllByDbId(long dbId) => Video.GetAllByDBID(dbId);

        public void SetAssociation(ref Video video) => Video.SetAsso(ref video);
    }
}
