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
using Ninject;
using OpenSyno.Helpers;

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


        private static void CommandPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox attachedElement;         
            attachedElement = d as TextBox;                
        
            if (attachedElement != null)
            {
                if (e.OldValue == null)
                {
                    // Only register the internal event once.
                    attachedElement.KeyUp += FilterEnterKeysAndForwardToCommand;                    
                }
            }
        }

        private static void FilterEnterKeysAndForwardToCommand(object sender, KeyEventArgs e)
        {
            var attachedElement = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                AppBarBindingsHelper.UpdateBinding(attachedElement);

                // If ony the enter key was pressed
                var command = GetCommand(attachedElement);
                var commandParameter = GetCommandParameter(attachedElement);
                command.Execute(commandParameter);

                if (GetHideKeyboardAfterEnter(attachedElement))
                {
                    GetControlToFocusAfterEnter(attachedElement).Focus();                    
                }
            }
        }



        public static Control GetControlToFocusAfterEnter(DependencyObject obj)
        {
            return (Control)obj.GetValue(ControlToFocusAfterEnterProperty);
        }

        public static void SetControlToFocusAfterEnter(DependencyObject obj, Control value)
        {
            obj.SetValue(ControlToFocusAfterEnterProperty, value);
        }

        // Using a DependencyProperty as the backing store for ControlToFocusAfterEnter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControlToFocusAfterEnterProperty =
            DependencyProperty.RegisterAttached("ControlToFocusAfterEnter", typeof(Control), typeof(EnterKeyUpToCommandBehavior), new PropertyMetadata(null));

        


        public static bool GetHideKeyboardAfterEnter(DependencyObject obj)
        {
            return (bool)obj.GetValue(HideKeyboardAfterEnterProperty);
        }

        public static void SetHideKeyboardAfterEnter(DependencyObject obj, bool value)
        {
            obj.SetValue(HideKeyboardAfterEnterProperty, value);
        }

        // Using a DependencyProperty as the backing store for HideKeyboardAfterEnter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideKeyboardAfterEnterProperty =
            DependencyProperty.RegisterAttached("HideKeyboardAfterEnter", typeof(bool), typeof(EnterKeyUpToCommandBehavior), new PropertyMetadata(true));

        


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
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(EnterKeyUpToCommandBehavior), new PropertyMetadata(null));
    }
}
