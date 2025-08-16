using CommunityToolkit.Diagnostics;
using NuGetSwitch.Helper;
using System.IO;

namespace NuGetSwitch.Model;

/// <summary>
/// Represents a Visual Studio Solution
/// </summary>
public class VsSolution
{
    private List<VsProject> m_projects = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="VsSolution" /> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public VsSolution(string filePath)
    {
        Guard.IsNotNullOrWhiteSpace(filePath);

        string? solutionFolder = Path.GetDirectoryName(filePath);

        Guard.IsNotNull(solutionFolder);

        FilePath = filePath;
        SolutionFolder = solutionFolder;
    }

    /// <summary>
    /// Gets the file path.
    /// </summary>
    /// <value>The file path.</value>
    public string FilePath { get; }

    /// <summary>
    /// Gets the solution folder.
    /// </summary>
    /// <value>The solution folder.</value>
    public string SolutionFolder { get; }

    /// <summary>
    /// Loads this instance.
    /// </summary>
    public void Load()
    {
        NuGetPackageIds.Clear();

        // Read VS projects from the solution file
        m_projects = VsSolutionFileHelper.GetProjectsFromSolution(FilePath);

        foreach (VsProject project in m_projects)
        {
            string fullPath = Path.Combine(SolutionFolder, project.ProjectPath);

            List<NuGetPackage> packages = VsProjectFileHelper.GetNuGetPackagesFromProject(fullPath);

            foreach (NuGetPackage package in packages)
            {
                project.NuGetPackages.Add(package);

                if (!NuGetPackageIds.Contains(package.PackageId))
                {
                    NuGetPackageIds.Add(package.PackageId);
                }
            }
        }
    }

    /// <summary>
    /// Deletes the object folders.
    /// </summary>
    /// <returns>System.Threading.Tasks.Task&lt;System.Collections.Generic.List&lt;System.String&gt;&gt;.</returns>
    public async Task<List<string>> DeleteObjFolders()
    {
        List<Task> deleteTasks = [];
        List<string> messages = [];

        foreach (VsProject project in m_projects)
        {
            string projectPath = Path.Combine(SolutionFolder, project.ProjectPath);
            string objFolder = Path.Combine(Path.GetDirectoryName(projectPath) ?? "", "obj");
            if (!Directory.Exists(objFolder))
                continue;

            Task task = new Task(() =>
            {
                Directory.Delete(objFolder, true);
                messages.Add($"Deleted: {objFolder}");
            });
            deleteTasks.Add(task);
            task.Start();
        }

        await Task.WhenAll(deleteTasks);

        if (!messages.Any())
        {
            messages.Add("No obj folders found to delete.");
        }

        return messages;
    }

    /// <summary>
    /// Gets the projects.
    /// </summary>
    /// <value>The projects.</value>
    public IEnumerable<VsProject> Projects => m_projects;

    /// <summary>
    /// Gets the list of NuGet package IDs
    /// </summary>
    /// <value>The nu get package ids.</value>
    public List<string> NuGetPackageIds { get; } = [];


    /// <summary>
    /// Switches the specified workspace document.
    /// </summary>
    /// <param name="workspaceDocument">The workspace document.</param>
    /// <returns>System.Threading.Tasks.Task&lt;System.Collections.Generic.List&lt;System.String&gt;&gt;.</returns>
    public async Task<List<string>> Switch(WorkspaceDocument workspaceDocument)
    {
        Guard.IsNotNull(workspaceDocument);

        List<string> messages = [];

        await Task.Run(() =>
        {

            // Loop through all projects and update the NuGet packages
            foreach (VsProject project in Projects)
            {

                string fullPath = Path.Combine(SolutionFolder, project.ProjectPath);

                messages.Add($"Updating project: {fullPath}");

                // Update each nuget in the project
                foreach (NuGetPackage package in project.NuGetPackages)
                {
                    IList<string>? selectedLibraries = workspaceDocument.GetSelectedLibraries(package.PackageId);

                    if (selectedLibraries == null || selectedLibraries.Count == 0)
                    {
                        messages.Add($"\tSkipping: {package.PackageId}, no dlls selected");
                        continue;
                    }

                    messages.Add($"\tUpdating: {package.PackageId}");

                    // Remove existing PackageReference elements for this package
                    messages.Add($"\t\tRemove package: {package.PackageId}");
                    VsProjectFileHelper.RemoveNuGetPackageReference(fullPath, package.PackageId);

                    List<(string, string)> references = [];

                    foreach (string libPath in selectedLibraries)
                    {
                        string? basePath = Path.GetDirectoryName(fullPath);
                        if (basePath == null)
                            throw new InvalidOperationException("Invalid base path");
                        string relPath = PathHelper.GetRelativePath(libPath, basePath);
                        string name = Path.GetFileNameWithoutExtension(relPath);

                        messages.Add($"\t\tAdd reference: {relPath}");

                        references.Add((name, relPath));
                    }

                    VsProjectFileHelper.AddDllReferences(fullPath, references);
                }
            }
        });

        return messages;
    }
}