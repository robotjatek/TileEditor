using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TileEditor;

class SelectedChangeMessage : ValueChangedMessage<Tile>
{
    public SelectedChangeMessage(Tile value) : base(value)
    {
    }
}


/// <summary>
/// Interaction logic for TileSelectorView.xaml
/// </summary>
public partial class TileSelectorView : UserControl
{
    public TileSelectorView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = ((sender as ListBox)!.SelectedItem as Tile)!;
        WeakReferenceMessenger.Default.Send(new SelectedChangeMessage(selected));
    }
}
