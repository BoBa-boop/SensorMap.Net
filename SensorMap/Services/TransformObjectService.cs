using SensorMap.CustomControls;
using SensorMap.Interfaces;
using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cursor = System.Windows.Input;
using Cursors = System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
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
        public enum CollisionSide { None, Left, Right, Top, Bottom }

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
        private int numTry = 4;
        public void CollisionWithRect(FrameworkElement ui, Rect moveObject, Rect staticObject, Rect centerRect)
        {
            if (!moveObject.IntersectsWith(staticObject)) return;
            //какая сторона пересеклась
            CollisionSide collisionSide = GetCollisionSide(moveObject, staticObject);
            //координаты сторон
            bool objLeft = moveObject.X < centerRect.Left;
            bool objRight = moveObject.X > centerRect.Right;
            bool objTop = moveObject.Y < centerRect.Top;
            bool objBottom = moveObject.Y > centerRect.Bottom;
            int leftSet = Convert.ToInt32(-ui.ActualWidth - 5);
            int rightSet = Convert.ToInt32(centerRect.Width + 5);
            int topSet = Convert.ToInt32(-ui.ActualHeight - 5);
            int bottomSet = Convert.ToInt32(centerRect.Height + 5);

            // Выполняем смену позиции по X
            if (collisionSide == CollisionSide.Right)
            {
                if (objTop || objBottom) Canvas.SetTop(ui, -(centerRect.Width - 40)/2);
                Canvas.SetLeft(ui, leftSet);
                return;
            }
            else if (collisionSide == CollisionSide.Left)
            {
                if (objTop || objBottom) Canvas.SetTop(ui, -(centerRect.Width - 40) / 2);
                Canvas.SetLeft(ui, rightSet);
                return;
            }
            // Выполняем смену позиции по Y
            if (collisionSide == CollisionSide.Bottom)
            {
                if (objLeft || objRight) Canvas.SetLeft(ui, -(centerRect.Height - 15) / 2);
                Canvas.SetTop(ui, topSet);
                return;
            }
            else if (collisionSide == CollisionSide.Top)
            {
                if (objLeft || objRight) Canvas.SetLeft(ui, -(centerRect.Height - 15) / 2);
                Canvas.SetTop(ui, bottomSet);
                return;
            }
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
                Canvas.SetLeft(ui, topSet);
            }
            else if (objBottom)
            {
                Canvas.SetLeft(ui, bottomSet);
            }

        }
        private CollisionSide GetCollisionSide(Rect moveObject, Rect staticObject)
        {
            // 1. Проверяем факт пересечения
            if (!moveObject.IntersectsWith(staticObject))
            {
                return CollisionSide.None;
            }

            var intersection = Rect.Intersect(moveObject, staticObject);
            
            bool objLeft = (moveObject.Left >= intersection.Right - 2) &&
                           (moveObject.Left <= intersection.Right + 2);

            bool objRight = (moveObject.Right >= intersection.Left - 2) &&
                            (moveObject.Right <= intersection.Left + 2);

            bool objTop = (moveObject.Top >= intersection.Bottom - 2) &&
                          (moveObject.Top <= intersection.Bottom + 2);

            bool objBottom = (moveObject.Bottom >= intersection.Top - 2) &&
                             (moveObject.Bottom <= intersection.Top + 2);

            if (objTop) return CollisionSide.Top;
            if (objBottom) return CollisionSide.Bottom;
            if (objLeft) return CollisionSide.Left;
            if (objRight) return CollisionSide.Right;
            return CollisionSide.None;
        }
        //public void ChangeAddressPosition()
        //{


        //    ControlOutOfRangeImage(leftSet, rightSet, topSet, bottomSet);

        //    addressRect = new Rect(Canvas.GetLeft(_textBlock) + CustomBounds.X, Canvas.GetTop(_textBlock) + CustomBounds.Y, _textBlock.ActualWidth, _textBlock.ActualHeight);
        //    ControlAddressCollision(leftSet, rightSet, topSet, bottomSet);

        //}
        ///// <summary>
        ///// Контролирование накладывание адрессов друг на друга. Перемещение адреса выбранного датчика в свободную позицию
        ///// </summary>
        ///// <param name="leftSet"></param>
        ///// <param name="rightSet"></param>
        ///// <param name="topSet"></param>
        ///// <param name="bottomSet"></param>
        //private void ControlAddressCollision(int leftSet, int rightSet, int topSet, int bottomSet)
        //{
        //    Rect searchArea = Rect.Union(addressRect, CustomBounds);//зона поиска адресов
        //    searchArea.Inflate(20, 20);
        //    var collectionsIntersects = _canvas.Children.OfType<CustomSensor>()
        //        .Where(s => s != this && s._textBlock.Visibility != Visibility.Collapsed && s.addressRect.IntersectsWith(searchArea)).ToList();
        //    if (!collectionsIntersects.Any()) return;
        //    bool isPositionFixed = false;

        //    // --- ЭТАП 1: Попытки смещения ПО ГОРИЗОНТАЛИ ---
        //    for (int attempt = 0; attempt < numTry && !isPositionFixed; attempt++)
        //    {
        //        // Пересчитываем пересечения на КАЖДОЙ попытке, так как позиция изменилась
        //        var inter = collectionsIntersects.Where(s =>
        //            this.addressRect.IntersectsWith(s.CustomBounds) ||
        //            this.addressRect.IntersectsWith(s.addressRect)).ToList();

        //        if (!inter.Any())
        //        {
        //            // Горизонтальное смещение помогло
        //            isPositionFixed = true;
        //            break;
        //        }

        //        // Выполняем смену позиции по X
        //        if (AddressRight || (!AddressLeft && !AddressRight))
        //        {
        //            // Пробуем переместить влево
        //            Canvas.SetLeft(_textBlock, leftSet);
        //            AddressRight = false;
        //            AddressLeft = true;
        //        }
        //        else if (AddressLeft)
        //        {
        //            // Пробуем переместить вправо
        //            Canvas.SetLeft(_textBlock, rightSet);
        //            AddressLeft = false;
        //            AddressRight = true;
        //        }

        //        // Обновляем адресный прямоугольник после изменения позиции
        //        UpdateAddressRect();
        //    }

        //    // --- ЭТАП 2: Попытки смещения ПО ВЕРТИКАЛИ ---
        //    // Начинаем только если горизонтальные попытки не помогли
        //    if (!isPositionFixed)
        //    {
        //        for (int attempt = 0; attempt < numTry && !isPositionFixed; attempt++)
        //        {
        //            // Снова пересчитываем пересечения
        //            var inter = collectionsIntersects.Where(s =>
        //                this.addressRect.IntersectsWith(s.CustomBounds) ||
        //                this.addressRect.IntersectsWith(s.addressRect)).ToList();

        //            if (!inter.Any())
        //            {
        //                // Вертикальное смещение помогло
        //                isPositionFixed = true;
        //                break;
        //            }

        //            // Выполняем смену позиции по Y
        //            if (AddressBottom || (!AddressTop && !AddressBottom))
        //            {
        //                // Пробуем переместить вверх
        //                Canvas.SetTop(_textBlock, topSet);
        //                AddressBottom = false;
        //                AddressTop = true;
        //            }
        //            else if (AddressTop)
        //            {
        //                // Пробуем переместить вниз
        //                Canvas.SetTop(_textBlock, bottomSet);
        //                AddressTop = false;
        //                AddressBottom = true;
        //            }

        //            // Обновляем адресный прямоугольник
        //            UpdateAddressRect();
        //        }
        //    }

        //    // --- ПЛАН Б: Перемещение в центр, если ничего не помогло ---
        //    if (!isPositionFixed)
        //    {
        //        Canvas.SetLeft(_textBlock, (CustomBounds.Width - _textBlock.ActualWidth) / 2); // Более корректная центровка
        //        Canvas.SetTop(_textBlock, (CustomBounds.Height - _textBlock.ActualHeight) / 2);
        //        _textBlock.Opacity = 0.7; // Прозрачность, как индикатор неудачи
        //                                  // Сбрасываем флаги положения
        //        AddressLeft = false;
        //        AddressRight = false;
        //        AddressTop = false;
        //        AddressBottom = false;
        //        UpdateAddressRect();
        //    }
        //    else
        //    {
        //        _textBlock.Opacity = 1; // Возвращаем полную непрозрачность
        //    }
        //}
        ///// <summary>
        ///// Контролирование выхода адреса за границу схемы.
        ///// </summary>
        ///// <param name="leftSet"></param>
        ///// <param name="rightSet"></param>
        ///// <param name="topSet"></param>
        ///// <param name="bottomSet"></param>

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
