// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 172470bb6423290f20c5230031c8f0bac3be7fa090b17626c10c6f62ffcba70c
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FontAwesome.WPF
{
    /// <summary>
    /// Represents a spinable control
    /// </summary>
    public interface ISpinable
    {
        /// <summary>
        /// Gets or sets the current spin (angle) animation of the icon.
        /// </summary>
        bool Spin { get; set; }

        /// <summary>
        /// Gets or sets the duration of the spinning animation (in seconds). This will stop and start the spin animation.
        /// </summary>
        double SpinDuration { get; set; }
    }
}
