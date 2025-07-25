using NuGetSwitch.Model;
using System.IO;
using System.Text.RegularExpressions;

namespace NuGetSwitch.Helper
{
    /// <summary>
    /// Class VsSolutionFileHelper.
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
            var projects = new List<VsProject>();
            var projectLinePattern = new Regex(
                @"^Project\(""\{[^}]+\}""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s*""\{[^}]+\}""",
                RegexOptions.Compiled);

            foreach (var line in File.ReadLines(solutionFilePath))
            {
                var match = projectLinePattern.Match(line);
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
}
