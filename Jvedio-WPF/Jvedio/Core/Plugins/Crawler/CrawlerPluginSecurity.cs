using Jvedio.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using static Jvedio.App;

namespace Jvedio.Core.Plugins.Crawler
{
    internal static class CrawlerPluginSecurity
    {
        private static readonly HashSet<string> AllowedTypeNameSuffixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Crawler",
            "Plugin",
            "Provider",
        };

        internal static bool IsAllowedPublicType(Type type)
        {
            if (type == null || !type.IsPublic || type.IsAbstract)
                return false;

            string name = type.Name;
            return AllowedTypeNameSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        }

        internal static bool VerifyDll(string dllPath)
        {
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
                return false;

            if (!ConfigManager.PluginConfig.RequireSignedCrawlerDll)
                return true;

            return HasAuthenticodeSignature(dllPath);
        }

        internal static bool HasAuthenticodeSignature(string dllPath)
        {
            try {
                X509Certificate cert = X509Certificate.CreateFromSignedFile(dllPath);
                if (cert == null)
                    return false;

                if (cert is X509Certificate2 cert2) {
                    using (var chain = new X509Chain()) {
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                        if (!chain.Build(cert2)) {
                            Logger.Warn($"crawler dll cert chain failed: {dllPath}");
                            return false;
                        }
                    }
                }

                Logger.Info($"crawler dll signed: {dllPath}, subject={cert.Subject}");
                return true;
            } catch (Exception ex) {
                Logger.Warn($"crawler dll signature check failed: {dllPath}, {ex.Message}");
                return false;
            }
        }
    }
}
