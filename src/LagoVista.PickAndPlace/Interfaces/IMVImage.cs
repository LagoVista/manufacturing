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
