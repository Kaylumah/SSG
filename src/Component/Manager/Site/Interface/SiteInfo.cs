// Copyright (c) Kaylumah, 2021. All rights reserved.
// See LICENSE file in the project root for full license information.
using System.Runtime.Serialization;

namespace Kaylumah.Ssg.Manager.Site.Interface
{
    public class SiteInfo
    {
        public string Lang { get; set; }
        public string BaseUrl { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Collections Collections { get; set; } = new Collections();
    }
}