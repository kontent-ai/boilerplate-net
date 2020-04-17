using System;
using System.Text.Json.Serialization;

namespace Kentico.Kontent.AspNetCore.Webhooks.Models
{
    public class WebhookModel
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        [JsonPropertyName("api_name")]
        public string ApiName { get; set; }

        [JsonPropertyName("project_id")]
        public Guid ProjectId { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("items")]
        public Item[] Items { get; set; }

        [JsonPropertyName("taxonomies")]
        public Taxonomy[] Taxonomies { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("codename")]
        public string Codename { get; set; }
    }

    public class Taxonomy
    {
        [JsonPropertyName("codename")]
        public string Codename { get; set; }
    }
}
