using CommunityToolkit.Diagnostics;
using System.IO;

namespace NuGetSwitch.Helper
{
    /// <summary>
    /// Class PathHelper.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Returns a relative path from the starting path to the target absolute path.
        /// </summary>
        /// <param name="absolutePath">The target absolute path.</param>
        /// <param name="basePath">The starting path from which to calculate the relative path.</param>
        /// <returns>A relative path using "..\" notation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either path is null or empty.</exception>
        public static string GetRelativePath(string absolutePath, string basePath)
        {
            Guard.IsNotNullOrWhiteSpace(absolutePath);
            Guard.IsNotNullOrWhiteSpace(basePath);

            // Normalize the paths
            string absFullPath = Path.GetFullPath(absolutePath).TrimEnd(Path.DirectorySeparatorChar);
            string baseFullPath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar);

            // Split into directory segments
            var absParts = absFullPath.Split(Path.DirectorySeparatorChar);
            var baseParts = baseFullPath.Split(Path.DirectorySeparatorChar);

            // Find the common root
            int commonLength = 0;
            while (commonLength < absParts.Length &&
                   commonLength < baseParts.Length &&
                   string.Equals(absParts[commonLength], baseParts[commonLength], StringComparison.OrdinalIgnoreCase))
            {
                commonLength++;
            }

            // Navigate back to the common ancestor
            var relativeParts = new string[baseParts.Length - commonLength];
            for (int i = 0; i < relativeParts.Length; i++)
            {
                relativeParts[i] = "..";
            }

            // Navigate down to the target path
            var downParts = absParts[commonLength..];

            // Combine the parts
            var fullRelativePath = Path.Combine(Path.Combine(relativeParts), Path.Combine(downParts));

            return string.IsNullOrEmpty(fullRelativePath) ? "." : fullRelativePath;
        }
    }
}