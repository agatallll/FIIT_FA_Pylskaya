using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    public AvlTree() : this(null) { } // конструктор без параметров для generic создания
    public AvlTree(IComparer<TKey>? comparer) : base(comparer) { } // конструктор с компаратором
    
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value); // фабрика узлов
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode) { } // хук для будущей балансировки
}
