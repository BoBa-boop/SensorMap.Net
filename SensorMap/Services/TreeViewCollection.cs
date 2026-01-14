using System.Collections.ObjectModel;

namespace SensorMap.Services
{
    public class TreeNode<TChild>
    {
        public string Name { get; set; }
        public ICollection<TreeNode<TChild>> Children { get; set; } = new List<TreeNode<TChild>>();
        public TChild Data { get; set; }

        public TreeNode(string parentName, TChild data = default)
        {
            Name = parentName;
            Data = data;
        }
    }
    public class TreeViewCollection<TParent, TChild>
    {
        private readonly ObservableCollection<TParent> _parentCollection;
        private readonly ObservableCollection<TChild> _childCollection;
        private readonly string _titleProp;
        private readonly Func<TParent, TChild, bool> _filter;

        public ICollection<TreeNode<TChild>> Nodes { get; private set; }

        public TreeViewCollection(string TitleProp,ObservableCollection<TParent> parentCollection, ObservableCollection<TChild> childCollection, Func<TParent,TChild,bool> filter)
        {
            _parentCollection = parentCollection;
            _childCollection = childCollection;
            _titleProp = TitleProp;
            _filter = filter;
            BuildTree();
        }

        private void BuildTree()
        {
            Nodes = new List<TreeNode<TChild>>();

            // Формируем начальные узлы дерева (родителей)
            foreach (var parent in _parentCollection)
            {
                Nodes.Add(new TreeNode<TChild>(GetDisplayNameFromObject(parent)));
            }

            // Заполняем связи между родителями и детьми согласно переданному условию
            foreach (var child in _childCollection)
            {
                var matchingParentNode = Nodes.FirstOrDefault(n => _parentCollection.Where(p => GetDisplayNameFromObject(p) == n.Name).Any(p => _filter(p, child)));
                if (matchingParentNode != null)
                {
                    var childNode = new TreeNode<TChild>(matchingParentNode.Name,child);
                    matchingParentNode.Children.Add(childNode);
                }
            }
        }
        private string GetDisplayNameFromObject(object obj)
        {
            if (obj != null)
            {
                var properties = obj.GetType().GetProperties();
                var foundSpecialProp = properties.FirstOrDefault(prop => _titleProp.Contains(prop.Name));
                if (foundSpecialProp != null && foundSpecialProp.PropertyType == typeof(string))
                {
                    return (string)foundSpecialProp.GetValue(obj);
                }
                else if(obj is string parent)
                {
                    return parent;
                }
            }
            return "Пусто";


        }
    }
}
