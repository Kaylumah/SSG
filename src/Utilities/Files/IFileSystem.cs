using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Kaylumah.Ssg.Utilities
{
    public class File<TMetadata>
    {
        public string Encoding { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public TMetadata Data { get; set; }
    }

    public interface IFileSystem
    {
        IEnumerable<IFileInfo> GetFiles(string path, bool recursive = false);
        IFileInfo GetFile(string path);
        Task<File<TData>> GetFile<TData>(string path);
        IDirectoryContents GetDirectoryContents(string path);
    }

    public class FileSystem : IFileSystem
    {
        private readonly IFileProvider _fileProvider;

        public FileSystem(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public IDirectoryContents GetDirectoryContents(string path)
        {
            return _fileProvider.GetDirectoryContents(path);
        }

        public IFileInfo GetFile(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }

        public async Task<File<TData>> GetFile<TData>(string path)
        {
            var fileInfo = GetFile(path);
            var encoding = new EncodingUtil().DetermineEncoding(fileInfo.CreateReadStream());
            var fileName = fileInfo.Name;
            using var streamReader = new StreamReader(fileInfo.CreateReadStream());
            var text = await streamReader.ReadToEndAsync();
            var metadata = new MetadataUtil().Retrieve<TData>(text);
            return new File<TData>
            {
                Encoding = encoding.WebName,
                Name = fileName,
                Path = path,
                Content = metadata.Content,
                Data = metadata.Data
            };
        }

        public IEnumerable<IFileInfo> GetFiles(string path, bool recursive = false)
        {
            var result = new List<IFileInfo>();
            var directoryContents = _fileProvider.GetDirectoryContents(path);
            result.AddRange(directoryContents.Where(x => !x.IsDirectory));

            if (recursive)
            {
                var directories = directoryContents.Where(x => x.IsDirectory);
                foreach(var directory in directories)
                {
                    result.AddRange(GetFiles(Path.Combine(path, directory.Name), recursive));
                }
            }
            return result;
        }
    }
}