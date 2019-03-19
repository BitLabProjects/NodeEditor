using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeEditor.App.Commands {
  interface ICommandToken {
  }

  interface ICommandManager {
    void StartCommand(ICommandToken token);
  }

  abstract class CommandBase {
    public abstract void Execute(ICommandToken token);
  }

  class CommandManager : ICommandManager {
    private Dictionary<Type, Func<CommandBase>> mTokenTypeToFactory;
    public CommandManager() {
      mTokenTypeToFactory = new Dictionary<Type, Func<CommandBase>>();
    }

    public void StartCommand(ICommandToken token) {
      //1. Realize the command
      Func<CommandBase> factory;
      if (!mTokenTypeToFactory.TryGetValue(token.GetType(), out factory)) {
        // Command not registered
        return;
      }

      //2. Run the command
      var command = factory();
      command.Execute(token);
    }

    internal void RegisterCommand(Type tokenType, Func<CommandBase> factory) {
      mTokenTypeToFactory[tokenType] = factory;
    }
  }
}
