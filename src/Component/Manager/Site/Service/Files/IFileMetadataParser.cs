using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kaylumah.Ssg.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kaylumah.Ssg.Manager.Site.Service
{
    public interface IFileMetadataParser
    {
        Metadata<FileMetaData> Parse(MetadataCriteria criteria);
    }

    public class MetadataCriteria
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string Permalink { get;set; }

        public MetadataCriteria()
        {
            // TODO
            Permalink = "/:year/:month/:day/:name:ext";
        }
    }

    public class DefaultMetadatas : KeyedCollection<string, DefaultMetadata>
    {
        protected override string GetKeyForItem(DefaultMetadata item)
        {
            return item.Path;
        }
    }

    public class DefaultMetadata
    {
        public string Path { get;set; }
        public FileMetaData Values { get;set; }

    }

    public class MetadataParserOptions
    {
        public const string Options = "Metadata";
        public DefaultMetadatas Defaults { get;set; } = new DefaultMetadatas();
    }

    public class FileMetadataParser : IFileMetadataParser
    {
        private readonly ILogger _logger;
        private readonly MetadataUtil _metadataUtil;
        private readonly DefaultMetadatas _defaults;
        public FileMetadataParser(ILogger<FileMetadataParser> logger, IOptions<MetadataParserOptions> options)
        {
            _logger = logger;
            _metadataUtil = new MetadataUtil();
            _defaults = options.Value.Defaults;
        }

        public Metadata<FileMetaData> Parse(MetadataCriteria criteria)
        {
            var result = _metadataUtil.Retrieve<FileMetaData>(criteria.Content);
            var outputLocation = DetermineOutputLocation(criteria.FileName, criteria.Permalink);

            var paths = new List<string>() { string.Empty };
            var index = outputLocation.LastIndexOf(Path.DirectorySeparatorChar);
            if (index >= 0)
            {
                var input = outputLocation.Substring(0, index);
                paths.AddRange(DetermineFilters(input));
                paths = paths.OrderBy(x => x.Length).ToList();
            }

            var fileMetaData = new FileMetaData();
            foreach (var path in paths)
            {
                var meta = _defaults.SingleOrDefault(x => x.Path.Equals(path));
                if (meta != null)
                {
                    Merge(fileMetaData, meta.Values, $"default:{path}");
                }
            }

            Merge(fileMetaData, result.Data, "file");

            result.Data = fileMetaData;
            result.Data.Uri = outputLocation;
            return result;
        }

        private List<string> DetermineFilters(string input)
        {
            var result = new List<string>();
            var index = -1;
            while((index = input.LastIndexOf(Path.DirectorySeparatorChar)) >= 0)
            {
                result.Add(input);
                input = input.Substring(0, index);
            }

            if (!string.IsNullOrEmpty(input))
            {
                result.Add(input);
            }

            // var current = input.Replace(root, "");
            // if (!current.Equals(string.Empty))
            // {
            //     paths.Add(current);
            //     var index = current.LastIndexOf(Path.DirectorySeparatorChar);
            //     if (index > 0)
            //     {
            //         Recursive(root, current.Substring(0, index), paths);
            //     }
            // }
            return result;
        }

        private string DetermineOutputLocation(string fileName, string permalink)
        {
            var pattern = @"((?<year>\d{4})\-(?<month>\d{2})\-(?<day>\d{2})\-)?(?<filename>[\s\S]*?)\.(?<ext>.*)";
            var match = Regex.Match(fileName, pattern);

            var outputFileName = match.FileNameByPattern();
            var fileDate = match.DateByPattern();
            // if (fileDate != null)
            // {
            //     metaData["date"] = fileDate;
            // }
            var outputExtension = Path.GetExtension(fileName);//RetrieveExtension(outputFileName);

            var result = permalink
                .Replace("/:year", fileDate == null ? string.Empty : $"/{fileDate?.ToString("yyyy")}")
                .Replace("/:month", fileDate == null ? string.Empty : $"/{fileDate?.ToString("MM")}")
                .Replace("/:day", fileDate == null ? string.Empty : $"/{fileDate?.ToString("dd")}");

            result = result.Replace(":name", Path.GetFileNameWithoutExtension(outputFileName))
                .Replace(":ext", outputExtension);

            if (result.StartsWith("/"))
            {
                result = result[1..];
            }
            return result;
            //metaData.Uri = result;
            //metaData.Remove(nameof(metaData.Permalink).ToLower());
        }

        private void Merge(FileMetaData target, FileMetaData source, string reason)
        {
            if (source != null)
            {
                foreach (var entry in source)
                {
                    if (target.ContainsKey(entry.Key))
                    {
                        _logger.LogInformation($"Overwritting '{entry.Key}' with '{entry.Value}' instead of {target[entry.Key]} because '{reason}'");

                    }
                    target[entry.Key] = entry.Value;
                }
            }
        }

    }
}