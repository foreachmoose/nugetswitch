using CommunityToolkit.Diagnostics;
using NuGetSwitch.Model;
using System.IO;
using System.Xml.Linq;

namespace NuGetSwitch.Helper
{
    /// <summary>
    /// Class VsProjectFileHelper.
    /// </summary>
    public static class VsProjectFileHelper
    {
        /// <summary>
        /// Removes a NuGet package reference from a Visual Studio .csproj file.
        /// </summary>
        /// <param name="csprojPath">Full path to the .csproj file.</param>
        /// <param name="packageId">The package ID to remove.</param>
        public static void RemoveNuGetPackageReference(string csprojPath, string packageId)
        {
            Guard.IsNotNullOrWhiteSpace(csprojPath);
            Guard.IsNotNullOrWhiteSpace(packageId);

            if (!File.Exists(csprojPath)) 
                throw new FileNotFoundException("Project file not found", csprojPath);

            try
            {
                XDocument doc = XDocument.Load(csprojPath);
                XNamespace ns = doc.Root!.GetDefaultNamespace();

                var packageRefs = doc.Descendants(ns + "PackageReference")
                    .Where(p => string.Equals(p.Attribute("Include")?.Value, packageId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var refNode in packageRefs)
                {
                    refNode.Remove();
                }

                if (packageRefs.Any())
                {
                    doc.Save(csprojPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove packages from {csprojPath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds or updates multiple DLL references in a .csproj file.
        /// </summary>
        /// <param name="csprojPath">Path to the .csproj file.</param>
        /// <param name="references">A collection of (referenceName, hintPath) pairs.</param>
        public static void AddDllReferences(string csprojPath, IEnumerable<(string ReferenceName, string HintPath)> references)
        {
            Guard.IsNotNullOrWhiteSpace(csprojPath);
            Guard.IsNotNull(references);
            Guard.IsTrue(references.Any(), "No references provided");

            if (!File.Exists(csprojPath)) 
                throw new FileNotFoundException("Project file not found", csprojPath);
            
            try
            {
                XDocument doc = XDocument.Load(csprojPath);
                XNamespace ns = doc.Root!.GetDefaultNamespace();

                // Ensure at least one ItemGroup exists or create one for references
                XElement? itemGroup = doc.Descendants(ns + "ItemGroup")
                                        .FirstOrDefault(g => g.Elements(ns + "Reference").Any());

                if (itemGroup == null)
                {
                    itemGroup = new XElement(ns + "ItemGroup");
                    doc.Root.Add(itemGroup);
                }

                foreach (var (referenceName, hintPath) in references)
                {
                    if (string.IsNullOrWhiteSpace(referenceName) || string.IsNullOrWhiteSpace(hintPath))
                    {
                        Console.Error.WriteLine($"Skipped invalid entry: '{referenceName}' - '{hintPath}'");
                        continue;
                    }

                    var existingReference = itemGroup.Elements(ns + "Reference")
                                                     .FirstOrDefault(r => string.Equals(
                                                         r.Attribute("Include")?.Value,
                                                         referenceName,
                                                         StringComparison.OrdinalIgnoreCase));

                    if (existingReference != null)
                    {
                        existingReference.Element(ns + "HintPath")?.SetValue(hintPath);
                    }
                    else
                    {
                        var newReference = new XElement(ns + "Reference",
                            new XAttribute("Include", referenceName),
                            new XElement(ns + "HintPath", hintPath),
                            new XElement(ns + "Private", "true")
                        );
                        itemGroup.Add(newReference);
                    }
                }

                doc.Save(csprojPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating DLL references: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the nu get packages from project.
        /// </summary>
        /// <param name="projectFilePath">The project file path.</param>
        /// <returns>List&lt;NuGetPackage&gt;.</returns>
        public static List<NuGetPackage> GetNuGetPackagesFromProject(string projectFilePath)
        {
            List<NuGetPackage> packages = new List<NuGetPackage>();
            if (!File.Exists(projectFilePath))
                return packages;

            XDocument doc = XDocument.Load(projectFilePath);
            XNamespace ns = doc.Root?.Name.Namespace ?? "";

            foreach (var packageRef in doc.Descendants(ns + "PackageReference"))
            {
                string? id = packageRef.Attribute("Include")?.Value;
                string? version = packageRef.Attribute("Version")?.Value
                                 ?? packageRef.Element(ns + "Version")?.Value;

                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version))
                {
                    packages.Add(new NuGetPackage(id, version));
                }
            }
            return packages;
        }
    }

}