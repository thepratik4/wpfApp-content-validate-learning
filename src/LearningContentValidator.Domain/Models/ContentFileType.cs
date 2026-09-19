namespace LearningContentValidator.Domain.Models;

/// <summary>
/// Categorization of supported content file types in learning packages.
/// In C#, an enum is a strongly typed value type backed by an underlying integral type (Int32 by default).
/// Conceptually similar to C++11 'enum class', preventing implicit integral conversion.
/// </summary>
public enum ContentFileType
{
    Unknown = 0,
    Pdf = 1,
    Html = 2,
    Json = 3,
    Image = 4,
    Video = 5,
    Zip = 6
}

