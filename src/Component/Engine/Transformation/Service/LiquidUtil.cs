// Copyright (c) Kaylumah, 2021. All rights reserved.
// See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Kaylumah.Ssg.Utilities
{
    public interface IRenderModel
    {
        string Content { get; }
    }
    public class RenderRequest
    {
        public IRenderModel Model { get; set; }
        public string TemplateName { get; set; }
    }

    public class RenderResult
    {
        public string Content { get; set; }
    }

    internal class MyIncludeFromDisk : ITemplateLoader
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _templateFolder;
        public MyIncludeFromDisk(IFileSystem fileSystem, string templateFolder)
        {
            _fileSystem = fileSystem;
            _templateFolder = templateFolder;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return Path.Combine(_fileSystem.GetFile(_templateFolder).Name, templateName);
            // return Path.Combine(Environment.CurrentDirectory, templateName);
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            using var reader = new StreamReader(_fileSystem.GetFile(templatePath).CreateReadStream());
            return reader.ReadToEnd();
            //return File.ReadAllText(templatePath);
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            using var reader = new StreamReader(_fileSystem.GetFile(templatePath).CreateReadStream());
            return await reader.ReadToEndAsync();
            // return await File.ReadAllTextAsync(templatePath);
        }
    }
}