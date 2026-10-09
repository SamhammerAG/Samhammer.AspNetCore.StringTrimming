using AwesomeAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    /// <summary>
    /// [NoTrim] is read by <see cref="TrimStringContractResolver"/>, no extra converter is needed on the member.
    /// </summary>
    public class NoTrimAttributeTest
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        }.AddStringTrimming();

        [Fact]
        public void StringProperty_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<PropertyContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep \\n\" }", settings);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep \n");
        }

        [Fact]
        public void StringField_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<FieldContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \", \"rawTexts\": [ \" keep \" ] }", settings);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep  ");
            result.RawTexts.Should().Equal(" keep ");
        }

        [Fact]
        public void StringCollections_KeepWhitespace()
        {
            var json = """
                {
                    "texts": [ " trim " ],
                    "rawList": [ " a ", null ],
                    "rawArray": [ " b " ],
                    "rawEnumerable": [ " c " ],
                    "rawReadOnlyList": [ " d " ],
                    "rawHashSet": [ " e " ]
                }
                """;

            var result = JsonConvert.DeserializeObject<CollectionContract>(json, settings);

            result.Texts.Should().Equal("trim");
            result.RawList.Should().Equal(" a ", null);
            result.RawArray.Should().Equal(" b ");
            result.RawEnumerable.Should().Equal(" c ");
            result.RawReadOnlyList.Should().Equal(" d ");
            result.RawHashSet.Should().Equal(" e ");
        }

        [Fact]
        public void StringDictionaries_KeepWhitespace()
        {
            var json = """
                {
                    "dictionary": { "k": " trim " },
                    "rawDictionary": { "k": " a " },
                    "rawIDictionary": { "k": " b " },
                    "rawReadOnlyDictionary": { "k": " c " }
                }
                """;

            var result = JsonConvert.DeserializeObject<DictionaryContract>(json, settings);

            result.Dictionary["k"].Should().Be("trim");
            result.RawDictionary["k"].Should().Be(" a ");
            result.RawIDictionary["k"].Should().Be(" b ");
            result.RawReadOnlyDictionary["k"].Should().Be(" c ");
        }

        [Fact]
        public void ConstructorParameter_ViaProperty_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<PropertyRecord>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \" }", settings);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep  ");
        }

        [Fact]
        public void ConstructorParameter_ViaParameter_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<ParameterContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \" }", settings);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep  ");
        }

        [Fact]
        public void JsonConstructor_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<JsonConstructorContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \" }", settings);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep  ");
        }

        [Fact]
        public void NestedContracts_KeepWhitespace()
        {
            var result = JsonConvert.DeserializeObject<NestedContract>("{ \"items\": [ { \"name\": \" trim \", \"raw\": \" keep \" } ] }", settings);

            result.Items.Single().Name.Should().Be("trim");
            result.Items.Single().Raw.Should().Be(" keep ");
        }

        [Fact]
        public void NonStringTokens_KeepOriginalText()
        {
            var result = JsonConvert.DeserializeObject<CollectionContract>("{ \"rawList\": [ 1.50, true, \" 2026-10-01T10:00:00 \" ] }", settings);

            result.RawList.Should().Equal("1.50", "true", " 2026-10-01T10:00:00 ");
        }

        // Known limitation: with Newtonsoft.Json [NoTrim] is only supported on strings and string collections/dictionaries.
        // On complex types it is ignored and the strings inside are trimmed (System.Text.Json supports this).
        [Fact]
        public void ComplexType_IsIgnored_StringsInsideAreTrimmed()
        {
            var result = JsonConvert.DeserializeObject<ComplexTypeContract>("{ \"rawNested\": { \"name\": \" trimmed \" } }", settings);

            result.RawNested.Name.Should().Be("trimmed");
        }

        [Fact]
        public void ExistingJsonConverter_IsKept()
        {
            var result = JsonConvert.DeserializeObject<CustomConverterContract>("{ \"raw\": \" value \" }", settings);

            result.Raw.Should().Be("custom: value ");
        }

        [Fact]
        public void Serialize_IsNotAffected()
        {
            var contract = new CollectionContract { Texts = [" a "], RawList = [" b "] };

            var json = JsonConvert.SerializeObject(contract, settings);

            json.Should().Contain("\"texts\":[\" a \"]").And.Contain("\"rawList\":[\" b \"]");
        }

        public class PropertyContract
        {
            public string Name { get; set; }

            [NoTrim]
            public string Raw { get; set; }
        }

#pragma warning disable SA1401 // Fields should be private: public fields are tested on purpose
        public class FieldContract
        {
            public string Name;

            [NoTrim]
            public string Raw;

            [NoTrim]
            public List<string> RawTexts;
        }
#pragma warning restore SA1401

        public class CollectionContract
        {
            public List<string> Texts { get; set; }

            [NoTrim]
            public List<string> RawList { get; set; }

            [NoTrim]
            public string[] RawArray { get; set; }

            [NoTrim]
            public IEnumerable<string> RawEnumerable { get; set; }

            [NoTrim]
            public IReadOnlyList<string> RawReadOnlyList { get; set; }

            [NoTrim]
            public HashSet<string> RawHashSet { get; set; }
        }

        public class DictionaryContract
        {
            public Dictionary<string, string> Dictionary { get; set; }

            [NoTrim]
            public Dictionary<string, string> RawDictionary { get; set; }

            [NoTrim]
            public IDictionary<string, string> RawIDictionary { get; set; }

            [NoTrim]
            public IReadOnlyDictionary<string, string> RawReadOnlyDictionary { get; set; }
        }

        public record PropertyRecord(string Name, [property: NoTrim] string Raw);

        public class ParameterContract
        {
            public ParameterContract(string name, [NoTrim] string raw)
            {
                Name = name;
                Raw = raw;
            }

            public string Name { get; }

            public string Raw { get; }
        }

        public class JsonConstructorContract
        {
            public JsonConstructorContract()
            {
            }

            [JsonConstructor]
            public JsonConstructorContract(string name, string raw)
            {
                Name = name;
                Raw = raw;
            }

            public string Name { get; }

            [NoTrim]
            public string Raw { get; }
        }

        public class NestedContract
        {
            public List<PropertyContract> Items { get; set; }
        }

        public class ComplexTypeContract
        {
            [NoTrim]
            public PropertyContract RawNested { get; set; }
        }

        public class CustomConverterContract
        {
            [NoTrim]
            [JsonConverter(typeof(PrefixConverter))]
            public string Raw { get; set; }
        }

        public sealed class PrefixConverter : JsonConverter<string>
        {
            public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return "custom:" + (string)reader.Value;
            }

            public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }
        }
    }
}
