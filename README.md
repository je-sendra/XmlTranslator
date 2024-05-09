# XmlTranslator

This has currently been tested to be able to translate this xml file:

```xml
<?xml version='1.0' encoding='UTF-8'?>
<labels>
    <entry key='key1'>value1</entry>
</label>
```

You can use it by navigating to the project root and running the following command:

```shell
dotnet run --project Cli filePath.xml languageCode interval
```

Where `filePath.xml` is the path to the xml file you want to translate, `languageCode` is the language code you want to translate to, and `interval` is the interval of lines you want to be written to the output file at a time.

This is an example of how you can use it:

```shell
dotnet run --project Cli SampleData/labels.xml es 50
```

This will translate the `labels.xml` file to Spanish and write the output to `Output\labels_es_timestamp.xml` with 50 lines per interval.

Enjoy!
