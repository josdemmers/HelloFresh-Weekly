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
        public PageProps pageProps { get; set; }
    }

    public class PageProps
    {
        public SsrPayload ssrPayload { get; set; }
    }

    public class SsrPayload
    {
        public ContentfulLandingPagesEntries contentfulLandingPagesEntries { get; set; }
    }

    public class ContentfulLandingPagesEntries
    {
        public VariationItem variationItem { get; set; }
    }

    public class VariationItem
    {
        public PageVariation pageVariation { get; set; }
    }

    public class PageVariation
    {
        public Fields fields { get; set; }
    }

    public class Fields
    {
        public string id { get; set; }
        public string description { get; set; }
        public List<Section> sections { get; set; }
        public List<Stack> stack { get; set; }
    }

    public class Section
    {
        // Note: The section with "id": "nlBE-Recipes-Content" contains the recipes
        public Fields fields { get; set; }
    }
    public class Stack
    {
        public string description { get; set; }
        public string title { get; set; }
    }
}
