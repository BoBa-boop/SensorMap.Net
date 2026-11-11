using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace SensorMap.Behaviors
{
    public class GroupState
    {
        //    «Свернуть все» меняет значение по умолчанию и сбрасывает исключения.Уже созданные Expander сворачиваются сразу.
        //Группы, которых пока нет в визуальном дереве, получат нужное состояние в Loaded при создании контейнера, когда до них докрутят.
        //Если пользователь вручную раскрыл или свернул группу, это запоминается. При прокрутке туда-обратно состояние не сбрасывается.
        //IsExpanded в шаблоне по умолчанию false, пока не сработает Loaded, поэтому события Expanded/Collapsed не затирают состояние.
        public bool DefaultExpanded { get; private set; } = true;
        private readonly HashSet<string> _overrides = new();

        // состояние группы = значение по умолчанию, инвертированное, если группа в исключениях
        public bool IsExpanded(string key) => DefaultExpanded ^ _overrides.Contains(key);

        public void Set(string key, bool expanded)
        {
            if (expanded == DefaultExpanded) _overrides.Remove(key);
            else _overrides.Add(key);
        }

        public void Reset(bool expand)
        {
            DefaultExpanded = expand;
            _overrides.Clear();
        }
    }

    public static class GroupExpansion
    {
        public static readonly DependencyProperty TrackProperty =
            DependencyProperty.RegisterAttached("Track", typeof(bool), typeof(GroupExpansion),
                new PropertyMetadata(false, OnTrackChanged));

        public static bool GetTrack(DependencyObject d) => (bool)d.GetValue(TrackProperty);
        public static void SetTrack(DependencyObject d, bool v) => d.SetValue(TrackProperty, v);

        private static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached("State", typeof(GroupState), typeof(GroupExpansion));

        public static GroupState GetState(DataGrid grid)
        {
            if (grid.GetValue(StateProperty) is not GroupState s)
            {
                s = new GroupState();
                grid.SetValue(StateProperty, s);
            }
            return s;
        }

        private static void OnTrackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Expander exp) return;

            exp.Loaded -= OnLoaded;
            exp.DataContextChanged -= OnDataContextChanged;
            exp.Expanded -= OnToggled;
            exp.Collapsed -= OnToggled;

            if ((bool)e.NewValue)
            {
                exp.Loaded += OnLoaded;
                exp.DataContextChanged += OnDataContextChanged;
                exp.Expanded += OnToggled;
                exp.Collapsed += OnToggled;
            }
        }

        private static void OnLoaded(object s, RoutedEventArgs e) => Apply((Expander)s);
        private static void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e) => Apply((Expander)s);

        // контейнер создан (в т.ч. при прокрутке) -> применяем сохранённое состояние
        private static void Apply(Expander exp)
        {
            if (exp.DataContext is not CollectionViewGroup g) return;
            var grid = FindGrid(exp);
            if (grid == null) return;

            var want = GetState(grid).IsExpanded(Key(g));
            if (exp.IsExpanded != want)
                exp.IsExpanded = want;
        }

        // пользователь (или SetAll) изменил группу -> запоминаем
        private static void OnToggled(object s, RoutedEventArgs e)
        {
            var exp = (Expander)s;
            if (!ReferenceEquals(e.OriginalSource, exp)) return;
            if (exp.DataContext is not CollectionViewGroup g) return;

            var grid = FindGrid(exp);
            if (grid == null) return;

            GetState(grid).Set(Key(g), exp.IsExpanded);
        }

        private static string Key(CollectionViewGroup g) => g.Name?.ToString() ?? string.Empty;

        private static DataGrid FindGrid(DependencyObject d)
        {
            while (d != null && d is not DataGrid)
                d = VisualTreeHelper.GetParent(d);
            return d as DataGrid;
        }
    }
}
