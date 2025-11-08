// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b4a9b274d074467d45fdd21ba575d17716238a73682cefa7a5f5274d8508157f
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IMVImage<T> : IDisposable where T : class, IDisposable
    {
        T Image { get; }
    }
}
