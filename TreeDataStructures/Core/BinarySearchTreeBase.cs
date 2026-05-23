using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root; // корень дерева
    public IComparer<TKey> Comparer { get; protected set; } = comparer ?? Comparer<TKey>.Default; // компаратор для сравнения ключей
    public int Count { get; protected set; } // текущее количество узлов
    public bool IsReadOnly => false; // коллекция доступна для изменений

    public ICollection<TKey> Keys => new List<TKey>(InOrder().Select(e => e.Key)); // ключи в порядке возрастания
    public ICollection<TValue> Values => new List<TValue>(InOrder().Select(e => e.Value)); // значения в порядке возрастания ключей

    public virtual void Add(TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key)); // валидация ключа на null
        if (Root == null) // дерево пустое
        {
            Root = CreateNode(key, value); // создаем корневой узел
            Count++; // увеличиваем счетчик
            OnNodeAdded(Root); // вызываем хук после вставки
            return;
        }
        TNode? current = Root; // начинаем поиск с корня
        TNode? parent = null; // родитель текущего узла
        int cmp = 0; // результат сравнения
        while (current != null) // ищем место вставки
        {
            parent = current;
            cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) // ключ уже существует
            {
                current.Value = value; // обновляем значение
                return;
            }
            current = cmp < 0 ? current.Left : current.Right; // двигаемся влево или вправо
        }
        TNode newNode = CreateNode(key, value); // создаем новый узел
        newNode.Parent = parent; // устанавливаем родителя
        if (cmp < 0) parent!.Left = newNode; // вставляем как левого ребенка
        else parent!.Right = newNode; // вставляем как правого ребенка
        Count++; // увеличиваем счетчик
        OnNodeAdded(newNode); // вызываем хук после вставки
    }

    public virtual bool Remove(TKey key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key)); // валидация ключа на null
        TNode? node = FindNode(key); // ищем узел для удаления
        if (node == null) return false; // узел не найден
        RemoveNode(node); // удаляем узел
        Count--; // уменьшаем счетчик
        return true;
    }

    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null) // нет левого ребенка
        {
            TNode? parent = node.Parent; // запоминаем родителя до трансплантации
            Transplant(node, node.Right); // заменяем правым ребенком
            OnNodeRemoved(parent, node.Right); // вызываем хук
        }
        else if (node.Right == null) // нет правого ребенка
        {
            TNode? parent = node.Parent;
            Transplant(node, node.Left); // заменяем левым ребенком
            OnNodeRemoved(parent, node.Left); // вызываем хук
        }
        else // два ребенка
        {
            TNode successor = Minimum(node.Right); // минимум в правом поддереве
            if (successor.Parent != node) // преемник не является прямым ребенком
            {
                TNode? succParent = successor.Parent; // запоминаем родителя преемника
                Transplant(successor, successor.Right); // поднимаем правого ребенка преемника
                successor.Right = node.Right; // правый ребенок удаляемого становится правым ребенком преемника
                successor.Right.Parent = successor; // обновляем родителя
                OnNodeRemoved(succParent, successor.Right); // хук за промежуточное изменение
            }
            TNode? nodeParent = node.Parent; // родитель удаляемого узла
            Transplant(node, successor); // заменяем удаляемый узел преемником
            successor.Left = node.Left; // левый ребенок удаляемого становится левым ребенком преемника
            successor.Left.Parent = successor; // обновляем родителя
            OnNodeRemoved(nodeParent, successor); // хук за основное удаление
        }
    }

    public virtual bool ContainsKey(TKey key) => FindNode(key) != null; // проверка наличия ключа

    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        TNode? node = FindNode(key); // ищем узел
        if (node != null)
        {
            value = node.Value; // возвращаем значение
            return true;
        }
        value = default; // значение по умолчанию
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out TValue? val) ? val : throw new KeyNotFoundException(); // получение с проверкой
        set => Add(key, value); // установка через добавление/обновление
    }

    #region Hooks

    protected virtual void OnNodeAdded(TNode newNode) { } // хук после вставки (переопределяется в наследниках)

    protected virtual void OnNodeRemoved(TNode? parent, TNode? child) { } // хук после удаления

    #endregion

    #region Helpers

    protected abstract TNode CreateNode(TKey key, TValue value); // фабрика узлов

    protected TNode? FindNode(TKey key)
    {
        TNode? current = Root; // начинаем с корня
        while (current != null)
        {
            int cmp = Comparer.Compare(key, current.Key); // сравниваем ключи
            if (cmp == 0) return current; // нашли
            current = cmp < 0 ? current.Left : current.Right; // двигаемся дальше
        }
        return null; // не нашли
    }

    protected TNode Minimum(TNode node)
    {
        while (node.Left != null) node = node.Left; // идем к самому левому узлу
        return node;
    }

    protected void RotateLeft(TNode x)
    {
        TNode? y = x.Right; // правый ребенок вокруг которого вращаем
        if (y == null) return; // вращение невозможно
        x.Right = y.Left; // левое поддерево y становится правым поддеревом x
        if (y.Left != null) y.Left.Parent = x; // обновляем родителя
        y.Parent = x.Parent; // y занимает место x
        if (x.Parent == null) Root = y; // x был корнем
        else if (x.IsLeftChild) x.Parent.Left = y; // x был левым ребенком
        else x.Parent.Right = y; // x был правым ребенком
        y.Left = x; // x становится левым ребенком y
        x.Parent = y; // обновляем родителя x
    }

    protected void RotateRight(TNode y)
    {
        TNode? x = y.Left; // левый ребенок вокруг которого вращаем
        if (x == null) return; // вращение невозможно
        y.Left = x.Right; // правое поддерево x становится левым поддеревом y
        if (x.Right != null) x.Right.Parent = y; // обновляем родителя
        x.Parent = y.Parent; // x занимает место y
        if (y.Parent == null) Root = x; // y был корнем
        else if (y.IsLeftChild) y.Parent.Left = x; // y был левым ребенком
        else y.Parent.Right = x; // y был правым ребенком
        x.Right = y; // y становится правым ребенком x
        y.Parent = x; // обновляем родителя y
    }

    protected void RotateDoubleLeft(TNode x)
    {
        if (x.Right == null) return; // нет правого ребенка для двойного поворота
        RotateRight(x.Right); // сначала малый правый поворот правого ребенка
        RotateLeft(x); // затем малый левый поворот
    }

    protected void RotateDoubleRight(TNode y)
    {
        if (y.Left == null) return; // нет левого ребенка для двойного поворота
        RotateLeft(y.Left); // сначала малый левый поворот левого ребенка
        RotateRight(y); // затем малый правый поворот
    }

    protected void RotateBigLeft(TNode x)
    {
        RotateDoubleLeft(x); // большой левый поворот реализован через двойной
    }

    protected void RotateBigRight(TNode y)
    {
        RotateDoubleRight(y); // большой правый поворот реализован через двойной
    }

    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null) Root = v; // u был корнем
        else if (u.IsLeftChild) u.Parent.Left = v; // u был левым ребенком
        else u.Parent.Right = v; // u был правым ребенком
        if (v != null) v.Parent = u.Parent; // обновляем родителя v
    }

    #endregion

    public IEnumerable<TreeEntry<TKey, TValue>> InOrder() => new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder() => new TreeIterator(Root, TraversalStrategy.PreOrder, GetNodeHeight);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder() => new TreeIterator(Root, TraversalStrategy.PostOrder, GetNodeHeight);
    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse() => new TreeIterator(Root, TraversalStrategy.InOrderReverse, GetNodeHeight);
    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse() => new TreeIterator(Root, TraversalStrategy.PreOrderReverse, GetNodeHeight);
    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse() => new TreeIterator(Root, TraversalStrategy.PostOrderReverse, GetNodeHeight);

    protected virtual int GetNodeHeight(TNode node)
    {
        int left = node.Left == null ? 0 : GetNodeHeight(node.Left); // высота левого поддерева
        int right = node.Right == null ? 0 : GetNodeHeight(node.Right); // высота правого поддерева
        return 1 + Math.Max(left, right); // высота текущего узла
    }

    private class TreeIterator : IEnumerable<TreeEntry<TKey, TValue>>, IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly TNode? _root; // корень поддерева для обхода
        private readonly TraversalStrategy _strategy; // стратегия обхода
        private readonly Func<TNode, int> _heightProvider; // функция вычисления высоты
        private readonly Stack<TNode> _stack; // стек для обхода
        private readonly Stack<bool> _visited; // флаг посещения для постфиксного обхода
        private readonly Dictionary<TNode, int> _heightCache; // кэш высот
        private TreeEntry<TKey, TValue> _current; // текущий элемент

        public TreeIterator(TNode? root, TraversalStrategy strategy, Func<TNode, int> heightProvider)
        {
            _root = root;
            _strategy = strategy;
            _heightProvider = heightProvider;
            _stack = new Stack<TNode>();
            _visited = new Stack<bool>();
            _heightCache = new Dictionary<TNode, int>();
            Reset();
        }

        public TreeEntry<TKey, TValue> Current => _current; // текущий элемент итерации
        object IEnumerator.Current => _current; // нетипизированный доступ

        public bool MoveNext()
        {
            switch (_strategy)
            {
                case TraversalStrategy.InOrder:
                    return MoveNextInOrder(leftFirst: true);
                case TraversalStrategy.InOrderReverse:
                    return MoveNextInOrder(leftFirst: false);
                case TraversalStrategy.PreOrder:
                    return MoveNextPreOrder(reverse: false);
                case TraversalStrategy.PreOrderReverse:
                    return MoveNextPreOrder(reverse: true);
                case TraversalStrategy.PostOrder:
                    return MoveNextPostOrder(reverse: false);
                case TraversalStrategy.PostOrderReverse:
                    return MoveNextPostOrder(reverse: true);
                default:
                    return false;
            }
        }

        private bool MoveNextInOrder(bool leftFirst)
        {
            if (_stack.Count == 0) return false; // обход завершен
            TNode node = _stack.Pop(); // берем следующий узел
            _current = CreateEntry(node); // формируем запись
            TNode? next = leftFirst ? node.Right : node.Left; // следующая ветвь
            PushAlongSide(next, leftFirst); // заполняем стек
            return true;
        }

        private void PushAlongSide(TNode? node, bool leftFirst)
        {
            while (node != null) // идем до конца ветви
            {
                _stack.Push(node); // кладем в стек
                node = leftFirst ? node.Left : node.Right; // двигаемся дальше
            }
        }

        private bool MoveNextPreOrder(bool reverse)
        {
            if (_stack.Count == 0) return false; // обход завершен
            TNode node = _stack.Pop(); // берем следующий узел
            _current = CreateEntry(node); // формируем запись
            if (reverse)
            {
                if (node.Left != null) _stack.Push(node.Left); // сначала левый
                if (node.Right != null) _stack.Push(node.Right); // затем правый (будет сверху)
            }
            else
            {
                if (node.Right != null) _stack.Push(node.Right); // сначала правый
                if (node.Left != null) _stack.Push(node.Left); // затем левый (будет сверху)
            }
            return true;
        }

        private bool MoveNextPostOrder(bool reverse)
        {
            while (_stack.Count > 0)
            {
                TNode node = _stack.Peek(); // смотрим верхушку
                bool visited = _visited.Peek();
                if (!visited)
                {
                    _visited.Pop(); // снимаем старый флаг
                    _visited.Push(true); // помечаем как посещенный
                    if (reverse)
                    {
                        if (node.Left != null) { _stack.Push(node.Left); _visited.Push(false); } // сначала левый
                        if (node.Right != null) { _stack.Push(node.Right); _visited.Push(false); } // затем правый
                    }
                    else
                    {
                        if (node.Right != null) { _stack.Push(node.Right); _visited.Push(false); } // сначала правый
                        if (node.Left != null) { _stack.Push(node.Left); _visited.Push(false); } // затем левый
                    }
                }
                else
                {
                    _stack.Pop(); // удаляем обработанный узел
                    _visited.Pop();
                    _current = CreateEntry(node); // формируем запись
                    return true;
                }
            }
            return false; // обход завершен
        }

        public void Reset()
        {
            _stack.Clear(); // очищаем стек
            _visited.Clear(); // очищаем флаги
            _heightCache.Clear(); // сбрасываем кэш
            switch (_strategy)
            {
                case TraversalStrategy.InOrder:
                    PushAlongSide(_root, leftFirst: true);
                    break;
                case TraversalStrategy.InOrderReverse:
                    PushAlongSide(_root, leftFirst: false);
                    break;
                case TraversalStrategy.PreOrder:
                case TraversalStrategy.PreOrderReverse:
                    if (_root != null) _stack.Push(_root);
                    break;
                case TraversalStrategy.PostOrder:
                case TraversalStrategy.PostOrderReverse:
                    if (_root != null) { _stack.Push(_root); _visited.Push(false); }
                    break;
            }
        }

        public void Dispose() { } // освобождение ресурсов не требуется

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator() => this; // возвращаем себя как итератор
        IEnumerator IEnumerable.GetEnumerator() => this; // нетипизированный доступ

        private TreeEntry<TKey, TValue> CreateEntry(TNode node)
        {
            if (!_heightCache.TryGetValue(node, out int height)) // проверяем кэш
            {
                height = _heightProvider(node); // вычисляем высоту
                _heightCache[node] = height; // сохраняем в кэш
            }
            return new TreeEntry<TKey, TValue>(node.Key, node.Value, height); // формируем запись
        }
    }

    private enum TraversalStrategy { InOrder, PreOrder, PostOrder, InOrderReverse, PreOrderReverse, PostOrderReverse }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new TreeIterator(Root, TraversalStrategy.InOrder, GetNodeHeight)
            .Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value))
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value); // делегируем пару на вставку
    public void Clear() { Root = null; Count = 0; } // очищаем дерево
    public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key); // проверка наличия
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) // копирование в массив
    {
        if (array == null) throw new ArgumentNullException(nameof(array)); // проверка массива
        if (arrayIndex < 0 || arrayIndex > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex)); // проверка индекса
        if (array.Length - arrayIndex < Count) throw new ArgumentException("Недостаточно места в массиве"); // проверка размера
        foreach (var entry in InOrder())
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value); // заполняем массив
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key); // удаление по паре
}
