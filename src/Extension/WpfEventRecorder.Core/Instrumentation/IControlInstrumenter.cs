using System.Windows;
using WpfEventRecorder.Core.Recording;

namespace WpfEventRecorder.Core.Instrumentation;

/// <summary>
/// Interface for control-specific instrumenters.
/// </summary>
public interface IControlInstrumenter
{
    /// <summary>
    /// Gets the type of control this instrumenter handles.
    /// </summary>
    Type ControlType { get; }

    /// <summary>
    /// Checks if this instrumenter can handle the given element.
    /// </summary>
    bool CanInstrument(DependencyObject element);

    /// <summary>
    /// Attaches event handlers to the element.
    /// </summary>
    void Instrument(DependencyObject element, RecordingSession session, RecordingConfiguration configuration);

    /// <summary>
    /// Detaches event handlers from the element.
    /// </summary>
    void Uninstrument(DependencyObject element);
}
