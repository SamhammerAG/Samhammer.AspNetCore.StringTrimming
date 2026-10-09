## Samhammer.AspNetCore.StringTrimming

Trims leading and trailing whitespace of all string inputs of ASP.NET Core MVC, so controllers, validators and repositories don't have to deal with it.

| Input | Trimmed by |
| --- | --- |
| JSON body (`[FromBody]`) | serializer integration (System.Text.Json or Newtonsoft.Json) |
| Route, query, form (`[FromRoute]`, `[FromQuery]`, `[FromForm]`, complex form models) | model binder |
| Header (`[FromHeader]`), services, custom model binders | not trimmed |

Only reading is affected. Responses are serialized unchanged.

### Packages

| Package | Content | Use it for |
| --- | --- | --- |
| `Samhammer.AspNetCore.StringTrimming.SystemTextJson` | System.Text.Json integration | projects using System.Text.Json (ASP.NET Core default) |
| `Samhammer.AspNetCore.StringTrimming.NewtonsoftJson` | Newtonsoft.Json integration | projects using `AddNewtonsoftJson()` |
| `Samhammer.AspNetCore.StringTrimming` | common parts: `[NoTrim]`, model binder for route, query and form | referenced by both packages above, not installed directly |

The two serializer packages don't depend on each other. Install exactly one of them; the common package comes with it.

### How to use

System.Text.Json:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(...)
    .AddSystemTextJsonStringTrimming();
```

Newtonsoft.Json:

```csharp
builder.Services.AddControllers()
    .AddNewtonsoftJson(...)
    .AddNewtonsoftJsonStringTrimming();
```

Both are applied with `PostConfigure`, so the order relative to `AddJsonOptions()` / `AddNewtonsoftJson()` does not matter. Both register the model binder of the common package.

Without MVC (e.g. your own serializer settings):

```csharp
var options = new JsonSerializerOptions().AddStringTrimming();          // System.Text.Json
var settings = new JsonSerializerSettings().AddStringTrimming();        // Newtonsoft.Json
```

### Opt-out with `[NoTrim]`

Use `[NoTrim]` where whitespace is meaningful (markdown, templates, regex, secrets, signatures):

```csharp
public class MessageContract
{
    public string Title { get; set; }       // trimmed

    [NoTrim]
    public string Markdown { get; set; }    // kept as sent
}

public IActionResult Get([FromQuery][NoTrim] string pattern) { ... }

public record SignedLink(string Email, [property: NoTrim] string Signature);
```

| `[NoTrim]` on | System.Text.Json | Newtonsoft.Json | Model binder |
| --- | --- | --- | --- |
| string property / field | yes | yes | yes |
| string constructor parameter (via the property) | yes | yes | – |
| collection / dictionary of strings | yes | yes | collections only (e.g. `[FromQuery][NoTrim] List<string>`) |
| complex type (nothing inside is trimmed) | yes | no | – |
| action parameter | – | – | yes |

### Behavior

- `string.Trim()` is used: all Unicode whitespace incl. line breaks and NBSP; zero-width characters and BOM (U+FEFF) are not removed.
- JSON: `null` stays `null`, whitespace-only becomes `""`.
- Route, query, form: like the default binder, empty or whitespace-only values become `null` (`ConvertEmptyStringToNull`).
- Trimmed: properties, fields, constructor parameters, items of string collections, values of string dictionaries. Dictionary keys and property names are not trimmed.
- Not trimmed: strings inside `object`, `JToken`, `JsonElement` or `dynamic`, raw `Request.Body` reads, `IFormFile` content, headers.

#### System.Text.Json

- A global `JsonConverter<string>` trims every string, including root values (`[FromBody] string`) and items of any collection.
- `[NoTrim]` is applied by a type info modifier. If you replace `TypeInfoResolver` (e.g. source generated contexts) after the trimming was added, the modifier is lost.

#### Newtonsoft.Json

- Uses `TrimStringContractResolver` instead of a converter for `string`. A converter would change how Newtonsoft.Json reads strings (date-like strings would be parsed and reformatted, numbers would no longer be accepted for string properties). With the resolver, strings are read as usual and trimmed afterwards.
- The existing contract resolver is replaced, keeping its settings. Supported: `DefaultContractResolver` (ASP.NET Core default, camel case), `CamelCasePropertyNamesContractResolver`. A custom resolver must derive from `TrimStringContractResolver`, otherwise an `InvalidOperationException` is thrown.
- Supported string collections: `string[]`, `List<string>`, `IEnumerable<string>`, `IList<string>`, `ICollection<string>`, `IReadOnlyList<string>`, `IReadOnlyCollection<string>` and concrete `ICollection<string>` types with a default constructor (e.g. `HashSet<string>`). Dictionaries: `Dictionary<string, string>`, `IDictionary<string, string>`, `IReadOnlyDictionary<string, string>` and concrete types with a default constructor.
- Not trimmed: a string as root value (`[FromBody] string`).

### Things to check when introducing it

- Values that are hashed, signed or compared byte-exact (checksums, signatures, passwords, cache keys).
- Values where whitespace is meaningful (markdown, code, templates, regex, separators).
- Existing data is not migrated: values stored before keep their whitespace.
- Service-to-service calls go through the same pipeline and are trimmed as well.

### Build and release

One pipeline (`.github/workflows/cicd.yaml`) builds and tests all projects. Pushing a tag (e.g. `1.0.0`) publishes all three packages to NuGet with the tag as version, so the packages always share the same version.
