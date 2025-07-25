namespace NuGetSwitch.Service
{
    /// <summary>
    /// Interface IDialogService
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows the workspace open file dialog.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        string? ShowWorkspaceOpenFileDialog(string startFolder);

        /// <summary>
        /// Shows the solution open file dialog.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        string? ShowSolutionOpenFileDialog();

        /// <summary>
        /// Shows the workspace save file dialog.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="defaultPath">The default path.</param>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        string? ShowWorkspaceSaveFileDialog(string fileName, string defaultPath);

        /// <summary>
        /// Shows the add references file dialog.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;IEnumerable&lt;System.String&gt;&gt;.</returns>
        IEnumerable<string>? ShowAddReferencesFileDialog(string startFolder);

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        void ShowError(string message);
    }
}
