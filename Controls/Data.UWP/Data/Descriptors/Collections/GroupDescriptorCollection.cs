﻿using System;
using System.Collections.Generic;

namespace Telerik.Data.Core
{
    /// <summary>
    /// Represents a strongly-typed collection of <see cref="GroupDescriptorBase"/> instances.
    /// </summary>
    public sealed class GroupDescriptorCollection : DataDescriptorCollection<GroupDescriptorBase>
    {
        private const string RemoveDescriptorExceptionMessage = "The default group descriptor built based on the ICollectionView groups cannot be removed.";
        private const string DescriptorAlwaysOnFirstLevelExceptionMessage = "The default group decriptor built based on the ICollectionView groups is always on the first level";
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDescriptorCollection" /> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        internal GroupDescriptorCollection(IDataDescriptorsHost owner)
            : base(owner)
        {
        }

        protected override void SetItem(int index, GroupDescriptorBase item)
        {
            var oldItem = this[index];
            if (oldItem.GetType().Equals(typeof(CollectionViewGroupDescriptor)))
            {
                throw new NotSupportedException(RemoveDescriptorExceptionMessage);
            }

            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, GroupDescriptorBase item)
        {
            if (index == 0 && this.Count > 0 && this[0] is CollectionViewGroupDescriptor)
            {
                throw new NotSupportedException(DescriptorAlwaysOnFirstLevelExceptionMessage);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];

            if (!allowRemoveCollectionViewDescriptor && item.GetType().Equals(typeof(CollectionViewGroupDescriptor)))
            {
                throw new NotSupportedException(RemoveDescriptorExceptionMessage);
            }
            else
            {
                base.RemoveItem(index);
            }
        }

        private bool allowRemoveCollectionViewDescriptor = false;
        internal bool TryRemoveCollectionViewGroup()
        {
            if(this.Count > 0 && this[0] is CollectionViewGroupDescriptor)
            {
                this.allowRemoveCollectionViewDescriptor = true;
                base.RemoveAt(0);
                this.allowRemoveCollectionViewDescriptor = false;

                return true;
            }

            return false;
        }

        protected override void ClearItems()
        {
            if (this.Count == 0)
            {
                return;
            }

            foreach (var descriptor in this)
            {
                if (descriptor.GetType().Equals(typeof(CollectionViewGroupDescriptor)))
                {
                    throw new NotSupportedException(RemoveDescriptorExceptionMessage);
                }
            }

            base.ClearItems();
        }
    }
}
