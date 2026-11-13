using System.Windows;
using CorporateCarsharing.Client.ViewModels;
namespace CorporateCarsharing.Client;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
