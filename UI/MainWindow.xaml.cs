using NodeEditor.App.Commands;
using NodeEditor.Controls;
using NodeEditor.Geometry;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NodeEditor.UI {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
    }

    public ICommand RemoveConnectionCommand => new DelegateCommand((object conn) => {
      DataContext = (DataContext as Graph).RemoveConnection(conn as Connection);
    });

    private void SaveButton_Click(object sender, RoutedEventArgs e) {
      var commandManager = AttachedProps.GetCommandManager(sender as DependencyObject);
      commandManager.StartCommand(new SaveCommandToken());
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e) {
      var commandManager = AttachedProps.GetCommandManager(sender as DependencyObject);
      commandManager.StartCommand(new PlayCommandToken());
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      var button = sender as Button;
      var componentName = button.DataContext as string;
      var commandManager = AttachedProps.GetCommandManager(button);
      commandManager.StartCommand(new AddNodeCommandToken(null, componentName));
    }
    //private void PlusButton_Click(object sender, RoutedEventArgs e) {
    //  var commandManager = AttachedProps.GetCommandManager(sender as DependencyObject);
    //  commandManager.StartCommand(new PlayCommandToken());
    //}
  }
}
