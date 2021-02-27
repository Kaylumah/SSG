using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Kaylumah.Ssg.Manager.Site.Service;
using Kaylumah.Ssg.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Test.Unit
{
    public interface IFileMetadataParser
    {
        Metadata<FileMetaData> Parse(MetadataCriteria criteria);
    }

    public class MetadataCriteria
    {
        public string Root { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class FileMetadataParser : IFileMetadataParser
    {
        private readonly ILogger _logger;
        private readonly MetadataUtil _metadataUtil;
        private Dictionary<string, FileMetaData> _defaults;
        public FileMetadataParser(ILogger<FileMetadataParser> logger)
        {
            _logger = logger;
            _metadataUtil = new MetadataUtil();
            _defaults = new Dictionary<string, FileMetaData>
            {
                [Path.DirectorySeparatorChar.ToString()] = new FileMetaData()
                {
                    Layout = "default.html"
                }
            };
        }

        public Metadata<FileMetaData> Parse(MetadataCriteria criteria)
        {
            var result = _metadataUtil.Retrieve<FileMetaData>(criteria.Content);

            var paths = new List<string>();
            var input = criteria.FilePath.Replace($"{Path.DirectorySeparatorChar}{criteria.FileName}", string.Empty);
            Recursive(criteria.Root, input, paths);
            var x = paths.OrderBy(x => x.Length).ToList();
            x.Insert(0, Path.DirectorySeparatorChar.ToString());

            var fileMetaData = new FileMetaData();
            foreach (var path in x)
            {
                if (_defaults.ContainsKey(path))
                {
                    Merge(fileMetaData, _defaults[path], $"default:{path}");
                }
            }

            Merge(fileMetaData, result.Data, "file");
            result.Data = fileMetaData;
            return result;
        }

        private void Recursive(string root, string input, List<string> paths)
        {
            var current = input.Replace(root, "");
            if (!current.Equals(string.Empty))
            {
                paths.Add(current);
                var index = current.LastIndexOf(Path.DirectorySeparatorChar);
                if (index > 0)
                {
                    Recursive(root, current.Substring(0, index), paths);
                }
            }
        }

        private void Merge(FileMetaData target, FileMetaData source, string reason)
        {
            if (target == null)
            {
                target = new FileMetaData();
            }

            if (source != null)
            {
                foreach (var entry in source)
                {
                    if (target.ContainsKey(entry.Key))
                    {
                        // TODO log that its overwritten...
                    }
                    target[entry.Key] = entry.Value;
                }
            }
        }

        public class MetadataTests
        {
            [Fact]
            public void Test_FileMetadataParser_EmptyInput()
            {
                var root = "/a/b/c";
                var fileName = "1.txt";
                var filePath = Path.Combine(root, "b/d/e/f", fileName);
                var criteria = new MetadataCriteria
                {
                    FileName = fileName,
                    FilePath = filePath,
                    Root = root,
                    Content = string.Empty
                };

                var loggerMock = new Mock<ILogger<FileMetadataParser>>();
                IFileMetadataParser sut = new FileMetadataParser(loggerMock.Object);
                var result = sut.Parse(criteria);
                result.Should().NotBeNull();
                result.Data.Should().NotBeNull();
            }

            [Fact]
            public void Test_FileMetadataParser_EmptyInput2()
            {
                var root = "/a/b/c";
                var fileName = "1.txt";
                var filePath = Path.Combine(root, fileName);
                var criteria = new MetadataCriteria
                {
                    FileName = fileName,
                    FilePath = filePath,
                    Root = root,
                    Content = string.Empty
                };

                var loggerMock = new Mock<ILogger<FileMetadataParser>>();
                IFileMetadataParser sut = new FileMetadataParser(loggerMock.Object);
                var result = sut.Parse(criteria);
                result.Should().NotBeNull();
                result.Data.Should().NotBeNull();
            }
        }
    }
}