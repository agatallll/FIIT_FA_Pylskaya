using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    public SplayTree() : this(null) { } // конструктор без параметров для generic создания
    public SplayTree(IComparer<TKey>? comparer) : base(comparer) { } // конструктор с компаратором
    
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value); // фабрика узлов
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode); // поднимаем вставленный узел к корню
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        // хук не требует дополнительных действий для базовых тестов
    }
    
    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key); // ищем узел
        if (node != null)
        {
            Splay(node); // поднимаем найденный узел к корню
            return true;
        }
        return false;
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key); // ищем узел
        if (node != null)
        {
            Splay(node); // поднимаем найденный узел к корню
            value = node.Value;
            return true;
        }
        value = default!;
        return false;
    }
    
    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null) // пока не достигли корня
        {
            var parent = node.Parent;
            var grandParent = parent.Parent;
            
            if (grandParent == null) // одиночный поворот (zig)
            {
                if (node.IsLeftChild) RotateRight(parent); // левый ребенок -> правый поворот
                else RotateLeft(parent); // правый ребенок -> левый поворот
            }
            else if (node.IsLeftChild && parent.IsLeftChild) // zig-zig (левый-левый)
            {
                RotateRight(grandParent);
                RotateRight(parent);
            }
            else if (node.IsRightChild && parent.IsRightChild) // zig-zig (правый-правый)
            {
                RotateLeft(grandParent);
                RotateLeft(parent);
            }
            else if (node.IsLeftChild && parent.IsRightChild) // zig-zag (левый-правый)
            {
                RotateRight(parent);
                RotateLeft(grandParent);
            }
            else // zig-zag (правый-левый)
            {
                RotateLeft(parent);
                RotateRight(grandParent);
            }
        }
    }
}
