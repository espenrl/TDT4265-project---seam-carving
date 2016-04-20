using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace erl
{
    public class InkCanvasDrawBehavior : Behavior<InkCanvas>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
            AssociatedObject.KeyDown += KeyDown;
            AssociatedObject.KeyUp += KeyUp;
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            Update();
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            Update();
        }


        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                AssociatedObject.EditingMode = InkCanvasEditingMode.Ink;
                AssociatedObject.DefaultDrawingAttributes.Color = Colors.Red;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                AssociatedObject.EditingMode = InkCanvasEditingMode.EraseByPoint;
                AssociatedObject.DefaultDrawingAttributes.Color = Colors.Green;
            }
            else
            {
                AssociatedObject.EditingMode = InkCanvasEditingMode.Ink;
                AssociatedObject.DefaultDrawingAttributes.Color = Colors.Green;
            }
        }
    }
}