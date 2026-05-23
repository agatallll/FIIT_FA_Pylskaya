using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
{
    public RedBlackTree() : this(null) { } // конструктор без параметров для generic создания
    public RedBlackTree(IComparer<TKey>? comparer) : base(comparer) { } // конструктор с компаратором
    
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value); // фабрика узлов
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode) { } // хук для будущей балансировки
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child) { } // хук для будущей балансировки
}
