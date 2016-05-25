namespace BuildingControl.Utility
{
  using System;
  using System.Windows.Input;

  class SimpleCommand : ICommand
  {
    public event EventHandler CanExecuteChanged;

    public SimpleCommand(Action action)
    {
      this.action = action;
    }
    public SimpleCommand(Action<object> paramAction)
    {
      this.paramAction = paramAction;
    }
    public bool CanExecute(object parameter)
    {
      return (true);
    }
    public void Execute(object parameter)
    {
      if (this.action != null)
      {
        this.action();
      }
      if (this.paramAction != null)
      {
        this.paramAction(parameter);
      }
    }
    Action action;
    Action<object> paramAction;
  }
}
