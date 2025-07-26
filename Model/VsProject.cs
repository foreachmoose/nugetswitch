using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Model;

/// <summary>
/// Represents a Visual Studio project
/// </summary>
public class VsProject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VsProject"/> class.
    /// </summary>
    /// <param name="projectName">Name of the project.</param>
    /// <param name="projectPath">The project path.</param>
    public VsProject(string projectName, string projectPath)
    {
        Guard.IsNotNull(projectName);
        Guard.IsNotNull(projectPath);

        ProjectName = projectName;
        ProjectPath = projectPath;
    }

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    /// <value>The name of the project.</value>
    public string ProjectName { get; }

    /// <summary>
    /// Gets the project path.
    /// </summary>
    /// <value>The project path.</value>
    public string ProjectPath { get; }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"{ProjectName} - {ProjectPath}";
    }

    /// <summary>
    /// Gets or sets the nu get packages.
    /// </summary>
    /// <value>The nu get packages.</value>
    public List<NuGetPackage> NuGetPackages { get; set; } = [];
}