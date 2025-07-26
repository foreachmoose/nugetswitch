namespace NuGetSwitch.Service
{
    /// <summary>
    /// Dialog service interface
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows the solution open file dialog.
        /// </summary>
        /// <returns>System.Nullable&lt;System.String&gt;.</returns>
        string? ShowSolutionOpenFileDialog();

        /// <summary>
        /// Shows the add references file dialog.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        /// <returns>System.Nullable&lt;IEnumerable&lt;System.String&gt;&gt;.</returns>
        IEnumerable<string>? ShowAddReferencesFileDialog(string startFolder);

        /// <summary>
        /// Shows an error.
        /// </summary>
        /// <param name="message">The message.</param>
        void ShowError(string message);
    }
}
