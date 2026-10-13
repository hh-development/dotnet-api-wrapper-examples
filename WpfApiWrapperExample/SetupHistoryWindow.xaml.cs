using System.Collections.Generic;
using System.Windows;
using WpfApiWrapperExample.Models;

namespace WpfApiWrapperExample
{
    /// <summary>
    /// Shows the change history of a setup, as returned by /Setups/History/{id}.
    /// Filtering, searching and counting are all handled by the DevExpress GridControl (see the XAML).
    /// </summary>
    public partial class SetupHistoryWindow : Window
    {
        public SetupHistoryWindow(string setupName, IReadOnlyList<ApiHistoryEntryModel> entries)
        {
            InitializeComponent();
            Title = $"History - {setupName}";

            HistoryGrid.ItemsSource = entries;
        }
    }
}
