using System.IO;
using System.Text.Json;
using CommunityToolkit.Diagnostics;

namespace NuGetSwitch.Service;

/// <summary>
/// Simple storage service, mainly for storing
/// workspace documents
/// </summary>
/// <typeparam name="T"></typeparam>
public class StorageService
{
    /// <summary>
    /// Saves the document
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="document">The document.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SaveAsync<T>(string filePath, T document) where T : class
    {
        Guard.IsNotNullOrEmpty(filePath);
        Guard.IsNotNull(document);

        string json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Loads a document. Returns null, if it doesn't exist.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>A Task&lt;T&gt; representing the asynchronous operation.</returns>
    public async Task<T?> LoadAsync<T>(string filePath) where T : class
    {
        Guard.IsNotNullOrEmpty(filePath);

        if (!File.Exists(filePath))
            return null;

        string json = await File.ReadAllTextAsync(filePath);
        T? doc = JsonSerializer.Deserialize<T>(json);

        return doc;
    }
}

