using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LibGit2Sharp;
using NuGetSwitch.Message;
using NuGetSwitch.Model;

namespace NuGetSwitch.ViewModel;

public class GitViewModel : ObservableObject
{
    private readonly IMessenger m_messenger;
    private VsSolution? m_solution;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitViewModel"/> class.
    /// </summary>
    /// <param name="messenger">The messenger.</param>
    public GitViewModel(IMessenger messenger)
    {
        m_messenger = messenger;

        GitUndoChangesCommand = new RelayCommand(GitUndoChanges, CanExecuteGitUndoChanges);

        // Register for a switch message
        messenger.Register<SwitchMessage>(this, (_, m) => UpdateGitStatus(m.Solution));

        // Register for a solution loaded message
        messenger.Register<SolutionOpenedMessage>(this, (_, m) => UpdateGitStatus(m.Solution));

        messenger.Register<SolutionClosedMessage>(this, (_, _) =>
        {
            m_solution = null;
            ModifiedProjectFiles.Clear();
            NotifyCanExecuteChanged();
        });
    }

    /// <summary>
    /// Gets the git undo changes command.
    /// </summary>
    /// <value>The git undo changes command.</value>
    public RelayCommand GitUndoChangesCommand { get; }


    /// <summary>
    /// True if Git changes can be undone
    /// </summary>
    /// <returns>System.Boolean.</returns>
    private bool CanExecuteGitUndoChanges()
    {
        return ModifiedProjectFiles.Any();
    }

    /// <summary>
    /// Gets the modified Visual Studio project files.
    /// </summary>
    /// <value>The modified project files.</value>
    public ObservableCollection<string> ModifiedProjectFiles { get; } = [];

    /// <summary>
    /// Queries the Git repo and updates the list of modified Visual Studio project files
    /// </summary>
    private void UpdateGitStatus(VsSolution solution)
    {
        if (solution == null)
            throw new InvalidOperationException("No solution open");

        ModifiedProjectFiles.Clear();

        string solutionFolder = solution.SolutionFolder;
        string repoPath = Repository.Discover(solution.SolutionFolder);

        if (repoPath == null)
        {
            m_messenger.Send(new StatusMessage($"No Git repository found in the solution folder: {solutionFolder}"));
            return;
        }

        using Repository repo = new Repository(repoPath);

        RepositoryStatus status = repo.RetrieveStatus();

        foreach (StatusEntry entry in status.Where(e => e.State.HasFlag(FileStatus.ModifiedInWorkdir) && e.FilePath.EndsWith("csproj")))
        {
            ModifiedProjectFiles.Add(entry.FilePath);
        }

        m_solution = solution;

        NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Reverts any changes made to Visual Studio project files
    /// </summary>
    public void GitUndoChanges()
    {
        if (m_solution == null)
            throw new InvalidOperationException("No solution open");

        m_messenger.Send(new ClearStatusMessages());


        string solutionFolder = m_solution.SolutionFolder;
        string repoPath = Repository.Discover(solutionFolder);

        if (repoPath == null)
        {
            m_messenger.Send(new StatusMessage($"No Git repository found in the solution folder: {solutionFolder}"));
            return;
        }

        m_messenger.Send(new StatusMessage("Reverting changes"));

        using (Repository repo = new Repository(repoPath))
        {
            repo.CheckoutPaths("HEAD", ModifiedProjectFiles, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
            m_messenger.Send(new StatusMessage($"Reverted {ModifiedProjectFiles.Count} modified project files to HEAD."));
        }

        //todo: ParseVsSolutionFile(m_solution.FilePath);

        UpdateGitStatus(m_solution);

        NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Update the command states based on the current workspace state.
    /// </summary>
    private void NotifyCanExecuteChanged()
    {
        GitUndoChangesCommand.NotifyCanExecuteChanged(); 
    }
}

