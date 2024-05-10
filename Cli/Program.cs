using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace XmlTranslator.Cli;

public static class Program
{
    // Output directory
    public const string OutputDirectory = "Output";

    // DEEPL API key
    private static string DeepLApiKey { get; set; }

    // Entry point
    public static void Main(string[] args)
    {
        // Create output directory if it doesn't exist
        if (!Directory.Exists(OutputDirectory))
        {
            Directory.CreateDirectory(OutputDirectory);
        }

        // Print the CLI name
        Console.WriteLine("XmlTranslator CLI");

        // Load .env file
        if (!File.Exists(".env"))
        {
            Console.WriteLine("The .env file does not exist.");
            Environment.Exit(1);
        }

        // Read the .env file
        foreach (var line in File.ReadAllLines(".env"))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }

        // Check if DEEPL_API_KEY is set
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DEEPL_API_KEY")))
        {
            Console.WriteLine("Please set the DEEPL_API_KEY environment variable.");
            Environment.Exit(1);
        }

        // Set the DEEPL API key
        DeepLApiKey = Environment.GetEnvironmentVariable("DEEPL_API_KEY");

        // Get XML file
        var xmlFile = args.FirstOrDefault();

        // Get output language
        var outputLanguage = args.Skip(1).FirstOrDefault();

        // Check if XML file is provided
        if (string.IsNullOrWhiteSpace(xmlFile))
        {
            Console.WriteLine("Please provide an XML file.");
            Environment.Exit(1);
        }

        // Check if output language is provided
        if (string.IsNullOrWhiteSpace(outputLanguage))
        {
            Console.WriteLine("Please provide an output language.");
            Environment.Exit(1);
        }

        // Check if XML file exists
        if (!File.Exists(xmlFile))
        {
            Console.WriteLine("The provided XML file does not exist.");
            Environment.Exit(1);
        }

        // Check if output language is supported
        switch (outputLanguage.ToLower())
        {
            case "es":
            case "en":
                break;
            default:
                Console.WriteLine("The provided output language is not supported.");
                Environment.Exit(1);
                break;
        }

        // Get the XML file
        var xml = XElement.Parse(File.ReadAllText(xmlFile));

        // Get timestamp
        var timestamp = DateTime.Now.Ticks;

        // Translate the XML file
        var translatedLines = new List<string>();

        foreach (var currentElement in xml.Elements())
        {
            // Check if the element has to be translated
            if (currentElement.Name.LocalName == "entry")
            {
                // If a multiple of 100 lines has been translated, print the ammount of translated lines
                if (translatedLines.Count % 100 == 0)
                    Console.WriteLine($"Translated {translatedLines.Count} lines.");

                // Get the value
                var value = currentElement.Value;

                // Translate the value
                var translatedValue = Translate(value, outputLanguage);

                // Change the element value
                currentElement.Value = translatedValue;

                // Add the translated line
                translatedLines.Add(currentElement.ToString());
            }
            else
            {
                // Add the line as it is
                translatedLines.Add(currentElement.ToString());
            }
        }

        // Get the output file name
        var outputFileName = Path.GetFileNameWithoutExtension(xmlFile) + $"_{outputLanguage}_{DateTime.Now.Ticks}.xml";

        // Write the translated lines to the output file
        File.WriteAllLines(Path.Combine(OutputDirectory, outputFileName), translatedLines);
    }

    // Translate the value using DeepL
    public static string Translate(string value, string outputLanguage)
    {
        // Send the request to translate the value using DeepL
        var response = new HttpClient().PostAsync("https://api-free.deepl.com/v2/translate", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["auth_key"] = DeepLApiKey,
            ["text"] = value,
            ["target_lang"] = outputLanguage
        })).Result;

        // Check if the response is successful
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to translate the value.");
            Environment.Exit(1);
        }

        // Read the response content
        var content = response.Content.ReadAsStringAsync().Result;

        // Parse the response content
        var json = JsonDocument.Parse(content);

        // Get the translated value
        var translatedValue = json.RootElement.GetProperty("translations")[0].GetProperty("text").GetString();

        // Return the translated value
        return translatedValue;
    }
}
