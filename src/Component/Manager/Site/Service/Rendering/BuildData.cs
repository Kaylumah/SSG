using System;
using Kaylumah.Ssg.Utilities;

namespace Kaylumah.Ssg.Manager.Site.Service
{
    class BuildData
    {
        public string Version { get;set; }
        public string Copyright { get;set; }
        public string SourceBaseUri { get;set; }
        public string GitHash { get;set; }
        public string ShortGitHash { get;set; }
        public DateTimeOffset Time { get; set; }

        public BuildData(AssemblyInfo info)
        {
            var version = "1.0.0+LOCALBUILD";
            if (info.Version.Length > 6)
            {
                version = info.Version;
            }
            var appVersion = version.Substring(0, version.IndexOf('+'));
            var gitHash = version[(version.IndexOf('+') + 1)..]; // version.Substring(version.IndexOf('+') + 1);
            var shortGitHash = gitHash.Substring(0, 7);
            var repositoryType = info.Metadata["RepositoryType"];
            var repositoryUrl = info.Metadata["RepositoryUrl"];

            var index = repositoryUrl.LastIndexOf($".{repositoryType}");
            var sourceBaseUrl = repositoryUrl.Remove(index, repositoryType.Length + 1).Insert(index, "/commit");
            
            Time = DateTimeOffset.Now;
            Version = appVersion;
            Copyright = info.Copyright;
            GitHash = gitHash;
            ShortGitHash = shortGitHash;
            SourceBaseUri = sourceBaseUrl;
        }
    }
}