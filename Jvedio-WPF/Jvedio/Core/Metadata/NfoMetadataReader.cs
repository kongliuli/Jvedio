using Jvedio.Core.Config;
using Jvedio.Entity;
using Jvedio.Entity.Common;
using System.Collections.Generic;
using System.Xml;

namespace Jvedio.Core.Metadata
{
    /// <summary>本地 NFO 读取统一入口（扫描导入与 <see cref="NfoMetadataProvider"/> 共用）。</summary>
    public static class NfoMetadataReader
    {
        public static Movie TryReadMovie(string path, Dictionary<string, List<string>> dirVideoCache = null)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            XmlDocument doc = new XmlDocument();
            XmlNode rootNode;
            try {
                doc.Load(path);
                rootNode = doc.SelectSingleNode("movie");
            } catch {
                throw;
            }

            if (rootNode == null || rootNode.ChildNodes == null || rootNode.ChildNodes.Count == 0)
                return null;

            Movie movie = new Movie();
            foreach (XmlNode node in rootNode.ChildNodes) {
                if (node == null || string.IsNullOrEmpty(node.Name))
                    continue;
                NfoParse.Parse(ref movie, node.Name, node.InnerText);
            }

            if (string.IsNullOrEmpty(movie.id))
                return null;

            movie.vediotype = JvedioLib.Security.Identify.GetVideoType(movie.id);
            Movie.SetFileSize(path, ref movie, dirVideoCache);

            var xpathCache = new Dictionary<string, XmlNodeList>(System.StringComparer.OrdinalIgnoreCase);
            movie.tag = Movie.GetTagList(doc, "tag", xpathCache);
            movie.genre = Movie.GetTagList(doc, "genre", xpathCache);
            movie.actor = Movie.FindAndJoinData(doc, new List<string> { "actor/name" }, xpathCache: xpathCache);
            movie.actressimageurl = Movie.FindAndJoinData(doc, new List<string> { "actor/thumb" }, RenameConfig.DEFAULT_NULL_STRING, xpathCache);
            movie.extraimageurl = Movie.FindAndJoinData(doc, new List<string> { "fanart/thumb" }, RenameConfig.DEFAULT_NULL_STRING, xpathCache);

            if (movie.isNullMovie())
                return null;
            return movie;
        }
    }
}
