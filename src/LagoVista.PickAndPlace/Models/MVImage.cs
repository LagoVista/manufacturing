// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 417219e63628708c8059e9304d0b9a4d858b55d6994bc9ffa89eacb4ef36b28d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Models
{
    public class MVImage<T> : IMVImage<T>, IDisposable where T: class, IDisposable
    {
        public MVImage(T image)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
        }

        public T Image { get; }

        public void Dispose()
        {
            if(Image != null)
                Image.Dispose();
        }
    }
}
