using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    public Treap() : this(null) { } // конструктор без параметров для generic создания
    public Treap(IComparer<TKey>? comparer) : base(comparer) { } // конструктор с компаратором
    
    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value); // фабрика узлов с случайным приоритетом
    
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode) { } // хук не требуется для декартова дерева
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child) { } // хук не требуется
    
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null) return (null, null); // база рекурсии
        if (Comparer.Compare(key, root.Key) < 0) // ключ меньше текущего
        {
            var (left, right) = Split(root.Left, key); // рекурсивно разрезаем левое поддерево
            root.Left = right; // правая часть становится левым ребенком
            if (right != null) right.Parent = root; // обновляем родителя
            if (left != null) left.Parent = null; // сбрасываем родителя корня левой части
            return (left, root);
        }
        else // ключ больше или равен текущему
        {
            var (left, right) = Split(root.Right, key); // рекурсивно разрезаем правое поддерево
            root.Right = left; // левая часть становится правым ребенком
            if (left != null) left.Parent = root; // обновляем родителя
            if (right != null) right.Parent = null; // сбрасываем родителя корня правой части
            return (root, right);
        }
    }
    
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right; // левое пусто
        if (right == null) return left; // правое пусто
        if (left.Priority > right.Priority) // приоритет левого больше
        {
            left.Right = Merge(left.Right, right); // правое поддерево сливаем с right
            if (left.Right != null) left.Right.Parent = left; // обновляем родителя
            return left;
        }
        else // приоритет правого больше или равен
        {
            right.Left = Merge(left, right.Left); // левое поддерево сливаем с left
            if (right.Left != null) right.Left.Parent = right; // обновляем родителя
            return right;
        }
    }
    
    public override void Add(TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key)); // валидация ключа на null
        var existing = FindNode(key); // проверяем существование
        if (existing != null)
        {
            existing.Value = value; // обновляем значение
            return;
        }
        var newNode = CreateNode(key, value); // создаем узел
        var (left, right) = Split(Root, key); // разрезаем дерево по ключу
        Root = Merge(Merge(left, newNode), right); // сливаем три части
        if (Root != null) Root.Parent = null; // сбрасываем родителя корня
        Count++; // увеличиваем счетчик
    }
    
    public override bool Remove(TKey key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key)); // валидация ключа на null
        var node = FindNode(key); // ищем узел
        if (node == null) return false; // не найден
        var merged = Merge(node.Left, node.Right); // сливаем детей
        Transplant(node, merged); // заменяем узел результатом слияния
        Count--; // уменьшаем счетчик
        return true;
    }
}
