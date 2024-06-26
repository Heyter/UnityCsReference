// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;

namespace UnityEngine.UIElements.UIR
{
    class LinkedPoolItem<T>
    {
        internal T poolNext;
    }

    class LinkedPool<T> where T : LinkedPoolItem<T>
    {
        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_ResetAction;
        readonly int m_Limit; // Used to prevent catastrophic memory retention.
        T m_PoolFirst;

        public LinkedPool(Func<T> createFunc, Action<T> resetAction, int limit = 10000)
        {
            Debug.Assert(createFunc != null);
            m_CreateFunc = createFunc;
            m_ResetAction = resetAction;

            Debug.Assert(limit > 0);
            m_Limit = limit;
        }

        public int Count { get; private set; }

        public void Clear()
        {
            m_PoolFirst = null;
            Count = 0;
        }

        public T Get()
        {
            var item = m_PoolFirst;

            if (!ReferenceEquals(m_PoolFirst, null))
            {
                --Count;
                m_PoolFirst = item.poolNext;
                m_ResetAction?.Invoke(item);
            }
            else
                item = m_CreateFunc();

            return item;
        }

        public void Return(T item)
        {
            if (Count < m_Limit)
            {
                item.poolNext = m_PoolFirst;
                m_PoolFirst = item;
                ++Count;
            }
        }
    }

    class BasicNode<T> : LinkedPoolItem<BasicNode<T>>
    {
        public BasicNode<T> next;
        public T data;

        public void InsertFirst(ref BasicNode<T> first)
        {
            if (first == null)
            {
                first = this;
                return;
            }

            this.next = first.next;
            first.next = this;
        }
    }

    class BasicNodePool<T> : LinkedPool<BasicNode<T>>
    {
        static void Reset(BasicNode<T> node)
        {
            node.next = null;
            node.data = default(T);
        }

        static BasicNode<T> Create()
        {
            return new BasicNode<T>();
        }

        public BasicNodePool() : base(Create, Reset)
        {}
    }
}
