﻿namespace Caliburn.Micro {
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interactivity;

    /// <summary>
    /// A collection that can exist as part of a behavior.
    /// </summary>
    /// <typeparam name="T">The type of item in the attached collection.</typeparam>
    public class AttachedCollection<T> : FreezableCollection<T>, IAttachedObject
        where T : DependencyObject, IAttachedObject {
        DependencyObject associatedObject;

        /// <summary>
        /// Creates an instance of <see cref="AttachedCollection{T}"/>
        /// </summary>
        public AttachedCollection() {
            ((INotifyCollectionChanged)this).CollectionChanged += OnCollectionChanged;
        }

        /// <summary>
        /// Attached the collection.
        /// </summary>
        /// <param name="dependencyObject">The dependency object to attach the collection to.</param>
        public void Attach(DependencyObject dependencyObject) {
            WritePreamble();
            associatedObject = dependencyObject;
            WritePostscript();

            this.Apply(x => x.Attach(associatedObject));
        }

        /// <summary>
        /// Detaches the collection.
        /// </summary>
        public void Detach() {
            this.Apply(x => x.Detach());
            WritePreamble();
            associatedObject = null;
            WritePostscript();
        }

        DependencyObject IAttachedObject.AssociatedObject {
            get { return associatedObject; }
        }

        /// <summary>
        /// Called when an item is added from the collection.
        /// </summary>
        /// <param name="item">The item that was added.</param>
        protected void OnItemAdded(T item) {
            if (associatedObject != null)
                item.Attach(associatedObject);
        }

        /// <summary>
        /// Called when an item is removed from the collection.
        /// </summary>
        /// <param name="item">The item that was removed.</param>
        protected void OnItemRemoved(T item) {
            if(item.AssociatedObject != null)
                item.Detach();
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch(e.Action) {
                case NotifyCollectionChangedAction.Add:
                    e.NewItems.OfType<T>().Where(x => !Contains(x)).Apply(OnItemAdded);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    e.OldItems.OfType<T>().Apply(OnItemRemoved);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    e.OldItems.OfType<T>().Apply(OnItemRemoved);
                    e.NewItems.OfType<T>().Where(x => !Contains(x)).Apply(OnItemAdded);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Apply(OnItemRemoved);
                    this.Apply(OnItemAdded);
                    break;
            }
        }
    }
}