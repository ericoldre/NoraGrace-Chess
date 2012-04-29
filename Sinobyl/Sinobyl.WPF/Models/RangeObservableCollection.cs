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
        private bool _suppressNotification = false;

        public RangeObservableCollection()
            : base() { }

        public RangeObservableCollection(IEnumerable<T> collection)
            : base(collection) { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            //Hide the notification when we are doing a full range
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        public void AddRange(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            //Turn off collection change notificaiton
            _suppressNotification = true;

            foreach (T item in list)
            {
                Add(item);
            }

            //Turn notification back on
            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)list));
        }

        public void RemoveRange(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            //Turn off collection change notificaiton
            _suppressNotification = true;

            foreach (T item in list)
            {
                //if (this.Contains(item))
                Remove(item);
            }

            //Turn notification back on
            _suppressNotification = false;

            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)list);
            OnCollectionChanged(args);
        }
    }
}