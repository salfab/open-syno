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
    public class EnterKeyUpToCommandBehavior
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
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(EnterKeyUpToCommandBehavior), new PropertyMetadata(null, CommandPropertyChangedCallback));

        private static TextBox _attachedElement;

        private static void CommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_attachedElement == null)
            {
                // It's a choice we made not to be able to change the attached element once it's set
                _attachedElement = d as TextBox;                
            }
            if (_attachedElement != null)
            {
                if (e.OldValue == null)
                {
                    // Only register the internal event once.
                    _attachedElement.KeyUp += FilterEnterKeysAndForwardToCommand;                    
                }
            }
        }

        private static void FilterEnterKeysAndForwardToCommand(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // If ony the enter key was pressed
                GetCommand(_attachedElement).Execute(null);                
            }
        }
    }
}
