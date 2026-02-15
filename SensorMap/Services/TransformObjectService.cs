using SensorMap.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;
using Cursor = System.Windows.Input;
using Cursors = System.Windows.Input;
namespace SensorMap.Services
{
    public class TransformObjectService : ITransformObject
    {
        public System.Windows.Input.Cursor GetCursorForHitType(HitType hitType)
        {
            System.Windows.Input.Cursor desired_cursor = System.Windows.Input.Cursors.Arrow;
            switch (hitType)
            {
                case HitType.None:
                    return desired_cursor = System.Windows.Input.Cursors.Arrow;
                case HitType.Body:
                    return desired_cursor = System.Windows.Input.Cursors.ScrollAll;
                case HitType.UpLeft:
                case HitType.BottomRight:
                    return desired_cursor = System.Windows.Input.Cursors.SizeNWSE;
                case HitType.BottomLeft:
                case HitType.UpRight:
                    return desired_cursor = System.Windows.Input.Cursors.SizeNESW;
                case HitType.Top:
                case HitType.Bottom:
                    return desired_cursor = System.Windows.Input.Cursors.SizeNS;
                case HitType.Left:
                case HitType.Right:
                    return desired_cursor = System.Windows.Input.Cursors.SizeWE;
            }
            return desired_cursor;
        }
        

        public HitType GetHitType( Rect customBounds, System.Windows.Point mousePosition, double gap = 0)
        {
            double leftBorder = customBounds.Left;
            double topBorder = customBounds.Top;
            double rightBorder = leftBorder + customBounds.Width;
            double bottomBorder = topBorder + customBounds.Height;

            if (mousePosition.X < leftBorder) return HitType.None;
            if (mousePosition.X > rightBorder) return HitType.None;
            if (mousePosition.Y < topBorder) return HitType.None;
            if (mousePosition.Y > bottomBorder) return HitType.None;

            // Если курсор у левой границы
            if (mousePosition.X < leftBorder + gap)
            { 
                if (mousePosition.Y - topBorder < gap) return HitType.UpLeft;
                if (bottomBorder - mousePosition.Y < gap) return HitType.BottomLeft;
                return HitType.Left;
            }
            // Если курсор у правой границы
            if (rightBorder - mousePosition.X < gap)
            {
                if (mousePosition.Y - topBorder < gap) return HitType.UpRight;
                if (bottomBorder - mousePosition.Y < gap) return HitType.BottomRight;
                return HitType.Right;
            }
            // Если курсор у верхней границы
            if (mousePosition.Y < gap + topBorder) return HitType.Top;
            // Если курсор у Нижней границы
            if (bottomBorder - mousePosition.Y < gap) return HitType.Bottom;
            
            return HitType.Body;
        }

        public Canvas GetParentCanvas(UIElement element)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null && parent is not Canvas)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as Canvas;
        }
        public Rect? TransformRectangle(Rect originalBounds, HitType hitType, double offset_x, double offset_y)
        {
            double new_x = originalBounds.Left;
            double new_y = originalBounds.Top;
            double new_width = originalBounds.Width;
            double new_height = originalBounds.Height;

            switch (hitType)
            {
                case HitType.UpLeft:
                    new_x += offset_x;
                    new_y += offset_y;
                    new_width -= offset_x;
                    new_height -= offset_y;
                    break;
                case HitType.UpRight:
                    new_y += offset_y;
                    new_width += offset_x;
                    new_height -= offset_y;
                    break;
                case HitType.BottomRight:
                    new_width += offset_x;
                    new_height += offset_y;
                    break;
                case HitType.BottomLeft:
                    new_x += offset_x;
                    new_width -= offset_x;
                    new_height += offset_y;
                    break;
                case HitType.Left:
                    new_x += offset_x;
                    new_width -= offset_x;
                    break;
                case HitType.Right:
                    new_width += offset_x;
                    break;
                case HitType.Bottom:
                    new_height += offset_y;
                    break;
                case HitType.Top:
                    new_y += offset_y;
                    new_height -= offset_y;
                    break;
            }

            // Проверяем на корректные размеры
            if (new_width > 0 && new_height > 0)
            {
                return new Rect(new_x,new_y,new_width, new_height);
            }

            return null;
        }


        #region CoordConverter
        public Point WorldToScreen(Point world, Matrix matrix)
        {
            double x = world.X;
            double y = world.Y;
            double sx = matrix.M11 * x + matrix.M12 * y + matrix.OffsetX;
            double sy = matrix.M21 * x + matrix.M22 * y + matrix.OffsetY;
            return new Point(sx, sy);
        }
        public Point ScreenToWorld(Point screen, Matrix matrix)
        {
            if (!matrix.HasInverse) return new Point(0, 0);
            Matrix inv = matrix;
            inv.Invert();
            double x = screen.X, y = screen.Y;
            double wx = inv.M11 * x + inv.M12 * y + inv.OffsetX;
            double wy = inv.M21 * x + inv.M22 * y + inv.OffsetY;
            return new Point(wx, wy);
        }
        #endregion
    }
}
