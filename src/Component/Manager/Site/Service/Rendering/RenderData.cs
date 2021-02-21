using System.Collections.Generic;
using Kaylumah.Ssg.Utilities;

namespace Kaylumah.Ssg.Manager.Site.Service
{
    class RenderData : IRenderModel, IMetadata
    {
        public BuildData Build { get;set; }
        public SiteData Site { get;set; } = new SiteData();
        public PageData Page { get;set; }
        public string Content => Page?.Content ?? string.Empty;
        public string Title => Page?.Title ?? Site?.Title ?? null;
        public string Description => Page?.Description ?? Site?.Description ?? null;
        public string Language => Page?.Language ?? Site?.Language ?? null;
        public string Author => Page?.Author ?? Site?.Author ?? null;

    }

    class SiteData : Dictionary<string, object>, IMetadata, ISiteMetadata
    {
        public string Title => this.GetValue<string>(nameof(Title));
        public string Description => this.GetValue<string>(nameof(Description));
        public string Language => this.GetValue<string>(nameof(Language));
        public string Author => this.GetValue<string>(nameof(Author));

        public Dictionary<string, object> Data
        {
            get
            {
                return this.GetValue<Dictionary<string, object>>(nameof(Data));
            }
            set
            {
                this.SetValue(nameof(Data), value);
            }
        }

        public Dictionary<string, object> Collections            
        {
            get
            {
                return this.GetValue<Dictionary<string, object>>(nameof(Collections));
            }
            set
            {
                this.SetValue(nameof(Collections), value);
            }
        }
    }

    class PageData : Dictionary<string, object>, IMetadata, IPageMetadata
    {
        public string Title => this.GetValue<string>(nameof(Title));
        public string Description => this.GetValue<string>(nameof(Description));
        public string Language => this.GetValue<string>(nameof(Language));
        public string Author => this.GetValue<string>(nameof(Author));
        public string Content { get; }

        public PageData(File file) : base(file.MetaData)
        {
            Content = file.Content;
        }
    }

    public interface IMetadata
    {
        string Title { get; }
        string Description { get; }
        string Language { get; }
        string Author { get; }
    }

    public interface IPageMetadata
    {
        string Content { get; }
    }

    public interface ISiteMetadata
    {
        Dictionary<string, object> Data { get; set; }
        Dictionary<string, object> Collections { get; set; }
    }
}