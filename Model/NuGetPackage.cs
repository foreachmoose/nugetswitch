using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Model;

/// <summary>
/// Represents a NuGet package reference
/// </summary>
public class NuGetPackage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetPackage"/> class.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="version">The version.</param>
    public NuGetPackage(string packageId, string version)
    {
        Guard.IsNotNullOrWhiteSpace(packageId);
        Guard.IsNotNullOrWhiteSpace(version);

        PackageId = packageId;
        Version = version;
    }

    /// <summary>
    /// Gets the package identifier.
    /// </summary>
    /// <value>The package identifier.</value>
    public string PackageId { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    /// <value>The version.</value>
    public string Version { get; }
}