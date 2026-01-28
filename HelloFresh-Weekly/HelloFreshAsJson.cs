using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HelloFresh_Weekly
{
    public class HelloFreshAsJson
    {
        public Props props { get; set; }
    }

    public class Props
    {
        public PageProps pageProps { get; set; } = new PageProps();
    }

    public class PageProps
    {
        public SsrPayload ssrPayload { get; set; } = new SsrPayload();
    }

    public class SsrPayload
    {
        public ContentfulLandingPagesEntries contentfulLandingPagesEntries { get; set; } = new ContentfulLandingPagesEntries();
    }

    public class ContentfulLandingPagesEntries
    {
        public VariationItem variationItem { get; set; } = new VariationItem();
    }

    public class VariationItem
    {
        public PageVariation pageVariation { get; set; } = new PageVariation();
    }

    public class PageVariation
    {
        public Fields fields { get; set; } = new Fields();
    }

    public class Fields
    {
        // Note: The field with "id": "nlNL-Contentmoduleimagesection" contains the recipes
        //       or "title": "Nederlands".
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<Section> sections { get; set; } = new List<Section>();
    }

    public class Section
    {
        public Fields fields { get; set; } = new Fields();
    }
    public class Stack
    {
        public string description { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
    }
}
