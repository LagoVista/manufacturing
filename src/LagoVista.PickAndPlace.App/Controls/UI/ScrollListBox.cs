// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 39848ac9008ec71ead380e9baba0de5e610fd49ee94fe5c17f40788902120be4
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.Controls.UI
{
    public class ListBoxScroll : System.Windows.Controls.ListBox
    {
        public ListBoxScroll() : base()
        {
            SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(ListBoxScroll_SelectionChanged);
        }

        void ListBoxScroll_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ScrollIntoView(SelectedItem);
        }
    }
}
