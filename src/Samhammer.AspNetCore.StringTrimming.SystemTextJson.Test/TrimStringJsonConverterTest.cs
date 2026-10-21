using System.Text.Json;
using AwesomeAssertions;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.SystemTextJson.Test
{
    public class TrimStringJsonConverterTest
    {
        private readonly JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddStringTrimming();

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
                    "dictionary": { " key ": "  value  " }
                }
                """;

            var result = JsonSerializer.Deserialize<TestContract>(json, options);

            result.Name.Should().Be("Alice");
            result.Nested.Value.Should().Be("Inner");
            result.Items.Select(i => i.Value).Should().Equal("First", "Second");
            result.Texts.Should().Equal("a", "b", null);
            result.Array.Should().Equal("c");
            result.Dictionary.Should().ContainKey(" key ").WhoseValue.Should().Be("value");
        }

        [Theory]
        [InlineData("{ \"name\": null }", null)]
        [InlineData("{ \"name\": \"\" }", "")]
        [InlineData("{ \"name\": \"   \" }", "")]
        [InlineData("{ \"name\": \"a  b\" }", "a  b")]
        [InlineData("{ \"name\": \" 2026-10-01T10:00:00+02:00 \" }", "2026-10-01T10:00:00+02:00")]
        public void Deserialize_EdgeCases(string json, string expected)
        {
            var result = JsonSerializer.Deserialize<TestContract>(json, options);

            result.Name.Should().Be(expected);
        }

        [Fact]
        public void Deserialize_RootStringAndStringList()
        {
            JsonSerializer.Deserialize<string>("\"  root value  \"", options).Should().Be("root value");
            JsonSerializer.Deserialize<List<string>>("[\" id1 \", \"id2  \"]", options).Should().Equal("id1", "id2");
        }

        [Fact]
        public void Deserialize_ConstructorParameters()
        {
            var result = JsonSerializer.Deserialize<TestRecord>("{ \"name\": \"  Bob  \", \"raw\": \"  keep  \" }", options);

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
                    "rawNested": { "value": " keep " }
                }
                """;

            var result = JsonSerializer.Deserialize<TestContract>(json, options);

            result.Name.Should().Be("trim me");
            result.Raw.Should().Be("  keep me \n");
            result.RawTexts.Should().Equal(" keep ");
            result.RawNested.Value.Should().Be(" keep ");
        }

        [Fact]
        public void Serialize_DoesNotTrim()
        {
            var contract = new TestContract { Name = "  Alice  ", Raw = " raw ", Texts = new List<string> { " a " } };

            var json = JsonSerializer.Serialize(contract, options);

            json.Should().Contain("\"name\":\"  Alice  \"").And.Contain("\"raw\":\" raw \"").And.Contain("\"texts\":[\" a \"]");
        }

        [Fact]
        public void AddStringTrimming_CalledTwice_AddsConverterOnce()
        {
            var twice = new JsonSerializerOptions().AddStringTrimming().AddStringTrimming();

            twice.Converters.OfType<TrimStringJsonConverter>().Should().HaveCount(1);
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

            public Dictionary<string, string> Dictionary { get; set; }

            [NoTrim]
            public List<string> RawTexts { get; set; }

            [NoTrim]
            public NestedTestContract RawNested { get; set; }
        }

        public class NestedTestContract
        {
            public string Value { get; set; }
        }

        public record TestRecord(string Name, [property: NoTrim] string Raw);
    }
}
