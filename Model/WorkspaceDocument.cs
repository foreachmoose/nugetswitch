using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Model;

/// <summary>
/// Represents a workspace document
/// </summary>
public class WorkspaceDocument
{
    /// <summary>
    /// Gets or sets a value indicating whether this instance is dirty.
    /// </summary>
    /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    public bool IsDirty { get; set; }

    /// <summary>
    /// Gets or sets the selected libraries.
    /// </summary>
    /// <value>The selected libraries.</value>
    [JsonInclude]
    [JsonPropertyName("libraries")]
    private Dictionary<string, List<string>> SelectedLibraries { get; set; } = new();

    /// <summary>
    /// True if there are any selected libraries across all packages.
    /// </summary>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool AnySelectedLibraries()
    {
        // Check if any package has selected libraries
        return SelectedLibraries.Any(pair => pair.Value.Count > 0);
    }

    /// <summary>
    /// Adds the libraries.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="libraries">The libraries.</param>
    public void AddLocalReferences(string packageId, IEnumerable<string> libraries)
    {
        Guard.IsNotNullOrEmpty(packageId);
        Guard.IsNotNull(libraries);
        Guard.IsTrue(libraries.Any(), "No libraries to add");

        if (!SelectedLibraries.ContainsKey(packageId))
        {
            SelectedLibraries.Add(packageId, []);
        }

        List<string> list = SelectedLibraries[packageId];
        list.AddRange(libraries);

        IsDirty = true;
    }

    /// <summary>
    /// Removes the libraries.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="libraries">The libraries.</param>
    public void RemoveLibraries(string packageId, IEnumerable<string> libraries)
    {
        Guard.IsNotNullOrEmpty(packageId);
        Guard.IsNotNull(libraries);
        Guard.IsTrue(libraries.Any(), "No libraries to add");

        List<string> list = SelectedLibraries[packageId];
        foreach (string library in libraries)
        {
            list.Remove(library);
        }

        IsDirty = true;
    }

    /// <summary>
    /// Gets the selected libraries.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <returns>System.Collections.Generic.IList&lt;System.String&gt;.</returns>
    public IList<string>? GetSelectedLibraries(string packageId)
    {
        Guard.IsNotNullOrWhiteSpace(packageId);

        bool hasSelectedLibraries = SelectedLibraries.TryGetValue(packageId, out var selectedLibraries);

        return hasSelectedLibraries ? selectedLibraries : [];
    }
}