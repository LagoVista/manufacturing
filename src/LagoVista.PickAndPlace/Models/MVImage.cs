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
