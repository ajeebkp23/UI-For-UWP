﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Data.Core;

namespace Telerik.UI.Xaml.Controls.Grid
{
    /// <summary>
    /// Base class used to sort items based on the item's <see cref="DescriptionBase.PropertyName"/> value.
    /// </summary>
    internal abstract class SortDescription : DescriptionBase
    {
        private SortOrder sortOrder;

        /// <summary>
        /// Gets or sets the custom name that will be used as display name.
        /// </summary>
        public SortOrder SortOrder
        {
            get
            {
                return this.sortOrder;
            }

            set
            {
                if (this.sortOrder != value)
                {
                    this.sortOrder = value;
                    this.OnPropertyChanged("SortOrder");
                    this.NotifyChange(new SettingsChangedEventArgs());
                }
            }
        }

        /// <summary>
        /// Returns the value that will be passed in the aggregate for given item.
        /// </summary>
        /// <param name="item">The item which value will be extracted.</param>
        /// <returns>Returns the value for given item.</returns>
        protected internal virtual object GetValueForItem(object item)
        {
            if (this.MemberAccess == null)
            {
                throw new System.InvalidOperationException("Member access has not been initialized");
            }

            return this.MemberAccess.GetValue(item);
        }

        /// <inheritdoc />
        protected sealed override void CloneCore(Cloneable source)
        {
            base.CloneCore(source);
            SortDescription original = source as SortDescription;
            if (original != null)
            {
                this.SortOrder = original.SortOrder;
            }
            this.CloneOverride(source);
        }

        /// <summary>
        /// Makes the instance a clone (deep copy) of the specified <see cref="Telerik.Data.Core.Cloneable"/>.
        /// </summary>
        /// <param name="source">The object to clone.</param>
        /// <remarks>Notes to Inheritors
        /// If you derive from <see cref="Telerik.Data.Core.Cloneable"/>, you need to override this method to copy all properties.
        /// It is essential that all implementations call the base implementation of this method (if you don't call base you should manually copy all needed properties including base properties).
        /// </remarks>
        protected abstract void CloneOverride(Cloneable source);
    }
}