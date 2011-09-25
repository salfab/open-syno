using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace OpenSyno.Helpers
{
    public class AppBarBindingsHelper
    {
        public static void UpdateBinding(DependencyObject dependencyObject)
        {
            // Support for text boxes.
            if (dependencyObject as TextBox != null)
            {
                BindingExpression bindingExpression = ((TextBox)dependencyObject).GetBindingExpression(TextBox.TextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateSource();
                }
            }

            // TODO : Support for Content Controls.
        }
    }
}
