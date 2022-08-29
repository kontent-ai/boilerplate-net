using System.Text.Json.Serialization;

namespace Kontent.Ai.Boilerplate.CacheInvalidation;

internal record ChangeFeedResponseItem([property: JsonPropertyName("codename")]string Codename);
