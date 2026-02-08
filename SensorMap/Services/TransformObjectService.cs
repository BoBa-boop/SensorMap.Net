using SensorMap.Interfaces;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Media3D;
namespace SensorMap.Services
{
    public class TransformObjectService : ITransformObject
    {
        public System.Windows.Input.Cursor GetCursorForHitType(HitType hitType)
        {
            return hitType switch
            {
                HitType.None => System.Windows.Input.Cursors.Arrow,
                HitType.Body => System.Windows.Input.Cursors.SizeAll,
                HitType.UpLeft or HitType.BottomRight => System.Windows.Input.Cursors.SizeNWSE,
                HitType.BottomLeft or HitType.UpRight => System.Windows.Input.Cursors.SizeNESW,
                HitType.Top or HitType.Bottom => System.Windows.Input.Cursors.SizeNS,
                HitType.Left or HitType.Right => System.Windows.Input.Cursors.SizeWE,
                _ => System.Windows.Input.Cursors.Arrow
            };
        }
        

        public HitType GetHitType( Rect customBounds, System.Windows.Point mousePosition, double gap = 3)
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
    }
}
