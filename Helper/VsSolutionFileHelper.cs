using NuGetSwitch.Model;
using System.IO;
using System.Text.RegularExpressions;
using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Helper;

/// <summary>
/// Helper class for parsing Visual Studio solution file
/// </summary>
public static class VsSolutionFileHelper
{
    /// <summary>
    /// Gets the projects from solution.
    /// </summary>
    /// <param name="solutionFilePath">The solution file path.</param>
    /// <returns>List&lt;VsProject&gt;.</returns>
    public static List<VsProject> GetProjectsFromSolution(string solutionFilePath)
    {
        Guard.IsNotNullOrWhiteSpace(solutionFilePath);

        List<VsProject> projects = [];
        Regex projectLinePattern = new Regex(@"^Project\(""\{[^}]+\}""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s*""\{[^}]+\}""",
            RegexOptions.Compiled);

        foreach (var line in File.ReadLines(solutionFilePath))
        {
            Match match = projectLinePattern.Match(line);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string path = match.Groups[2].Value;
                projects.Add(new VsProject(name, path));
            }
        }
        return projects;
    }

}