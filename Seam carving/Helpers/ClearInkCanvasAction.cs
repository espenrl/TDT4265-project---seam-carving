using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace erl
{
    public class ClearInkCanvasAction : TriggerAction<DependencyObject>
    {
        public static readonly DependencyProperty InkCanvasProperty = DependencyProperty.Register(
            "InkCanvas", typeof (InkCanvas), typeof (ClearInkCanvasAction), new PropertyMetadata(default(InkCanvas)));

        public InkCanvas InkCanvas
        {
            get { return (InkCanvas) GetValue(InkCanvasProperty); }
            set { SetValue(InkCanvasProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            if(InkCanvas == null) return;

            InkCanvas.Strokes.Clear();
            InkCanvas.RaiseEvent(new RoutedEventArgs(InkCanvas.StrokeErasedEvent));
        }
    }
}