using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.BST;

public class BinarySearchTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, BstNode<TKey, TValue>>
{
    public BinarySearchTree() : this(null) { } // конструктор без параметров для generic создания
    public BinarySearchTree(IComparer<TKey>? comparer) : base(comparer) { } // конструктор с компаратором
    
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value); // фабрика узлов
}
