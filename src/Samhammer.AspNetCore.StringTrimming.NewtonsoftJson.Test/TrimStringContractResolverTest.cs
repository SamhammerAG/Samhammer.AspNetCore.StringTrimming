using AwesomeAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    public class TrimStringContractResolverTest
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        }.AddStringTrimming();

        [Fact]
        public void Deserialize_TrimsAllStringValues()
        {
            var json = """
                {
                    "name": "  Alice \r\n",
                    "nested": { "value": "\t Inner  " },
                    "items": [ { "value": " First " }, { "value": "Second  " } ],
                    "texts": [ "  a ", " b", null ],
                    "array": [ " c " ],
                    "enumerable": [ " d " ],
                    "readOnlyList": [ " e " ],
                    "hashSet": [ " f " ],
                    "dictionary": { " key ": "  value  " },
                    "readOnlyDictionary": { "k": " v " }
                }
                """;

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be("Alice");
            result.Nested.Value.Should().Be("Inner");
            result.Items.Select(i => i.Value).Should().Equal("First", "Second");
            result.Texts.Should().Equal("a", "b", null);
            result.Array.Should().Equal("c");
            result.Enumerable.Should().Equal("d");
            result.ReadOnlyList.Should().Equal("e");
            result.HashSet.Should().Equal("f");
            result.Dictionary.Should().ContainKey(" key ").WhoseValue.Should().Be("value");
            result.ReadOnlyDictionary["k"].Should().Be("v");
        }

        [Theory]
        [InlineData("{ \"name\": null }", null)]
        [InlineData("{ \"name\": \"\" }", "")]
        [InlineData("{ \"name\": \"   \" }", "")]
        [InlineData("{ \"name\": \"a  b\" }", "a  b")]
        [InlineData("{ \"name\": 123 }", "123")]
        [InlineData("{ \"name\": \"2026-10-01T10:00:00+02:00\" }", "2026-10-01T10:00:00+02:00")]
        [InlineData("{ \"name\": \" 2026-10-01T10:00:00 \" }", "2026-10-01T10:00:00")]
        public void Deserialize_EdgeCases_BehaveLikeNewtonsoftWithoutTrimming(string json, string expected)
        {
            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be(expected);
        }

        [Fact]
        public void Deserialize_DateLikeStringsInCollections_AreNotParsedAsDates()
        {
            var result = JsonConvert.DeserializeObject<TestContract>("{ \"texts\": [ \"2026-10-01T10:00:00+02:00\" ], \"dictionary\": { \"d\": \"2026-10-01T10:00:00+02:00\" } }", settings);

            result.Texts.Should().Equal("2026-10-01T10:00:00+02:00");
            result.Dictionary["d"].Should().Be("2026-10-01T10:00:00+02:00");
        }

        [Fact]
        public void Deserialize_ReusesInitializedCollections()
        {
            var result = JsonConvert.DeserializeObject<TestContract>("{ \"initialized\": [ \" b \" ] }", settings);

            result.Initialized.Should().Equal("a", "b");
        }

        [Fact]
        public void Deserialize_ConstructorParameters()
        {
            var result = JsonConvert.DeserializeObject<TestRecord>("{ \"name\": \"  Bob  \", \"raw\": \"  keep  \" }", settings);

            result.Name.Should().Be("Bob");
            result.Raw.Should().Be("  keep  ");
        }

        [Fact]
        public void Deserialize_WithNoTrimAttribute_KeepsWhitespace()
        {
            var json = """
                {
                    "name": "  trim me  ",
                    "raw": "  keep me \n",
                    "rawTexts": [ " keep " ],
                    "rawDictionary": { "k": " keep " }
                }
                """;

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be("trim me");
            result.Raw.Should().Be("  keep me \n");
            result.RawTexts.Should().Equal(" keep ");
            result.RawDictionary["k"].Should().Be(" keep ");
        }

        [Fact]
        public void Deserialize_ObjectInStringList_Throws()
        {
            var act = () => JsonConvert.DeserializeObject<TestContract>("{ \"texts\": [ { \"a\": 1 } ] }", settings);

            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void Serialize_DoesNotTrimAndKeepsCamelCase()
        {
            var contract = new TestContract { Name = "  Alice  ", Texts = new List<string> { " a " }, Dictionary = new Dictionary<string, string> { { "k", " v " } } };

            var json = JsonConvert.SerializeObject(contract, settings);

            json.Should().Contain("\"name\":\"  Alice  \"").And.Contain("\"texts\":[\" a \"]").And.Contain("\"k\":\" v \"");
        }

        [Fact]
        public void AddStringTrimming_KeepsNamingStrategy()
        {
            settings.ContractResolver.Should().BeOfType<TrimStringContractResolver>()
                .Which.NamingStrategy.Should().BeOfType<CamelCaseNamingStrategy>();
        }

        [Fact]
        public void AddStringTrimming_WithCamelCasePropertyNamesContractResolver_UsesCamelCase()
        {
            var camelCaseSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }.AddStringTrimming();

            camelCaseSettings.ContractResolver.Should().BeOfType<TrimStringContractResolver>()
                .Which.NamingStrategy.Should().BeOfType<CamelCaseNamingStrategy>();
        }

        [Fact]
        public void AddStringTrimming_WithCustomContractResolver_Throws()
        {
            var act = () => new JsonSerializerSettings { ContractResolver = new CustomContractResolver() }.AddStringTrimming();

            act.Should().Throw<InvalidOperationException>();
        }

        public class TestContract
        {
            public string Name { get; set; }

            [NoTrim]
            public string Raw { get; set; }

            public NestedTestContract Nested { get; set; }

            public List<NestedTestContract> Items { get; set; }

            public List<string> Texts { get; set; }

            public string[] Array { get; set; }

            public IEnumerable<string> Enumerable { get; set; }

            public IReadOnlyList<string> ReadOnlyList { get; set; }

            public HashSet<string> HashSet { get; set; }

            public List<string> Initialized { get; } = new() { "a" };

            public Dictionary<string, string> Dictionary { get; set; }

            public IReadOnlyDictionary<string, string> ReadOnlyDictionary { get; set; }

            [NoTrim]
            public List<string> RawTexts { get; set; }

            [NoTrim]
            public Dictionary<string, string> RawDictionary { get; set; }
        }

        public class NestedTestContract
        {
            public string Value { get; set; }
        }

        public record TestRecord(string Name, [property: NoTrim] string Raw);

        private sealed class CustomContractResolver : DefaultContractResolver
        {
        }
    }
}
