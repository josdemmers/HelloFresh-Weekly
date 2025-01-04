using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HelloFresh_Weekly
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length <= 0) throw new ArgumentException("Please supply arguments.");

            var client = new HttpClient();
            var filePath = args[0];

            if (!File.Exists(filePath)) throw new FileNotFoundException($"Cannot find file {filePath} to edit.");

            IConfiguration config = Configuration.Default.WithDefaultLoader();
            string address = "https://www.hellofresh.nl/about/nieuws";
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(address);
            IHtmlCollection<IElement> htmlAnchorElements = document.QuerySelectorAll("a");
            var recipes = htmlAnchorElements
                .Where(e => ((IHtmlAnchorElement)e).Href.EndsWith("pdf", StringComparison.OrdinalIgnoreCase) && IsLanDutchRecipe(((IHtmlAnchorElement)e).Href) && !IsModularityRecipe(((IHtmlAnchorElement)e).Href))
                .Select(ef => ((IHtmlAnchorElement)ef).Href).ToList();

            Console.WriteLine($"Number of recipe files found: {recipes.Count}");

            if (recipes.Count == 0)
            {
                Console.WriteLine($"Starting json alternative...");

                string raw = document.ToHtml();
                int indexStartJson = raw.IndexOf("{\"props\":{\"pageProps\":");
                int indexEndJson = raw.IndexOf("</script><script>", indexStartJson);

                if (indexStartJson == -1 || indexEndJson == -1) 
                {
                    Console.WriteLine($"Possible new website layout. Unable to find any recipes.");
                    return;
                }

                string json = raw.Substring(indexStartJson, indexEndJson - indexStartJson);
                var helloFreshAsJson = JsonSerializer.Deserialize<HelloFreshAsJson>(json) ?? new HelloFreshAsJson();
                var recipeSection = helloFreshAsJson.props.pageProps.ssrPayload.contentfulLandingPagesEntries.variationItem.pageVariation.fields.sections.FirstOrDefault(s => s.fields.id.Equals("nlBE-Recipes-Content"));
                if (recipeSection != null) 
                {
                    foreach (var stack in recipeSection.fields.stack)
                    {
                        Console.WriteLine($"{stack.title}. Recipes: {stack.description.Split(".pdf").Count()}");
                        var matches = Regex.Matches(stack.description, "(assets|downloads).+(NL.pdf)");
                        
                        foreach (var match in matches)
                        {
                            string recipeUrl = match.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(recipeUrl) && IsLanDutchRecipe(recipeUrl) && !IsModularityRecipe(recipeUrl))
                            {
                                Console.WriteLine($"{recipeUrl}");
                                recipes.Add($"http://{recipeUrl}");
                            }                           
                        }

                    }   
                }
            }

            Directory.CreateDirectory($"Recipes{DateTime.Now.Year}");
            foreach (var recipe in recipes)
            {
                Console.WriteLine($"Downloading: {recipe}");

                var uri = new Uri(recipe);

                // Example:
                // ./Recipes2023/2023.49.ALL.1-32.NL.pdf

                await using var s = await client.GetStreamAsync(uri);
                await using var file = File.Create($"Recipes{DateTime.Now.Year}/{DateTime.Now.Year}.{Path.GetFileName(recipe)}");
                await s.CopyToAsync(file);
            }

            // Update readme
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            stringWriter.WriteLine("# HelloFresh-Weekly");
            stringWriter.WriteLine($"Last update: {DateTime.Now}");
            stringWriter.WriteLine(string.Empty);
            foreach (var recipe in recipes)
            {
                // Example:
                // - [2023.49.ALL.1-32.NL.pdf](./Recipes2023/2023.49.ALL.1-32.NL.pdf)

                stringWriter.WriteLine($"- [{DateTime.Now.Year}.{Path.GetFileName(recipe)}](./Recipes{DateTime.Now.Year}/{DateTime.Now.Year}.{Path.GetFileName(recipe)})");
            }
            stringWriter.Flush();
            stringWriter.Close();
            File.WriteAllText(filePath, stringBuilder.ToString());
        }

        private static bool IsLanDutchRecipe(string uri)
        {
            var file = Path.GetFileName(uri);
            return file.Contains(".NL.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNormalRecipe(string uri)
        {
            var file = Path.GetFileName(uri);
            return file.Contains(".ALL.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAddonRecipe(string uri)
        {
            var file = Path.GetFileName(uri);
            return file.Contains(".ADDON.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsModularityRecipe(string uri)
        {
            var file = Path.GetFileName(uri);
            return file.Contains(".Modularity.", StringComparison.OrdinalIgnoreCase);
        }
    }
}