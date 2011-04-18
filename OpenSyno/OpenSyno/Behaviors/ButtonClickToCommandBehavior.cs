using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace OpenSyno.Behaviors
{
    using System.Windows.Controls.Primitives;

    public class ButtonClickToCommandBehavior
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
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ButtonClickToCommandBehavior), new PropertyMetadata(null, CommandPropertyChangedCallback));

        private static void CommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Note : It should be a OneTime binding, since we never unregister the event !                        
            ((ButtonBase) d).Click += CommandCaller;
        }

        private static void CommandCaller(object sender, RoutedEventArgs e)
        {
            var button = ((ButtonBase) sender);
            //button.Click -= CommandCaller;
            GetCommand(button).Execute(GetCommandParameter(button));

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
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(ButtonClickToCommandBehavior), new PropertyMetadata(null));
    }
}
