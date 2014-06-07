using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

namespace Sinobyl.WPF.Models
{
    [Serializable]
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressBulkNotification = false;

        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChangedBulk;

        public RangeObservableCollection()
            : base() { }

        public RangeObservableCollection(IEnumerable<T> collection)
            : base(collection) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            //Hide the bulk notification when we are doing a full range
            if (!_suppressBulkNotification)
            {
                OnBulkCollectionChanged(e);
            }
        }

        protected void OnBulkCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove)
            {
                throw new ArgumentException("");
            }
            
            var bulkEv = this.CollectionChangedBulk;
            if (bulkEv != null)
            {
                bulkEv(this, e);
            }
            
        }
        protected override void ClearItems()
        {
            var clearedList = this.ToList();
            _suppressBulkNotification = true;
            base.ClearItems();
            _suppressBulkNotification = false;
            OnBulkCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, clearedList));
        }

        public void AddRange(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            //Turn off collection change notificaiton
            _suppressBulkNotification = true;

            foreach (T item in list)
            {
                Add(item);
            }

            //Turn notification back on
            _suppressBulkNotification = false;

            OnBulkCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list));
        }

        public void RemoveRange(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            //Turn off collection change notificaiton
            _suppressBulkNotification = true;

            foreach (T item in list)
            {
                //if (this.Contains(item))
                Remove(item);
            }

            //Turn notification back on
            _suppressBulkNotification = false;

            OnBulkCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list));
        }
    }
}