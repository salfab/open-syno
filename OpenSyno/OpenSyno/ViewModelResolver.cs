using Ninject;
using Ninject.Activation;
using Ninject.Planning.Bindings;

namespace OpenSyno.Helpers
{
    using System;
    using System.Windows;

    using OpenSyno.ViewModels;

    /// <summary>
    /// Bad design : if the view model to resolve is not in the same assembly, it will not work ! change it and move this class to the common dll !
    /// </summary>
    public class ViewModelResolver
    {
        // Note : Too bad, TypeNameConverter does not exist for WP7 :(
        // http://msdn.microsoft.com/en-us/library/system.configuration.typenameconverter.aspx

        //[TypeConverter(typeof(TypeTypeConverter))]
        public static string GetViewModelType(DependencyObject obj)
        {
            return (string)obj.GetValue(ViewModelTypeProperty);
        }

        public static void SetViewModelType(DependencyObject obj, string value)
        {
            obj.SetValue(ViewModelTypeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ViewModelType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelTypeProperty =
            DependencyProperty.RegisterAttached("ViewModelType", typeof(string), typeof(ViewModelResolver), new PropertyMetadata(ViewModelTypeChanged));
        
        private static void ViewModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
            var view = (FrameworkElement)d;
            
            view.Loaded += (s, ea) =>
                               {
                                   var loadedView = (FrameworkElement)d;
                                   Type type = Type.GetType((string)e.NewValue);
        
                                   // If we're dealing with a viewmodel directly
                                   if (type.IsSubclassOf(typeof (ViewModelBase)))
                                   {                                       
                                       loadedView.DataContext = IoC.Container.Get(type);
                                       return;
                                   }
                                   
                                   var factory = IoC.Container.Get(type);
                                   // if we're dealing with a factory which will build our view model
                                   // ...
                               };

        }


        public static bool ValidatedByAParentConstraint(IRequest arg, string bindingName)
        {
            IRequest request = arg;

            // we bubble up the constraints, to find one that would be available
            while (request.Constraint == null)
            {
                request = arg.ParentRequest;
            }

            return request.Constraint(new BindingMetadata { Name = bindingName });                                                              
        }
    }
}