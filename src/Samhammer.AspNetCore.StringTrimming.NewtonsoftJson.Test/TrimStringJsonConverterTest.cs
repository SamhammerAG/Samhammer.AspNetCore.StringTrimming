using AwesomeAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    public class TrimStringJsonConverterTest
    {
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings().AddStringTrimming();

        [Fact]
        public void Deserialize_TrimsStringValues()
        {
            var json = """
                {
                    "name": "  Alice \r\n",
                    "nested": { "value": "\t Inner \u00A0" },
                    "items": [ { "value": " First " }, { "value": "Second  " } ],
                    "texts": [ "  a ", " b", null ],
                    "dictionary": { " key ": "  value  " }
                }
                """;

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be("Alice");
            result.Nested.Value.Should().Be("Inner");
            result.Items.Select(i => i.Value).Should().Equal("First", "Second");
            result.Texts.Should().Equal("a", "b", null);
            result.Dictionary.Should().ContainKey(" key ").WhoseValue.Should().Be("value");
        }

        [Theory]
        [InlineData("{ \"name\": null }", null)]
        [InlineData("{ \"name\": \"\" }", "")]
        [InlineData("{ \"name\": \"   \" }", "")]
        [InlineData("{ \"name\": \"a  b\" }", "a  b")]
        public void Deserialize_EdgeCases(string json, string expected)
        {
            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be(expected);
        }

        [Fact]
        public void Deserialize_RootString_IsTrimmed()
        {
            JsonConvert.DeserializeObject<string>("\"  root value  \"", settings).Should().Be("root value");
        }

        [Fact]
        public void Deserialize_WithNoTrimStringJsonConverter_KeepsWhitespace()
        {
            var result = JsonConvert.DeserializeObject<TestContract>("{ \"name\": \"  trim me  \", \"raw\": \"  keep me \\n\" }", settings);

            result.Name.Should().Be("trim me");
            result.Raw.Should().Be("  keep me \n");
        }

        [Fact]
        public void Serialize_DoesNotTrim()
        {
            var json = JsonConvert.SerializeObject(new TestContract { Name = "  Alice  " }, settings);

            json.Should().Contain("\"Name\":\"  Alice  \"");
        }

        [Fact]
        public void AddStringTrimming_CalledTwice_AddsConverterOnce()
        {
            var twice = new JsonSerializerSettings().AddStringTrimming().AddStringTrimming();

            twice.Converters.OfType<TrimStringJsonConverter>().Should().HaveCount(1);
        }

        // Known limitation (same as FUTO): with a converter for string, Newtonsoft.Json passes non-string tokens to the converter.
        // Without the converter these values would be accepted as "123", "True" and "2026-10-01T10:00:00".
        [Theory]
        [InlineData("{ \"name\": 123 }")]
        [InlineData("{ \"name\": true }")]
        [InlineData("{ \"name\": \"2026-10-01T10:00:00\" }")]
        public void Deserialize_NonStringTokens_AreRejected(string json)
        {
            var act = () => JsonConvert.DeserializeObject<TestContract>(json, settings);

            act.Should().Throw<JsonSerializationException>();
        }

        public class TestContract
        {
            public string Name { get; set; }

            [JsonConverter(typeof(NoTrimStringJsonConverter))]
            public string Raw { get; set; }

            public NestedTestContract Nested { get; set; }

            public List<NestedTestContract> Items { get; set; }

            public List<string> Texts { get; set; }

            public Dictionary<string, string> Dictionary { get; set; }
        }

        public class NestedTestContract
        {
            public string Value { get; set; }
        }
    }
}
