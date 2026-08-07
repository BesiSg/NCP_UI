using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Utility.Lib.Ticket
{
    public class JiraSearchResult
    {
        [JsonPropertyName("issues")]
        public List<Issue> Issues { get; set; }
        [JsonPropertyName("nextPageToken")]
        public string nextPageToken { get; set; }
        [JsonPropertyName("isLast")]
        public bool isLast { get; set; }
    }


#nullable enable

    public class Issue : CustomFieldKeyId
    {
        [JsonPropertyName("expand")]
        public string? Expand { get; set; }

        [JsonPropertyName("fields")]
        public IssueFields? Fields { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class IssueFields
    {
        [JsonPropertyName("statuscategorychangedate")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? StatusCategoryChangeDate { get; set; }

        [JsonPropertyName("parent")]
        public ParentIssue? Parent { get; set; }

        [JsonPropertyName("priority")]
        public Priority? Priority { get; set; }

        [JsonPropertyName("assignee")]
        public JiraUser? Assignee { get; set; }

        [JsonPropertyName("status")]
        public Status? Status { get; set; }

        [JsonPropertyName("issuetype")]
        public IssueType? IssueType { get; set; }

        [JsonPropertyName("project")]
        public Project? Project { get; set; }

        [JsonPropertyName("resolution")]
        public Resolution? Resolution { get; set; }

        //[JsonPropertyName("statusCategory")]
        //public StatusCategory? StatusCategory { get; set; }

        [JsonPropertyName("components")]
        public List<Component>? Components { get; set; }

        [JsonPropertyName("issuelinks")]
        public List<IssueLink>? IssueLinks { get; set; }

        [JsonPropertyName("creator")]
        public JiraUser? Creator { get; set; }

        [JsonPropertyName("reporter")]
        public JiraUser? Reporter { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("description")]
        public AdfDoc? Description { get; set; }

        [JsonPropertyName("comment")]
        public CommentContainer? Comment { get; set; }

        [JsonPropertyName("created")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? Created { get; set; }

        [JsonPropertyName("updated")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? Updated { get; set; }

        [JsonPropertyName("resolutiondate")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? ResolutionDate { get; set; }


        // -------------------------
        // Customfields (non-null in your sample)
        // -------------------------

        // customfield_10339: { self, value, id }
        [JsonPropertyName("customfield_10339")]
        public CustomFieldValueId? Site { get; set; }

        // customfield_10291: { value, id }
        [JsonPropertyName("customfield_10291")]
        public CustomFieldValueId? MachineSN { get; set; }

        // customfield_10280: "V7.4.0.39"
        [JsonPropertyName("customfield_10280")]
        public string? SWVersion { get; set; }

        // customfield_10423: { self, value, id }
        [JsonPropertyName("customfield_10423")]
        public CustomFieldValueId? PkgDepartment { get; set; }

        // customfield_10424: { value, id }
        [JsonPropertyName("customfield_10424")]
        public CustomFieldValueId? MachineProductLine { get; set; }

        // customfield_10428: { value, id }
        [JsonPropertyName("customfield_10428")]
        public CustomFieldValueId? MachineCustomer { get; set; }


        [JsonPropertyName("attachment")]
        public List<JiraAttachment>? Attachment { get; set; }


        // Keep all other unmodeled fields (including the many null customfields)
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalFields { get; set; }

        public object? this[string key] => GetValue(key);

        private object? GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            // Normalize: allow "Priority" or "priority"
            var normalized = key.Trim();

            // 1) Try match against JsonPropertyName attributes first ("priority", "customfield_10280", etc.)
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var p in props)
            {
                var attr = p.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
                if (attr != null && string.Equals(attr.Name, normalized, StringComparison.OrdinalIgnoreCase))
                    return p.GetValue(this);
            }

            // 2) Try match against C# property names ("Priority", "SWVersion", etc.)
            var byName = GetType().GetProperty(normalized, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (byName != null)
                return byName.GetValue(this);

            // 3) Try extension data bag (unmodeled fields and null customfields)
            if (AdditionalFields != null)
            {
                // exact key
                if (AdditionalFields.TryGetValue(normalized, out var je))
                    return je;

                // case-insensitive lookup
                foreach (var kv in AdditionalFields)
                {
                    if (string.Equals(kv.Key, normalized, StringComparison.OrdinalIgnoreCase))
                        return kv.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Strong helper to get typed values back from JsonElement extension data.
        /// </summary>
        public T? GetAs<T>(string key, JsonSerializerOptions? options = null)
        {
            var v = GetValue(key);

            if (v is T t)
                return t;

            if (v is JsonElement je)
            {
                try
                {
                    return je.Deserialize<T>(options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web));
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }

    }

    public class JiraAttachment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }
    }

    // -------------------------
    // Small helper types for customfields
    // -------------------------

    public class CustomFieldValueId
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class CustomFieldNameId
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class CustomFieldKeyId
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
    //public class CustomFieldOptionWithSelf : CustomFieldValueId
    //{
    //    [JsonPropertyName("self")]
    //    public string? Self { get; set; }
    //}

    // -------------------------
    // Jira common types (minimal but sufficient for your JSON)
    // -------------------------

    public class ParentIssue : CustomFieldKeyId
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("fields")]
        public ParentIssueFields? Fields { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class ParentIssueFields
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("status")]
        public Status? Status { get; set; }

        [JsonPropertyName("priority")]
        public Priority? Priority { get; set; }

        [JsonPropertyName("issuetype")]
        public IssueType? IssueType { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class IssueLink
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("type")]
        public IssueLinkType? Type { get; set; }

        [JsonPropertyName("inwardIssue")]
        public LinkedIssue? InwardIssue { get; set; }

        [JsonPropertyName("outwardIssue")]
        public LinkedIssue? OutwardIssue { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class IssueLinkType : CustomFieldNameId
    {
        [JsonPropertyName("inward")]
        public string? Inward { get; set; }

        [JsonPropertyName("outward")]
        public string? Outward { get; set; }

        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class LinkedIssue : CustomFieldKeyId
    {
        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        [JsonPropertyName("fields")]
        public LinkedIssueFields? Fields { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class LinkedIssueFields
    {
        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("status")]
        public Status? Status { get; set; }

        [JsonPropertyName("priority")]
        public Priority? Priority { get; set; }

        [JsonPropertyName("issuetype")]
        public IssueType? IssueType { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class JiraUser
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; }

        [JsonPropertyName("emailAddress")]
        public string? EmailAddress { get; set; }

        //[JsonPropertyName("avatarUrls")]
        //public AvatarUrls? AvatarUrls { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("active")]
        public bool? Active { get; set; }

        [JsonPropertyName("timeZone")]
        public string? TimeZone { get; set; }

        [JsonPropertyName("accountType")]
        public string? AccountType { get; set; }

        //[JsonExtensionData]
        //public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    //public class AvatarUrls
    //{
    //    [JsonPropertyName("48x48")]
    //    public string? Size48 { get; set; }

    //    [JsonPropertyName("32x32")]
    //    public string? Size32 { get; set; }

    //    [JsonPropertyName("24x24")]
    //    public string? Size24 { get; set; }

    //    [JsonPropertyName("16x16")]
    //    public string? Size16 { get; set; }
    //}

    public class Status : CustomFieldNameId
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }

        //[JsonPropertyName("statusCategory")]
        //public StatusCategory? StatusCategory { get; set; }
    }

    public class StatusCategory
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("colorName")]
        public string? ColorName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Priority : CustomFieldNameId
    {
        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }

    public class Resolution : CustomFieldNameId
    {
        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class Component : CustomFieldNameId
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }
    }

    public class IssueType : CustomFieldNameId
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }

        [JsonPropertyName("subtask")]
        public bool? Subtask { get; set; }

        [JsonPropertyName("avatarId")]
        public int? AvatarId { get; set; }

        [JsonPropertyName("hierarchyLevel")]
        public int? HierarchyLevel { get; set; }
    }

    public class Project : CustomFieldNameId
    {
        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("projectTypeKey")]
        public string? ProjectTypeKey { get; set; }

        [JsonPropertyName("simplified")]
        public bool? Simplified { get; set; }

        //[JsonPropertyName("avatarUrls")]
        //public AvatarUrls? AvatarUrls { get; set; }

        [JsonPropertyName("projectCategory")]
        public ProjectCategory? ProjectCategory { get; set; }
    }

    public class ProjectCategory : CustomFieldNameId
    {
        //[JsonPropertyName("self")]
        //public string? Self { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class CommentContainer
    {
        [JsonPropertyName("comments")]
        public List<Comment>? Comments { get; set; }

        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("maxResults")]
        public int? MaxResults { get; set; }

        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("startAt")]
        public int? StartAt { get; set; }
    }

    public class Comment
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("author")]
        public JiraUser? Author { get; set; }

        [JsonPropertyName("updateAuthor")]
        public JiraUser? UpdateAuthor { get; set; }

        [JsonPropertyName("body")]
        public AdfDoc? Body { get; set; }

        [JsonPropertyName("created")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? Created { get; set; }

        [JsonPropertyName("updated")]
        [JsonConverter(typeof(JiraDateTimeConverter))]
        public DateTime? Updated { get; set; }

        [JsonPropertyName("jsdPublic")]
        public bool? JsdPublic { get; set; }
    }

    public sealed class AdfDoc
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }  // "doc"

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("content")]
        public List<AdfNode> Content { get; set; }
    }

    public sealed class AdfNode
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }  // "paragraph", "text", "hardBreak", etc.

        [JsonPropertyName("text")]
        public string Text { get; set; }  // only for "text" nodes

        [JsonPropertyName("content")]
        public List<AdfNode> Content { get; set; }  // children
    }



    /// <summary>
    /// Jira timestamps look like: "2026-04-27T00:59:45.087+0000" (no colon in timezone).
    /// This converter normalizes "+0000" => "+00:00" so DateTime can parse.
    /// </summary>
    public sealed class JiraDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Expected string for DateTime, got {reader.TokenType}");

            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return null;

            // Normalize timezone like +0000 / -0530 => +00:00 / -05:30
            if (s.Length >= 5)
            {
                var tz = s[^5..];
                if ((tz[0] == '+' || tz[0] == '-') &&
                    char.IsDigit(tz[1]) && char.IsDigit(tz[2]) && char.IsDigit(tz[3]) && char.IsDigit(tz[4]))
                {
                    s = s.Insert(s.Length - 2, ":");
                }
            }

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
                return dto;

            throw new JsonException($"Unable to parse Jira DateTime: '{s}'");
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}
