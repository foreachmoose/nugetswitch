using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NuGetSwitch.Message;

namespace NuGetSwitch.ViewModel;

/// <summary>
/// Class StatusMessageViewModel.
/// Implements the <see cref="ObservableObject" />
/// </summary>
/// <seealso cref="ObservableObject" />
public class StatusMessageViewModel : ObservableObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitViewModel"/> class.
    /// </summary>
    /// <param name="messenger">The messenger.</param>
    public StatusMessageViewModel(IMessenger messenger)
    {
        // Register for a switch message
        messenger.Register<StatusMessage>(this, (_, m) => Messages.Add(m.Status));
        messenger.Register<ClearStatusMessages>(this, (_, _) => Messages.Clear());
    }

    /// <summary>
    /// Gets the status message list
    /// </summary>
    /// <value>The messages.</value>
    public ObservableCollection<string> Messages { get; } = [];
}

