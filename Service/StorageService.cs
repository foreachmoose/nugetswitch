using System.IO;
using System.Text.Json;
using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Service
{
    /// <summary>
    /// Class StorageService.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorageService<T> where T : class, new()
    {
        /// <summary>
        /// Save as an asynchronous operation.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="document">The document.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SaveAsync(string filePath, T document)
        {
            Guard.IsNotNullOrEmpty(filePath);

            string json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <summary>
        /// Load as an asynchronous operation.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A Task&lt;T&gt; representing the asynchronous operation.</returns>
        public async Task<T?> LoadAsync(string filePath)
        {
            Guard.IsNotNullOrEmpty(filePath);

            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(filePath);
            T? doc = JsonSerializer.Deserialize<T>(json);

            return doc;
        }
    }

}