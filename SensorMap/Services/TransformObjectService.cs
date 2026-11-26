using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static SensorMap.Services.TransformObjectService;
using Cursors = System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
namespace SensorMap.Services
{
    public class TransformObjectService : ITransformObject
    {
        public System.Windows.Input.Cursor GetCursorForHitType(HitType hitType,double angle = 0)
        {
            System.Windows.Input.Cursor desired_cursor = System.Windows.Input.Cursors.Arrow;
            double dir;
            switch (hitType)
            {
                case HitType.None: return desired_cursor = System.Windows.Input.Cursors.Arrow;
                case HitType.Body: return desired_cursor = System.Windows.Input.Cursors.ScrollAll;
                case HitType.UpLeft:        dir = 315; break;
                case HitType.BottomRight:   dir = 135; break;
                case HitType.BottomLeft:    dir = 225; break;
                case HitType.UpRight:       dir = 45; break;
                case HitType.Top:           dir = 0; break;
                case HitType.Bottom:        dir = 180; break;
                case HitType.Left:          dir = 270; break;
                case HitType.Right:         dir = 90; break;
                default: return desired_cursor = System.Windows.Input.Cursors.Arrow;
            }
            int step = (int)Math.Round((dir + angle) / 45.0) % 8;
            if (step < 0) step += 8;
            return (step % 4) switch
            {
                0 => System.Windows.Input.Cursors.SizeNS,
                1 => System.Windows.Input.Cursors.SizeNESW,
                2 => System.Windows.Input.Cursors.SizeWE,
                _=> System.Windows.Input.Cursors.SizeNWSE
            };
        }
        public enum CollisionSide { None, Left, Right, Top, Bottom }
        public enum AddressPosition { Center,Left,Right,Top, Bottom,None }
        
