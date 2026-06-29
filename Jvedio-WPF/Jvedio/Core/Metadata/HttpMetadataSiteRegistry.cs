using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jvedio.Core.Metadata
{
    public static class HttpMetadataSiteRegistry
    {
        private static readonly HttpMetadataSiteRule[] BuiltInRules = {
            new HttpMetadataSiteRule {
                HostContains = "vndb.org",
                OgPropertyMap = {
                    ["og:title"] = "Title",
                    ["og:description"] = "Plot",
                },
            },
            new HttpMetadataSiteRule {
                HostContains = "dlsite.com",
                OgPropertyMap = {
                    ["og:title"] = "Title",
                    ["og:description"] = "Plot",
                },
                MetaNameMap = {
                    ["keywords"] = "Genre",
                },
            },
            new HttpMetadataSiteRule {
                HostContains = "fanza",
                OgPropertyMap = {
                    ["og:title"] = "Title",
                    ["og:description"] = "Plot",
                },
            },
        };

        public static HttpMetadataSiteRule Match(string webUrl, IEnumerable<HttpMetadataSiteRule> userRules = null)
        {
            if (string.IsNullOrWhiteSpace(webUrl))
                return null;
            if (!Uri.TryCreate(webUrl.Trim(), UriKind.Absolute, out Uri uri))
                return null;

            string host = uri.Host ?? string.Empty;
            IEnumerable<HttpMetadataSiteRule> rules = userRules;
            if (rules == null || !rules.Any()) {
                if (HttpMetadataOptions.EnableSiteRules
                    && HttpMetadataOptions.SiteRules != null
                    && HttpMetadataOptions.SiteRules.Count > 0) {
                    rules = HttpMetadataOptions.SiteRules;
                } else {
                    rules = BuiltInRules;
                }
            }

            foreach (HttpMetadataSiteRule rule in rules) {
                if (string.IsNullOrEmpty(rule?.HostContains))
                    continue;
                if (host.IndexOf(rule.HostContains, StringComparison.OrdinalIgnoreCase) >= 0)
                    return rule;
            }

            return null;
        }

        public static Dictionary<string, object> ParseHtml(string html, HttpMetadataSiteRule rule)
        {
            if (string.IsNullOrWhiteSpace(html) || rule == null)
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (rule.OgPropertyMap != null) {
                foreach (KeyValuePair<string, string> map in rule.OgPropertyMap) {
                    string value = ReadOgContent(doc, map.Key);
                    AddField(fields, map.Value, value);
                }
            }

            if (rule.MetaNameMap != null) {
                foreach (KeyValuePair<string, string> map in rule.MetaNameMap) {
                    string value = ReadMetaName(doc, map.Key);
                    AddField(fields, map.Value, value);
                }
            }

            if (fields.Count == 0) {
                string title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim();
                AddField(fields, "Title", title);
            }

            return fields.Count > 0 ? fields : null;
        }

        private static string ReadOgContent(HtmlDocument doc, string property)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode(
                $"//meta[@property='{property}']");
            return node?.GetAttributeValue("content", string.Empty)?.Trim();
        }

        private static string ReadMetaName(HtmlDocument doc, string name)
        {
            HtmlNode node = doc.DocumentNode.SelectSingleNode(
                $"//meta[@name='{name}']");
            return node?.GetAttributeValue("content", string.Empty)?.Trim();
        }

        private static void AddField(Dictionary<string, object> fields, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                return;
            if (!fields.ContainsKey(key) || string.IsNullOrWhiteSpace(fields[key]?.ToString()))
                fields[key] = value.Trim();
        }
    }
}
