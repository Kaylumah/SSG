﻿// Copyright (c) Kaylumah, 2021. All rights reserved.
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kaylumah.Ssg.Access.Artifact.Interface;
using Kaylumah.Ssg.Manager.Site.Interface;
using Kaylumah.Ssg.Utilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssg.Extensions.Data.Yaml;
using Kaylumah.Ssg.Manager.Site.Service.Files.Processor;

namespace Kaylumah.Ssg.Manager.Site.Service
{
    public class SiteManager : ISiteManager
    {
        private readonly IArtifactAccess _artifactAccess;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly IFileProcessor _fileProcessor;
        private readonly IYamlParser _yamlParser;
        private readonly SiteInfo _siteInfo;

        private readonly LiquidUtil _liquidUtil;

        public SiteManager(
            IFileProcessor fileProcessor,
            IArtifactAccess artifactAccess,
            IFileSystem fileSystem,
            IYamlParser yamlParser,
            ILogger<SiteManager> logger,
            IOptions<SiteInfo> options,
            LiquidUtil liquidUtil)
        {
            _fileProcessor = fileProcessor;
            _artifactAccess = artifactAccess;
            _fileSystem = fileSystem;
            _yamlParser = yamlParser;
            _logger = logger;
            _siteInfo = options.Value;
            _liquidUtil = liquidUtil;
        }

        private void EnrichSiteWithData(SiteData site, string dataDirectory)
        {
            var extensions = _siteInfo.SupportedDataFileExtensions.ToArray();
            var dataFiles = _fileSystem.GetFiles(dataDirectory)
                .Where(file => extensions.Contains(Path.GetExtension(file.Name)))
                .ToList();
            var data = new Dictionary<string, object>();
            foreach (var file in dataFiles)
            {
                var stream = file.CreateReadStream();
                using var reader = new StreamReader(stream);
                var raw = reader.ReadToEnd();
                var result = _yamlParser.Parse<object>(raw);
                data[Path.GetFileNameWithoutExtension(file.Name)] = result;
            }
            site.Data = data;
        }

        private void EnrichSiteWithTags(SiteData site, List<Files.Processor.File> files)
        {
            var tags = files
                .Where(x => x.MetaData.Tags != null)
                .SelectMany(x => x.MetaData.Tags)
                .Distinct();
            foreach (var tag in tags)
            {
                var tagFiles = files.Where(x => x.MetaData.Tags != null && x.MetaData.Tags.Contains(tag)).ToList();
                site.Tags.Add(tag, tagFiles);
            }
        }

        private void EnrichSiteWithCollections(SiteData site, Guid siteGuid, List<Files.Processor.File> files)
        {
            var collections = files
                .Where(x => x.MetaData.Collection != null)
                .Select(x => x.MetaData.Collection)
                .Distinct()
                .ToList();

            for (var i = collections.Count - 1; i > 0; i--)
            {
                var collection = collections[i];
                if (_siteInfo.Collections.Contains(collection))
                {
                    var collectionSettings = _siteInfo.Collections[collection];
                    if (!string.IsNullOrEmpty(collectionSettings.TreatAs))
                    {
                        if (_siteInfo.Collections.Contains(collectionSettings.TreatAs))
                        {
                            // todo log
                            var collectionFiles = files
                                .Where(x => x.MetaData.Collection != null && x.MetaData.Collection.Equals(collection));
                            foreach (var file in collectionFiles)
                            {
                                file.MetaData.Collection = collectionSettings.TreatAs;
                            }
                            collections.RemoveAt(i);
                        }
                    }
                }
            }

            foreach (var collection in collections)
            {
                site.Collections.Add(collection,
                    files
                    .Where(x => x.MetaData.Collection != null
                        && x.MetaData.Collection.Equals(collection))
                    .Select(x => new PageData(x)
                    {
                        Id = siteGuid.CreatePageGuid(x.MetaData.Uri).ToString()
                    })
                    .ToList()
                );
            }
        }

        public async Task GenerateSite(GenerateSiteRequest request)
        {
            GlobalFunctions.Instance.Url = _siteInfo.Url;
            GlobalFunctions.Instance.BaseUrl = _siteInfo.BaseUrl;

            var processed = await _fileProcessor.Process(new FileFilterCriteria
            {
                DirectoriesToSkip = new string[] {
                    request.Configuration.LayoutDirectory,
                    request.Configuration.PartialsDirectory,
                    request.Configuration.DataDirectory,
                    request.Configuration.AssetDirectory
                },
                FileExtensionsToTarget = _siteInfo.SupportedFileExtensions.ToArray()
            });

            var info = new AssemblyUtil().RetrieveAssemblyInfo(Assembly.GetExecutingAssembly());
            _logger.LogInformation(info.Metadata["RepositoryUrl"]);
            var buildInfo = new BuildData(info);
            var siteGuid = _siteInfo.Url.CreateSiteGuid();
            var siteInfo = new SiteData(_siteInfo, processed.ToArray())
            {
                Id = siteGuid.ToString(),
                Data = new Dictionary<string, object>(),
                Tags = new Dictionary<string, object>(),
                Collections = new Dictionary<string, object>()
            };

            EnrichSiteWithData(siteInfo, request.Configuration.DataDirectory);
            EnrichSiteWithCollections(siteInfo, siteGuid, processed.ToList());
            EnrichSiteWithTags(siteInfo, processed.ToList());

            var renderRequests = processed.ToRenderRequests(buildInfo, siteInfo, siteGuid);

            var renderResults = await _liquidUtil.Render(renderRequests.ToArray());

            var artifacts = processed.Select((t, i) =>
            {
                var renderResult = renderResults[i];
                return new Artifact
                {
                    Path = $"{t.MetaData.Uri}",
                    Contents = Encoding.UTF8.GetBytes(renderResult.Content)
                };
            }).ToList();

            // TODO can we do this better?
            var directoryContents =
                            _fileSystem.GetDirectoryContents("");
            var rootFile = directoryContents.FirstOrDefault();
            if (rootFile != null)
            {
                var root = rootFile.PhysicalPath.Replace(rootFile.Name, "");
                // var root2 = Directory.GetCurrentDirectory();

                var assets = _fileSystem.GetFiles(request.Configuration.AssetDirectory, true)
                    .Select(x => x.PhysicalPath.Replace(root, string.Empty));
                artifacts.AddRange(assets.Select(asset =>
                {
                    return new Artifact
                    {
                        Path = $"{asset}",
                        Contents = _fileSystem.GetFileBytes(asset)
                    };
                }));
                await _artifactAccess.Store(new StoreArtifactsRequest
                {
                    Artifacts = artifacts.ToArray(),
                    OutputLocation = new FileSystemOutputLocation() {
                        Clean = false,
                        Path = request.Configuration.Destination
                    }
                });
            }
        }
    }
}
