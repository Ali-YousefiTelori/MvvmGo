using System.Collections.Generic;

namespace System.Windows.Input
{
#if(!NET35 && !NET40)
    public static class CommandExtenstions
    {
        public static Func<Action, ICommand> InstanceAction { get; set; }

        public static ICommand Instance(Action action)
        {
            return InstanceAction(action);
        }

        public static ICommand Instance(Action action, Action<List<CommandErrorInfo>> errorAction, Func<List<CommandErrorInfo>> validateFunction)
        {
            return InstanceAction(() =>
            {
                var validateResult = validateFunction();
                if (validateResult == null || validateResult.Count == 0)
                    action();
                else
                    errorAction(validateResult);
            });
        }
    }
#endif
}
