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
            IHtmlCollection<IElement> htmlScriptElements = document.QuerySelectorAll("script");
            var recipesContainer = htmlScriptElements
                .Where(e => ((IHtmlScriptElement)e).Id?.Equals("__NEXT_DATA__", StringComparison.OrdinalIgnoreCase) ?? false)
                ?.FirstOrDefault()?.InnerHtml ?? string.Empty;

            List<string> recipes = new List<string>();
            var helloFreshAsJson = JsonSerializer.Deserialize<HelloFreshAsJson>(recipesContainer) ?? new HelloFreshAsJson();
            var recipeSection = helloFreshAsJson.props.pageProps.ssrPayload.contentfulLandingPagesEntries.variationItem.pageVariation.fields.sections.FirstOrDefault(s => s.fields.id.Equals("nlNL-Contentmoduleimagesection"));
            if (recipeSection != null)
            {
                var fields = recipeSection.fields;
                Console.WriteLine($"{fields.title}. Recipes: {fields.description.Split(".pdf").Count()}");
                var matches = Regex.Matches(fields.description, "(assets|downloads).+(NL.pdf)");

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