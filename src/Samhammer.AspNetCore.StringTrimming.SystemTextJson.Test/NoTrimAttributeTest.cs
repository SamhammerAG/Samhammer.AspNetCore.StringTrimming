using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson.Test
{
    /// <summary>
    /// [NoTrim] is applied by <see cref="NoTrimJsonTypeInfoModifier"/>, no extra converter is needed on the member.
    /// </summary>
    public class NoTrimAttributeTest
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddStringTrimming();

        [Fact]
        public void StringProperty_KeepsWhitespace()
        {
            var result = JsonSerializer.Deserialize<PropertyContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep \\n\" }", options);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep \n");
        }

        [Fact]
        public void StringField_KeepsWhitespace()
        {
            var result = JsonSerializer.Deserialize<FieldContract>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \", \"rawTexts\": [ \" keep \" ] }", options);

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

            var result = JsonSerializer.Deserialize<CollectionContract>(json, options);

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

            var result = JsonSerializer.Deserialize<DictionaryContract>(json, options);

            result.Dictionary["k"].Should().Be("trim");
            result.RawDictionary["k"].Should().Be(" a ");
            result.RawIDictionary["k"].Should().Be(" b ");
            result.RawReadOnlyDictionary["k"].Should().Be(" c ");
        }

        [Fact]
        public void ConstructorParameter_ViaProperty_KeepsWhitespace()
        {
            var result = JsonSerializer.Deserialize<PropertyRecord>("{ \"name\": \"  trim  \", \"raw\": \"  keep  \" }", options);

            result.Name.Should().Be("trim");
            result.Raw.Should().Be("  keep  ");
        }

        [Fact]
        public void NestedContracts_KeepWhitespace()
        {
            var result = JsonSerializer.Deserialize<NestedContract>("{ \"items\": [ { \"name\": \" trim \", \"raw\": \" keep \" } ] }", options);

            result.Items.Single().Name.Should().Be("trim");
            result.Items.Single().Raw.Should().Be(" keep ");
        }

        [Fact]
        public void ComplexType_NothingInsideIsTrimmed()
        {
            var result = JsonSerializer.Deserialize<ComplexTypeContract>("{ \"rawNested\": { \"name\": \" keep \", \"texts\": [ \" keep \" ] }, \"nested\": { \"name\": \" trim \" } }", options);

            result.RawNested.Name.Should().Be(" keep ");
            result.RawNested.Texts.Should().Equal(" keep ");
            result.Nested.Name.Should().Be("trim");
        }

        [Fact]
        public void ExistingJsonConverter_IsKept()
        {
            var result = JsonSerializer.Deserialize<CustomConverterContract>("{ \"raw\": \" value \" }", options);

            result.Raw.Should().Be("custom: value ");
        }

        [Fact]
        public void Serialize_IsNotAffected()
        {
            var contract = new CollectionContract { Texts = [" a "], RawList = [" b "] };

            var json = JsonSerializer.Serialize(contract, options);

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
            [JsonInclude]
            public string Name;

            [JsonInclude]
            [NoTrim]
            public string Raw;

            [JsonInclude]
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

        public class NestedContract
        {
            public List<PropertyContract> Items { get; set; }
        }

        public class ComplexTypeContract
        {
            [NoTrim]
            public ComplexTypeChild RawNested { get; set; }

            public ComplexTypeChild Nested { get; set; }
        }

        public class ComplexTypeChild
        {
            public string Name { get; set; }

            public List<string> Texts { get; set; }
        }

        public class CustomConverterContract
        {
            [NoTrim]
            [JsonConverter(typeof(PrefixConverter))]
            public string Raw { get; set; }
        }

        public sealed class PrefixConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return "custom:" + reader.GetString();
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
