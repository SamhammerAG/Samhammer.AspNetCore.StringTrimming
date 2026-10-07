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

### Opt-out

Use it where whitespace is meaningful (markdown, templates, regex, secrets, signatures).

| Where | System.Text.Json | Newtonsoft.Json |
| --- | --- | --- |
| JSON property | `[NoTrim]` | `[JsonConverter(typeof(NoTrimStringJsonConverter))]` |
| route, query, form parameter or property | `[NoTrim]` | `[NoTrim]` |

```csharp
public class MessageContract
{
    public string Title { get; set; }       // trimmed

    [NoTrim]                                                // System.Text.Json
    [JsonConverter(typeof(NoTrimStringJsonConverter))]      // Newtonsoft.Json
    public string Markdown { get; set; }    // kept as sent
}

public IActionResult Get([FromQuery][NoTrim] string pattern) { ... }
```

With System.Text.Json `[NoTrim]` also works on string collections, dictionaries and complex types (nothing inside is trimmed) and on records (`[property: NoTrim]`).

### Behavior

- `string.Trim()` is used: all Unicode whitespace incl. line breaks and NBSP; zero-width characters and BOM (U+FEFF) are not removed.
- JSON: `null` stays `null`, whitespace-only becomes `""`.
- Route, query, form: like the default binder, empty or whitespace-only values become `null` (`ConvertEmptyStringToNull`).
- Trimmed: string properties, fields, constructor parameters, items of string collections, values of string dictionaries, root strings. Dictionary keys and property names are not trimmed.
- Not trimmed: strings inside `object`, `JToken`, `JsonElement` or `dynamic`, raw `Request.Body` reads, `IFormFile` content, headers.

#### System.Text.Json

- A global `JsonConverter<string>` trims every string, including root values (`[FromBody] string`) and items of any collection.
- `[NoTrim]` is applied by a type info modifier. If you replace `TypeInfoResolver` (e.g. source generated contexts) after the trimming was added, the modifier is lost.

#### Newtonsoft.Json

- A global `JsonConverter` for `string` trims every string, including root values (`[FromBody] string`), collection items and dictionary values (same approach as FUTO).
- Known limitation: with a converter for `string`, Newtonsoft.Json passes the raw token to the converter. Numbers, booleans and date-like strings (e.g. `"2026-10-01T10:00:00"`, default `DateParseHandling.DateTime`) sent for a string property are rejected with a `JsonSerializationException` (400). Without the package they would be accepted as strings.

### Things to check when introducing it

- Values that are hashed, signed or compared byte-exact (checksums, signatures, passwords, cache keys).
- Values where whitespace is meaningful (markdown, code, templates, regex, separators).
- Existing data is not migrated: values stored before keep their whitespace.
- Service-to-service calls go through the same pipeline and are trimmed as well.

### Build and release

One pipeline (`.github/workflows/cicd.yaml`) builds and tests all projects. Pushing a tag (e.g. `1.0.0`) publishes all three packages to NuGet with the tag as version, so the packages always share the same version.
