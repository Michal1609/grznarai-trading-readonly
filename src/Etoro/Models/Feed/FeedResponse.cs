using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.Feed;

public record FeedPaging(
    [property: JsonPropertyName("next")] string? Next,
    [property: JsonPropertyName("offSet")] int Offset,
    [property: JsonPropertyName("take")] int Take,
    [property: JsonPropertyName("version")] string? Version
);

public record FeedMetadata(
    [property: JsonPropertyName("experimentName")] string? ExperimentName,
    [property: JsonPropertyName("streamType")] string? StreamType,
    [property: JsonPropertyName("designatedStreamType")] string? DesignatedStreamType
);

public record FeedOwnerAvatar(
    [property: JsonPropertyName("small")] string? Small,
    [property: JsonPropertyName("medium")] string? Medium,
    [property: JsonPropertyName("large")] string? Large
);

public record FeedOwner(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("fullname")] string? Fullname,
    [property: JsonPropertyName("avatar")] FeedOwnerAvatar? Avatar,
    [property: JsonPropertyName("isFollowing")] bool? IsFollowing
);

public record FeedMessage(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("languageCode")] string? LanguageCode
);

public record AttachmentMedia(
    [property: JsonPropertyName("width")] int? Width,
    [property: JsonPropertyName("height")] int? Height,
    [property: JsonPropertyName("duration")] int? Duration
);

public record FeedAttachment(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("thumbnailUrl")] string? ThumbnailUrl,
    [property: JsonPropertyName("host")] string? Host,
    [property: JsonPropertyName("mediaType")] string? MediaType,
    [property: JsonPropertyName("media")] AttachmentMedia? Media,
    [property: JsonPropertyName("metadata")] AttachmentMedia? Metadata
);

public record FeedTag(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("displayName")] string? DisplayName
);

public record FeedMention(
    [property: JsonPropertyName("userId")] int? UserId,
    [property: JsonPropertyName("username")] string? Username
);

public record FeedPostMetadata(
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("instrumentId")] int? InstrumentId,
    [property: JsonPropertyName("symbolName")] string? SymbolName
);

public record DiscussionsPost(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("owner")] FeedOwner? Owner,
    [property: JsonPropertyName("message")] FeedMessage? Message,
    [property: JsonPropertyName("created")] DateTimeOffset? Created,
    [property: JsonPropertyName("updated")] DateTimeOffset? Updated,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("isDeleted")] bool IsDeleted,
    [property: JsonPropertyName("isSpam")] bool IsSpam,
    [property: JsonPropertyName("editStatus")] string? EditStatus,
    [property: JsonPropertyName("attachments")] IReadOnlyList<FeedAttachment>? Attachments,
    [property: JsonPropertyName("tags")] IReadOnlyList<FeedTag>? Tags,
    [property: JsonPropertyName("mentions")] IReadOnlyList<FeedMention>? Mentions,
    [property: JsonPropertyName("metadata")] FeedPostMetadata? Metadata
);

public record EmotionsData(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("summary")] IReadOnlyDictionary<string, int>? Summary,
    [property: JsonPropertyName("requesterEmotion")] string? RequesterEmotion
);

public record CommentsData(
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("comments")] IReadOnlyList<Discussion>? Comments
);

public record RequesterContext(
    [property: JsonPropertyName("isLiked")] bool? IsLiked,
    [property: JsonPropertyName("isBookmarked")] bool? IsBookmarked,
    [property: JsonPropertyName("isFollowing")] bool? IsFollowing
);

public record DiscussionSummary(
    [property: JsonPropertyName("likesCount")] int? LikesCount,
    [property: JsonPropertyName("commentsCount")] int? CommentsCount,
    [property: JsonPropertyName("sharesCount")] int? SharesCount,
    [property: JsonPropertyName("viewsCount")] int? ViewsCount
);

public record Discussion(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("post")] DiscussionsPost? Post,
    [property: JsonPropertyName("commentsData")] CommentsData? CommentsData,
    [property: JsonPropertyName("emotionsData")] EmotionsData? EmotionsData,
    [property: JsonPropertyName("requesterContext")] RequesterContext? RequesterContext,
    [property: JsonPropertyName("summary")] DiscussionSummary? Summary,
    [property: JsonPropertyName("reason")] string? Reason
);

public record DiscussionsResponse(
    [property: JsonPropertyName("discussions")] IReadOnlyList<Discussion> Discussions,
    [property: JsonPropertyName("paging")] FeedPaging? Paging,
    [property: JsonPropertyName("metadata")] FeedMetadata? Metadata
);