        public HitType GetHitType( Rect customBounds, System.Windows.Point mousePosition, double gap = 0, double angle = 0)
        {
            if(angle != 0)
            {
                var center = new Point(customBounds.Left + customBounds.Width / 2, customBounds.Top + customBounds.Height / 2);
                var m = Matrix.Identity;

                m.RotateAt(-angle, center.X, center.Y);
                mousePosition = m.Transform(mousePosition);
            }
            //double leftBorder = customBounds.Left;
            //double topBorder = customBounds.Top;
            //double rightBorder = leftBorder + customBounds.Width;
            //double bottomBorder = topBorder + customBounds.Height;
            double leftBorder = customBounds.Left;
            double topBorder = customBounds.Top;
            double rightBorder = customBounds.Right;
            double bottomBorder = customBounds.Bottom;
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
        public Rect TransformRectangle(Rect originalBounds, HitType hitType, double offset_x, double offset_y,double angle = 0)
        {
            var inv = Matrix.Identity;
            inv.Rotate(-angle);
            Vector d = inv.Transform(new Vector(offset_x, offset_y));

            double new_x = originalBounds.Left;
            double new_y = originalBounds.Top;
            double new_width = originalBounds.Width;
            double new_height = originalBounds.Height;

            switch (hitType)
            {
                case HitType.UpLeft:
                    new_x += d.X;
                    new_y += d.Y;
                    new_width -= d.X;
                    new_height -= d.Y;
                    break;
                case HitType.UpRight:
                    new_y += d.Y;
                    new_width += d.X;
                    new_height -= d.Y;
                    break;
                case HitType.BottomRight:
                    new_width += d.X;
                    new_height += d.Y;
                    break;
                case HitType.BottomLeft:
                    new_x += d.X;
                    new_width -= d.X;
                    new_height += d.Y;
                    break;
                case HitType.Left:
                    new_x += d.X;
                    new_width -= d.X;
                    break;
                case HitType.Right:
                    new_width += d.X;
                    break;
                case HitType.Bottom:
                    new_height += d.Y;
                    break;
                case HitType.Top:
                    new_y += d.Y;
                    new_height -= d.Y;
                    break;
            }
            if (new_width <= 0 || new_height <= 0) return originalBounds;
            if (angle == 0)
            {
                return new Rect(new_x,new_y,new_width, new_height);
            }
            var oldCenter = new Point(originalBounds.Left + originalBounds.Width / 2, originalBounds.Top + originalBounds.Height / 2);
            var localShift = new Vector((new_x + new_width / 2) - oldCenter.X,(new_y + new_height / 2) - oldCenter.Y);

            var rot = Matrix.Identity;
            rot.Rotate(angle);
            Vector shift = rot.Transform(localShift);

            var newCenter = oldCenter + shift;
            return new Rect(newCenter.X - new_width / 2, newCenter.Y - new_height/2, new_width, new_height);
        }
        public AddressPosition ChangeRectPosition(Rect moveObject, Rect staticObject, AddressPosition pos)
        {
            if (!moveObject.IntersectsWith(staticObject)) return pos;
            //какая сторона пересеклась
            CollisionSide collisionSide = GetCollisionSide(moveObject, staticObject);

            // Выполняем смену позиции по X
            if (collisionSide == CollisionSide.Right)
            {
                return AddressPosition.Left;
            }
            else if (collisionSide == CollisionSide.Left)
            {
                return AddressPosition.Right;
            }
            // Выполняем смену позиции по Y
            if (collisionSide == CollisionSide.Bottom)
            {
                return AddressPosition.Top;
            }
            else if (collisionSide == CollisionSide.Top)
            {
                return AddressPosition.Bottom;
            }

            return AddressPosition.None;
        }


        public void NoCollisionWithRect(FrameworkElement ui, Rect moveObject, Rect staticObject,Rect centerRect)
        {
            if (moveObject.IntersectsWith(staticObject)) return;
            int leftSet = Convert.ToInt32(-ui.ActualWidth-5);
            int rightSet = Convert.ToInt32(centerRect.Width+5);
            int topSet = Convert.ToInt32(-ui.ActualHeight-5);
            int bottomSet = Convert.ToInt32(centerRect.Height+5);
            //какая сторона вышла из пересечения
            bool objLeft = moveObject.X < staticObject.Left;
            bool objRight = moveObject.X > staticObject.Right;
            bool objTop = moveObject.Y < staticObject.Top;
            bool objBottom = moveObject.Y > staticObject.Bottom;


            if (objLeft)
            {
                Canvas.SetLeft(ui, rightSet);
            }
            else if (objRight)
            {
                Canvas.SetLeft(ui, leftSet);
            }
            if (objTop)
            {
                Canvas.SetTop(ui, bottomSet);
            }
            else if (objBottom)
            {
                Canvas.SetTop(ui, topSet);
            }

        }
        private CollisionSide GetCollisionSide(Rect moveObject, Rect staticObject)
        {
            // Вычисляем глубину проникновения по осям
            double penetrationX = Math.Min(moveObject.Right - staticObject.Left, staticObject.Right - moveObject.Left);
            double penetrationY = Math.Min(moveObject.Bottom - staticObject.Top, staticObject.Bottom - moveObject.Top);

            // Определяем сторону по минимальной глубине проникновения
            if (penetrationX < penetrationY)
            {
                // Столкновение по горизонтали
                return (moveObject.Left < staticObject.Left) ? CollisionSide.Right : CollisionSide.Left;
            }
            else
            {
                // Столкновение по вертикали
                return (moveObject.Top < staticObject.Top) ? CollisionSide.Bottom : CollisionSide.Top;
            }
        }
        
        ///// <summary>
        ///// Контролирование выхода адреса за границу схемы.
        ///// </summary>
        ///// <param name="leftSet"></param>
        ///// <param name="rightSet"></param>
        ///// <param name="topSet"></param>
        ///// <param name="bottomSet"></param>
        public bool IsRectWillIntersect(Rect current, Rect other, double moveX,double moveY)
        {
            // Создаем гипотетический прямоугольник после перемещения
            var futurePosition = new Rect(
                current.X + moveX, 
                current.Y + moveY,
                current.Width,
                current.Height);

            return futurePosition.IntersectsWith(other);
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
