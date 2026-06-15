using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static SensorMap.Services.TransformObjectService;
using Cursor = System.Windows.Input.Cursor;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SensorMap.Interfaces
{ 
    /// <summary>
  /// Тип области прямоугольника под курсором
  /// </summary>
    public enum HitType
    {
        None, Body, UpLeft, UpRight, BottomRight, BottomLeft, Left, Right, Top, Bottom
    }
    public interface ITransformObject
    {
        /// <summary>
        /// Определяет тип области прямоугольника под точкой
        /// </summary>
        /// <param name="customBounds">Границы прямоугольника (X, Y, Width, Height)</param>
        /// <param name="gap">Допустимое расстояние до границы (в единицах координат)</param>
        /// <returns>Тип области</returns>
        HitType GetHitType(Rect customBounds, Point mousePosition, double gap = 3);

        /// <summary>
        /// Получает курсор для типа области
        /// </summary>
        /// <param name="hitType">Тип области</param>
        /// <returns>Соответствующий курсор</returns>
        Cursor GetCursorForHitType(HitType hitType);

        /// <summary>
        /// Выполняет трансформацию прямоугольника
        /// </summary>
        /// <param name="originalBounds">Исходные границы</param>
        /// <param name="hitType">Тип трансформации</param>
        /// <param name="offsetX">Смещение по X</param>
        /// <param name="offsetY">Смещение по Y</param>
        /// <returns>Новые границы прямоугольника или null, если трансформация недопустима</returns>
        Rect? TransformRectangle(Rect originalBounds, HitType hitType, double offsetX, double offsetY);
        /// <summary>
        /// Изменить позицию адреса в случае пересечения с другим объектом
        /// </summary>
        //AddressPosition ChangeRectPosition(Rect rect1,Rect rect2,AddressPosition pos = AddressPosition.None);
        /// <summary>
        /// Отсутсвие коллизии прямоугольников, если ChangePosiotion = true перемещение rect1 для создания пересечения.
        /// </summary>
        /// <param name="rect1"></param>
        /// <param name="rect2"></param>
        /// /// <param name="rect3">Относительно чего происходит смещение</param>
        void NoCollisionWithRect(FrameworkElement ui,Rect rect1, Rect rect2,Rect rect3);
        bool IsRectWillIntersect(Rect current, Rect other, double moveX = 0, double moveY = 0);
        Canvas GetParentCanvas(UIElement element);
        Point ScreenToWorld(Point point,Matrix matrix);
        Point WorldToScreen(Point point, Matrix matrix);
    }
}
