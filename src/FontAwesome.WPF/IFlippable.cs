// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d55a8241067f4024c93258df3a4c74ae1fe49f296c8c557468535089b0050bbd
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Windows;
using System.Windows.Controls;

namespace FontAwesome.WPF
{
    /// <summary>
    /// Defines the different flip orientations that a icon can have.
    /// </summary>
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
    public enum FlipOrientation
    {
        /// <summary>
        /// Default
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Flip horizontally (on x-achsis)
        /// </summary>
        Horizontal,
        /// <summary>
        /// Flip vertically (on y-achsis)
        /// </summary>
        Vertical,
    }

    /// <summary>
    /// Represents a flippable control
    /// </summary>
    public interface IFlippable
    {
        /// <summary>
        /// Gets or sets the current orientation (horizontal, vertical).
        /// </summary>
        FlipOrientation FlipOrientation { get; set; }
    }
}