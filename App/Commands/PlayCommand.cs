using NodeEditor.Fbp;
using NodeEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NodeEditor.App.Commands {
  class PlayCommandToken: ICommandToken {
    public PlayCommandToken() {
    }
  }

  class PlayCommand : CommandBase {
    private readonly NodeEditorApp mApp;
    public PlayCommand(NodeEditorApp app) {
      mApp = app;
    }

    public override void Execute(ICommandToken commandToken) {
      var token = commandToken as PlayCommandToken;

      var scheduler = new Scheduler(mApp.Graph);
      try {
        scheduler.Run();
      } catch (Exception ex) {
        MessageBox.Show("Error: " + ex.Message, "Scheduler error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}
