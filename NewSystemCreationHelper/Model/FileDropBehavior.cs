using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace NewSystemCreationHelperModule.Model
{
    public class FileDropBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
             DependencyProperty.Register(
                 nameof(Command),
                 typeof(ICommand),
                 typeof(FileDropBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.AllowDrop = true;

            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;

            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            Command?.Execute(files);
        }

    }
}