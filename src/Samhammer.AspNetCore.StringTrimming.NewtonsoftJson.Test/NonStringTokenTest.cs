using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Samhammer.AspNetCore.StringTrimming.NewtonsoftJson.Test
{
    /// <summary>
    /// Numbers, booleans and date-like strings sent to string members must behave like default Newtonsoft.Json (no 400),
    /// only leading and trailing whitespace is trimmed. Each test compares against Newtonsoft.Json without string trimming.
    /// </summary>
    public class NonStringTokenTest
    {
        private readonly JsonSerializerSettings defaultSettings = CreateSettings();

        private readonly JsonSerializerSettings settings = CreateSettings().AddStringTrimming();

        [Theory]
        [InlineData("123", "123")]
        [InlineData("-1", "-1")]
        [InlineData("0", "0")]
        [InlineData("1.50", "1.50")]
        [InlineData("true", "true")]
        [InlineData("false", "false")]
        [InlineData("\"2026-10-01T10:00:00\"", "2026-10-01T10:00:00")]
        [InlineData("\"2026-10-01T10:00:00Z\"", "2026-10-01T10:00:00Z")]
        [InlineData("\"2026-10-01T10:00:00+02:00\"", "2026-10-01T10:00:00+02:00")]
        [InlineData("\"2026-10-01T10:00:00.1234567+02:00\"", "2026-10-01T10:00:00.1234567+02:00")]
        [InlineData("\"2026-10-01\"", "2026-10-01")]
        [InlineData("\"\\/Date(1790841600000)\\/\"", "/Date(1790841600000)/")]
        [InlineData("\"true\"", "true")]
        [InlineData("\"123\"", "123")]
        public void StringProperty_KeepsOriginalText(string token, string expected)
        {
            var json = $"{{ \"name\": {token} }}";

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Name.Should().Be(expected);
            result.Name.Should().Be(JsonConvert.DeserializeObject<TestContract>(json, defaultSettings).Name);
        }

        [Theory]
        [InlineData("\"  2026-10-01T10:00:00  \"", "2026-10-01T10:00:00")]
        [InlineData("\"\\t2026-10-01T10:00:00+02:00\\r\\n\"", "2026-10-01T10:00:00+02:00")]
        [InlineData("\" 123 \"", "123")]
        [InlineData("\" true \"", "true")]
        public void StringProperty_PaddedValues_AreTrimmedButNotReformatted(string token, string expected)
        {
            var result = JsonConvert.DeserializeObject<TestContract>($"{{ \"name\": {token} }}", settings);

            result.Name.Should().Be(expected);
        }

        [Fact]
        public void NestedStringProperty_KeepsOriginalText()
        {
            var result = JsonConvert.DeserializeObject<TestContract>("{ \"nested\": { \"value\": 1.50 }, \"items\": [ { \"value\": true }, { \"value\": \" 2026-10-01T10:00:00 \" } ] }", settings);

            result.Nested.Value.Should().Be("1.50");
            result.Items.Select(i => i.Value).Should().Equal("true", "2026-10-01T10:00:00");
        }

        [Fact]
        public void NoTrimProperty_KeepsOriginalTextAndWhitespace()
        {
            var result = JsonConvert.DeserializeObject<TestContract>("{ \"raw\": \" 2026-10-01T10:00:00 \", \"rawTexts\": [ 123, \" 2026-10-01 \" ] }", settings);

            result.Raw.Should().Be(" 2026-10-01T10:00:00 ");
            result.RawTexts.Should().Equal("123", " 2026-10-01 ");
        }

        [Fact]
        public void StringCollections_KeepOriginalText()
        {
            const string values = "[ 123, 1.50, true, \"2026-10-01T10:00:00+02:00\", \" 2026-10-01 \", null ]";
            var json = $"{{ \"texts\": {values}, \"array\": {values}, \"enumerable\": {values}, \"readOnlyList\": {values}, \"hashSet\": {values} }}";

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            string[] expected = ["123", "1.50", "true", "2026-10-01T10:00:00+02:00", "2026-10-01", null];
            result.Texts.Should().Equal(expected);
            result.Array.Should().Equal(expected);
            result.Enumerable.Should().Equal(expected);
            result.ReadOnlyList.Should().Equal(expected);
            result.HashSet.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void StringCollections_BehaveLikeDefaultNewtonsoft()
        {
            var json = "{ \"texts\": [ 123, 1.50, true, \"2026-10-01T10:00:00+02:00\", \"2026-10-01\", null ] }";

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Texts.Should().Equal(JsonConvert.DeserializeObject<TestContract>(json, defaultSettings).Texts);
        }

        [Fact]
        public void StringDictionaries_KeepOriginalText()
        {
            const string values = "{ \"int\": 123, \"float\": 1.50, \"bool\": false, \"date\": \" 2026-10-01T10:00:00Z \", \"null\": null }";

            var result = JsonConvert.DeserializeObject<TestContract>($"{{ \"dictionary\": {values}, \"readOnlyDictionary\": {values} }}", settings);

            var expected = new Dictionary<string, string>
            {
                { "int", "123" },
                { "float", "1.50" },
                { "bool", "false" },
                { "date", "2026-10-01T10:00:00Z" },
                { "null", null },
            };
            result.Dictionary.Should().Equal(expected);
            result.ReadOnlyDictionary.Should().Equal(expected);
        }

        [Fact]
        public void StringDictionaries_BehaveLikeDefaultNewtonsoft()
        {
            var json = "{ \"dictionary\": { \"int\": 123, \"float\": 1.50, \"bool\": false, \"date\": \"2026-10-01T10:00:00Z\" } }";

            var result = JsonConvert.DeserializeObject<TestContract>(json, settings);

            result.Dictionary.Should().Equal(JsonConvert.DeserializeObject<TestContract>(json, defaultSettings).Dictionary);
        }

        [Fact]
        public void ConstructorParameters_KeepOriginalText()
        {
            var result = JsonConvert.DeserializeObject<TestRecord>("{ \"name\": \" 2026-10-01T10:00:00 \", \"code\": 1.50, \"flag\": true }", settings);

            result.Name.Should().Be("2026-10-01T10:00:00");
            result.Code.Should().Be("1.50");
            result.Flag.Should().Be("true");
        }

        [Theory]
        [InlineData("{ \"name\": {} }")]
        [InlineData("{ \"name\": [] }")]
        [InlineData("{ \"texts\": [ { \"a\": 1 } ] }")]
        [InlineData("{ \"texts\": [ [ \"a\" ] ] }")]
        [InlineData("{ \"dictionary\": { \"k\": { \"a\": 1 } } }")]
        public void ObjectOrArrayForString_ThrowsLikeDefaultNewtonsoft(string json)
        {
            var defaultAct = () => JsonConvert.DeserializeObject<TestContract>(json, defaultSettings);
            var act = () => JsonConvert.DeserializeObject<TestContract>(json, settings);

            defaultAct.Should().Throw<JsonException>();
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void ObjectAndJTokenMembers_KeepDefaultDateParsing()
        {
            var json = "{ \"data\": \"2026-10-01T10:00:00\", \"token\": { \"date\": \"2026-10-01T10:00:00\" } }";
            var expected = JsonConvert.DeserializeObject<UntypedTestContract>(json, defaultSettings);

            var result = JsonConvert.DeserializeObject<UntypedTestContract>(json, settings);

            result.Data.Should().BeOfType<DateTime>().And.Be(expected.Data);
            result.Token["date"].Type.Should().Be(JTokenType.Date).And.Be(expected.Token["date"].Type);
        }

        [Fact]
        public void TypedDateMembers_AreNotAffected()
        {
            var result = JsonConvert.DeserializeObject<UntypedTestContract>("{ \"date\": \"2026-10-01T10:00:00+02:00\", \"number\": 1.50, \"flag\": true }", settings);

            result.Date.Should().Be(new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.FromHours(2)));
            result.Number.Should().Be(1.50m);
            result.Flag.Should().BeTrue();
        }

        [Fact]
        public void CustomDateAndFloatHandling_StringsKeepOriginalText()
        {
            var customSettings = CreateSettings();
            customSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            customSettings.FloatParseHandling = FloatParseHandling.Decimal;
            customSettings.AddStringTrimming();

            var result = JsonConvert.DeserializeObject<TestContract>("{ \"name\": \" 2026-10-01T10:00:00+02:00 \", \"texts\": [ 1.50, \"2026-10-01\" ] }", customSettings);

            result.Name.Should().Be("2026-10-01T10:00:00+02:00");
            result.Texts.Should().Equal("1.50", "2026-10-01");
        }

        // JToken.ToObject reads from already parsed tokens (dates are parsed by JObject.Parse), the result must match default Newtonsoft.Json
        [Fact]
        public void JTokenToObject_BehavesLikeDefaultNewtonsoft()
        {
            var token = JObject.Parse("{ \"name\": \"2026-10-01T10:00:00\", \"texts\": [ 123, true, 1.50 ], \"dictionary\": { \"k\": false } }");
            var expected = token.ToObject<TestContract>(JsonSerializer.Create(defaultSettings));

            var result = token.ToObject<TestContract>(JsonSerializer.Create(settings));

            result.Name.Should().Be(expected.Name);
            result.Texts.Should().Equal(expected.Texts);
            result.Dictionary.Should().Equal(expected.Dictionary);
        }

        [Fact]
        public void MvcRegisteredSettings_KeepOriginalText()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddControllers().AddNewtonsoftJson().AddNewtonsoftJsonStringTrimming();
            using var provider = services.BuildServiceProvider();
            var mvcSettings = provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;

            var result = JsonConvert.DeserializeObject<TestContract>("{ \"name\": \" 2026-10-01T10:00:00 \", \"texts\": [ 123, true ], \"dictionary\": { \"k\": 1.50 } }", mvcSettings);

            result.Name.Should().Be("2026-10-01T10:00:00");
            result.Texts.Should().Equal("123", "true");
            result.Dictionary["k"].Should().Be("1.50");
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            };
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

            public Dictionary<string, string> Dictionary { get; set; }

            public IReadOnlyDictionary<string, string> ReadOnlyDictionary { get; set; }

            [NoTrim]
            public List<string> RawTexts { get; set; }
        }

        public class NestedTestContract
        {
            public string Value { get; set; }
        }

        public class UntypedTestContract
        {
            public object Data { get; set; }

            public JObject Token { get; set; }

            public DateTimeOffset Date { get; set; }

            public decimal Number { get; set; }

            public bool Flag { get; set; }
        }

        public record TestRecord(string Name, string Code, string Flag);
    }
}
