namespace OpenSyno.Helpers
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    using OpenSyno.Converters;
    using OpenSyno.ViewModels;


    public class ViewModelResolver
    {
        // Note : Too bad, TypeNameConverter does not exist for WP7 :(
        // http://msdn.microsoft.com/en-us/library/system.configuration.typenameconverter.aspx

        [TypeConverter(typeof(TypeTypeConverter))]
        public static Type GetViewModelType(DependencyObject obj)
        {
            return (Type)obj.GetValue(ViewModelTypeProperty);
        }

        public static void SetViewModelType(DependencyObject obj, Type value)
        {
            obj.SetValue(ViewModelTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ViewModelType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelTypeProperty =
            DependencyProperty.RegisterAttached("ViewModelType", typeof(Type), typeof(ViewModelResolver), new PropertyMetadata(ViewModelTypeChanged));
        
        private static void ViewModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (FrameworkElement)d;

            // If we're dealing with a viewmodel directly
            if (((Type)e.NewValue).IsSubclassOf(typeof(ViewModelBase)))
            {
                view.DataContext = IoC.Container.Resolve((Type)e.NewValue);
                return;
            }

            var factory = IoC.Container.Resolve((Type)e.NewValue);            
            // if we're dealing with a factory which will build our view model
            // ...
        }
    }
}