using System.Windows;
using VideoLibraryManager.ViewModel;

namespace VideoLibraryManager
{
    public class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(VideoManagerViewModel), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}