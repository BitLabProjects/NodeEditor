using NodeEditor.App;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NodeEditor {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class Application : System.Windows.Application {
    private void Application_Startup(object sender, StartupEventArgs e) {
      var app = new NodeEditorApp();
      var mainWindow = new UI.MainWindow();
      mainWindow.DataContext = app;
      mainWindow.Show();
    }
  }
}
