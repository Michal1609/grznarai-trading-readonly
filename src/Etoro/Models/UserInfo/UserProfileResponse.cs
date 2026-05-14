using System.Text.Json.Serialization;

namespace GrznarAi.Trading.ReadOnly.Etoro.Models.UserInfo;

public record UserProfileResponse(
    [property: JsonPropertyName("users")] IReadOnlyList<UserProfile> Users
);

public record UserProfile(
    [property: JsonPropertyName("gcid")] int Gcid,
    [property: JsonPropertyName("realCID")] int RealCid,
    [property: JsonPropertyName("demoCID")] int? DemoCid,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("language")] int? Language,
    [property: JsonPropertyName("languageIsoCode")] string? LanguageIsoCode,
    [property: JsonPropertyName("country")] int? Country,
    [property: JsonPropertyName("allowDisplayFullName")] bool AllowDisplayFullName,
    [property: JsonPropertyName("userBio")] UserBio? UserBio,
    [property: JsonPropertyName("whiteLabel")] int? WhiteLabel,
    [property: JsonPropertyName("optOut")] bool OptOut,
    [property: JsonPropertyName("homepage")] int? Homepage,
    [property: JsonPropertyName("playerStatus")] int? PlayerStatus,
    [property: JsonPropertyName("piLevel")] int? PiLevel,
    [property: JsonPropertyName("isPi")] bool IsPi,
    [property: JsonPropertyName("avatars")] IReadOnlyList<UserAvatar>? Avatars,
    [property: JsonPropertyName("masterAccountCid")] int? MasterAccountCid,
    [property: JsonPropertyName("accountType")] int? AccountType,
    [property: JsonPropertyName("fundType")] string? FundType,
    [property: JsonPropertyName("isVerified")] bool IsVerified,
    [property: JsonPropertyName("verificationLevel")] int VerificationLevel,
    [property: JsonPropertyName("accountStatus")] int? AccountStatus,
    [property: JsonPropertyName("gdprInfo")] UserGdprInfo? GdprInfo,
    [property: JsonPropertyName("firstName")] string? FirstName,
    [property: JsonPropertyName("middleName")] string? MiddleName,
    [property: JsonPropertyName("lastName")] string? LastName,
    [property: JsonPropertyName("aboutMe")] string? AboutMe,
    [property: JsonPropertyName("aboutMeShort")] string? AboutMeShort,
    [property: JsonPropertyName("customerRestrictions")] IReadOnlyList<CustomerRestriction>? CustomerRestrictions,
    [property: JsonPropertyName("userFlowSignature")] string? UserFlowSignature
);

public record UserBio(
    [property: JsonPropertyName("gcid")] int Gcid,
    [property: JsonPropertyName("languageCode")] string? LanguageCode,
    [property: JsonPropertyName("aboutMe")] string? AboutMe,
    [property: JsonPropertyName("aboutMeShort")] string? AboutMeShort,
    [property: JsonPropertyName("strategyID")] int? StrategyId
);

public record UserAvatar(
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("type")] string? Type
);

public record UserGdprInfo(
    [property: JsonPropertyName("accountStatus")] int? AccountStatus,
    [property: JsonPropertyName("playerStatus")] int? PlayerStatus,
    [property: JsonPropertyName("playerStatusReason")] int? PlayerStatusReason
);

public record CustomerRestriction(
    [property: JsonPropertyName("CID")] int Cid,
    [property: JsonPropertyName("restrictionTypeID")] int RestrictionTypeId,
    [property: JsonPropertyName("reasonID")] int ReasonId,
    [property: JsonPropertyName("occured")] DateTimeOffset? Occurred
);
