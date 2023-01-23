// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace UnityEngine.UIElements
{
    internal class FixedHeightVirtualizationController<T> : VerticalVirtualizationController<T> where T : ReusableCollectionItem, new()
    {
        float resolvedItemHeight => m_CollectionView.ResolveItemHeight();

        protected override bool VisibleItemPredicate(T i)
        {
            return true;
        }

        public FixedHeightVirtualizationController(BaseVerticalCollectionView collectionView)
            : base(collectionView) {}

        public override int GetIndexFromPosition(Vector2 position)
        {
            return (int)(position.y / resolvedItemHeight);
        }

        public override float GetExpectedItemHeight(int index)
        {
            return resolvedItemHeight;
        }

        public override float GetExpectedContentHeight()
        {
            return itemsCount * resolvedItemHeight;
        }

        public override void ScrollToItem(int index)
        {
            if (visibleItemCount == 0 || index < -1)
                return;

            var pixelAlignedItemHeight = resolvedItemHeight;
            if (index == -1)
            {
                // Scroll to last item
                int actualCount = (int)(lastHeight / pixelAlignedItemHeight);
                if (itemsCount < actualCount)
                    m_ScrollView.scrollOffset = new Vector2(0, 0);
                else
                    m_ScrollView.scrollOffset = new Vector2(0, (itemsCount + 1) * pixelAlignedItemHeight);
            }
            else if (firstVisibleIndex >= index)
            {
                m_ScrollView.scrollOffset = Vector2.up * (pixelAlignedItemHeight * index);
            }
            else // index > first
            {
                var actualCount = (int)(lastHeight / pixelAlignedItemHeight);
                if (index < firstVisibleIndex + actualCount)
                    return;

                var d = index - actualCount + 1; // +1 ensures targeted element is fully visible
                var visibleOffset = pixelAlignedItemHeight - (lastHeight - actualCount * pixelAlignedItemHeight);
                var yScrollOffset = pixelAlignedItemHeight * d + visibleOffset;

                m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, yScrollOffset);
            }
        }

        public override void Resize(Vector2 size)
        {
            var pixelAlignedItemHeight = resolvedItemHeight;
            var contentHeight = GetExpectedContentHeight();
            m_ScrollView.contentContainer.style.height = contentHeight;

            // Restore scroll offset and preemptively update the highValue
            // in case this is the initial restore from persistent data and
            // the ScrollView's OnGeometryChanged() didn't update the low
            // and highValues.
            var scrollableHeight = Mathf.Max(0, contentHeight - m_ScrollView.contentViewport.layout.height);
            var scrollOffset = Mathf.Min(serializedData.scrollOffset.y, scrollableHeight);
            m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(scrollableHeight);
            m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(scrollOffset);

            // Add only extra items if the list is actually visible.
            var itemCountFromHeight = (int)(m_CollectionView.ResolveItemHeight(size.y) / pixelAlignedItemHeight);
            if (itemCountFromHeight > 0)
                itemCountFromHeight += k_ExtraVisibleItems;

            var itemCount = Mathf.Min(itemCountFromHeight, itemsCount);

            if (visibleItemCount != itemCount)
            {
                var initialVisibleCount = visibleItemCount;
                if (visibleItemCount > itemCount)
                {
                    // Shrink
                    var removeCount = initialVisibleCount - itemCount;
                    for (var i = 0; i < removeCount; i++)
                    {
                        var lastIndex = m_ActiveItems.Count - 1;
                        ReleaseItem(lastIndex);
                    }
                }
                else
                {
                    // Grow
                    var addCount = itemCount - visibleItemCount;
                    for (var i = 0; i < addCount; i++)
                    {
                        var index = i + firstVisibleIndex + initialVisibleCount;
                        var recycledItem = GetOrMakeItemAtIndex();

                        Setup(recycledItem, index);
                    }
                }
            }

            OnScroll(new Vector2(0, scrollOffset));
        }

        public override void OnScroll(Vector2 scrollOffset)
        {
            var offset = scrollOffset.y;
            var pixelAlignedItemHeight = resolvedItemHeight;
            var firstVisibleItemIndex = (int)(offset / pixelAlignedItemHeight);

            m_ScrollView.contentContainer.style.paddingTop = firstVisibleItemIndex * pixelAlignedItemHeight;
            m_ScrollView.contentContainer.style.height = itemsCount * pixelAlignedItemHeight;
            serializedData.scrollOffset.y = scrollOffset.y;

            if (firstVisibleItemIndex != firstVisibleIndex)
            {
                firstVisibleIndex = firstVisibleItemIndex;
                if (m_ActiveItems.Count > 0)
                {
                    // we try to avoid rebinding a few items
                    if (firstVisibleIndex < m_ActiveItems[0].index) //we're scrolling up
                    {
                        //How many do we have to swap back
                        int count = m_ActiveItems[0].index - firstVisibleIndex;

                        var inserting = m_ScrollInsertionList;

                        for (int i = 0; i < count && m_ActiveItems.Count > 0; ++i)
                        {
                            var last = m_ActiveItems[^1];
                            inserting.Add(last);
                            m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1); //we remove from the end

                            last.rootElement.SendToBack(); //We send the element to the top of the list (back in z-order)
                        }

                        m_ActiveItems.InsertRange(0, inserting);
                        m_ScrollInsertionList.Clear();
                    }
                    else //down
                    {
                        if (firstVisibleIndex < m_ActiveItems[^1].index)
                        {
                            var inserting = m_ScrollInsertionList;

                            int checkIndex = 0;
                            while (firstVisibleIndex > m_ActiveItems[checkIndex].index)
                            {
                                var first = m_ActiveItems[checkIndex];
                                inserting.Add(first);
                                checkIndex++;

                                first.rootElement.BringToFront(); //We send the element to the bottom of the list (front in z-order)
                            }

                            m_ActiveItems.RemoveRange(0, checkIndex); //we remove them all at once
                            m_ActiveItems.AddRange(inserting); // add them back to the end
                            inserting.Clear();
                        }
                    }

                    //Let's rebind everything
                    for (var i = 0; i < m_ActiveItems.Count; i++)
                    {
                        var index = i + firstVisibleIndex;
                        Setup(m_ActiveItems[i], index);
                    }
                }
            }
        }

        internal override T GetOrMakeItemAtIndex(int activeItemIndex = -1, int scrollViewIndex = -1)
        {
            var item = base.GetOrMakeItemAtIndex(activeItemIndex, scrollViewIndex);
            item.rootElement.style.height = resolvedItemHeight;
            return item;
        }

        internal override void EndDrag(int dropIndex)
        {
            m_DraggedItem.rootElement.style.height = resolvedItemHeight;

            if (firstVisibleIndex > m_DraggedItem.index)
            {
                m_ScrollView.verticalScroller.value = serializedData.scrollOffset.y - resolvedItemHeight;
            }

            base.EndDrag(dropIndex);
        }
    }
}
