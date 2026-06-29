using HtmlAgilityPack;
using SuperUtils.Common;
using SuperUtils.Framework.Tasks;
using SuperUtils.NetWork;
using SuperUtils.NetWork.Entity;
using SuperUtils.NetWork.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Jvedio.Core.Metadata
{
    internal static class WebMetadataFetcher
    {
        public static async Task<Dictionary<string, object>> FetchAsync(
            string webUrl,
            RequestHeader header,
            TaskLogger logger,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(webUrl))
                return null;

            try {
                cancellationToken.ThrowIfCancellationRequested();
                HttpResult httpResult = await HttpClient.Get(webUrl.Trim(), header, HttpMode.String)
                    .ConfigureAwait(false);
                if (httpResult == null
                    || httpResult.StatusCode != HttpStatusCode.OK
                    || string.IsNullOrEmpty(httpResult.SourceCode))
                    return null;

                string body = httpResult.SourceCode.Trim();
                if (body.StartsWith("{") || body.StartsWith("["))
                    return ParseJson(body);

                return ParseHtml(webUrl, body);
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                logger?.Info($"WebMetadataFetcher: {ex.Message}");
                return null;
            }
        }

        private static Dictionary<string, object> ParseJson(string body)
        {
            Dictionary<string, object> dict = JsonUtils.TryDeserializeObject<Dictionary<string, object>>(body);
            return dict != null && dict.Count > 0 ? dict : null;
        }

        private static Dictionary<string, object> ParseHtml(string webUrl, string html)
        {
            HttpMetadataSiteRule rule = HttpMetadataSiteRegistry.Match(webUrl);
            if (rule != null) {
                Dictionary<string, object> siteFields = HttpMetadataSiteRegistry.ParseHtml(html, rule);
                if (siteFields != null && siteFields.Count > 0)
                    return siteFields;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode == null || string.IsNullOrWhiteSpace(titleNode.InnerText))
                return null;

            return new Dictionary<string, object> {
                ["Title"] = titleNode.InnerText.Trim(),
            };
        }
    }
}
