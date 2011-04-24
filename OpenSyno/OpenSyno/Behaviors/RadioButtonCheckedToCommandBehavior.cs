using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenSyno.Behaviors
{
    public class RadioButtonCheckedToCommandBehavior
    {


        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(RadioButtonCheckedToCommandBehavior), new PropertyMetadata(null, CommandPropertyChangedCallback));

        private static void CommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Note : It should be a OneTime binding, since we never unregister the event !                        
            ((RadioButton)d).Checked += CommandCaller;
        }

        private static void CommandCaller(object sender, RoutedEventArgs e)
        {
            var button = ((ButtonBase)sender);
            //button.Checked -= CommandCaller;
            var commandParameter = GetCommandParameter(button);
            GetCommand(button).Execute(commandParameter);

        }


        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(RadioButtonCheckedToCommandBehavior), new PropertyMetadata(null));
    }
}