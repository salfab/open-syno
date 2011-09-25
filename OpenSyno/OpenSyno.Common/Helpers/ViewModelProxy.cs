namespace OpenSyno.Helpers
{
    using System;
    using System.Windows;

    public class ViewModelProxy
    {
        private object _viewModel;

        public object ViewModel
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                _viewModel = value;
            }
        }

        //public static object GetViewModel(DependencyObject obj)
        //{
        //    return (object)obj.GetValue(ViewModelProperty);
        //}

        //public static void SetViewModel(DependencyObject obj, object value)
        //{
        //    obj.SetValue(ViewModelProperty, value);
        //}

        //// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ViewModelProperty =
        //    DependencyProperty.RegisterAttached("ViewModel", typeof(object), typeof(ViewModelProxy), new PropertyMetadata(null));


    }
}