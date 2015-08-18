using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMasterPassword.Common
{
    public interface IDetectChanges
    {
        event Action DataChanged;
    }


    public class GenericChangeDetection : IDetectChanges, IDisposable
    {
        public event Action DataChanged;

        private List<IDisposable> changeMonitors;

        public void OnDataChanged()
        {
            var fireEvent = DataChanged;
            if (null != fireEvent)
            {
                fireEvent();
            }
        }

        /// <summary>
        /// IDisposable
        /// </summary>
        public void Dispose()
        {
            if (null == changeMonitors) return;

            foreach (var item in changeMonitors)
            {
                item.Dispose();
            }
            changeMonitors = null;
        }

        /// <summary>
        /// monitor INotifyPropertyChanged instance
        /// </summary>
        public void AddINotifyPropertyChanged(INotifyPropertyChanged item)
        {
            Add(new MonitorINotifyPropertyChanged(this, item));
        }

        /// <summary>
        /// monitor AddIDetectChanges instance
        /// </summary>
        public void AddIDetectChanges(IDetectChanges item)
        {
            Add(new MonitorIDetectChanges(this, item));
        }

        /// <summary>
        /// monitor INotifyPropertyChanged instance
        /// </summary>
        public void AddCollectionOfIDetectChanges<T>(ObservableCollection<T> collection, Func<T, IDetectChanges> selectWhatToMonitor) where T : class 
        {
            Add(new MonitorCollectionOfIDetectChanges<T>(this, collection, selectWhatToMonitor));
        }

        private void Add(IDisposable item)
        {
            if (null == changeMonitors)
            {
                changeMonitors = new List<IDisposable>();
            }
            changeMonitors.Add(item);
        }

        private class MonitorINotifyPropertyChanged : IDisposable
        {
            private GenericChangeDetection parent;
            private INotifyPropertyChanged item;

            public MonitorINotifyPropertyChanged(GenericChangeDetection parent, INotifyPropertyChanged item)
            {
                this.item = item;
                this.parent = parent;

                item.PropertyChanged += Item_PropertyChanged;
            }

            private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                parent.OnDataChanged();
            }

            public void Dispose()
            {
                item.PropertyChanged -= Item_PropertyChanged;
            }
        }

        private class MonitorIDetectChanges : IDisposable
        {
            private GenericChangeDetection parent;
            private IDetectChanges item;

            public MonitorIDetectChanges(GenericChangeDetection parent, IDetectChanges item)
            {
                this.parent = parent;
                this.item = item;

                item.DataChanged += this.parent.OnDataChanged;
            }

            public void Dispose()
            {
                item.DataChanged -= parent.OnDataChanged;
            }
        }

        private class MonitorCollectionOfIDetectChanges<T> : IDisposable where T : class
        {
            private ObservableCollection<T> collection;
            private GenericChangeDetection parent;
            private Func<T, IDetectChanges> selectWhatToMonitor;

            private List<Tuple<T, IDetectChanges>> monitoredItems = new List<Tuple<T, IDetectChanges>>();

            public MonitorCollectionOfIDetectChanges(GenericChangeDetection parent, ObservableCollection<T> collection, Func<T, IDetectChanges> selectWhatToMonitor)
            {
                this.parent = parent;
                this.collection = collection;
                this.selectWhatToMonitor = selectWhatToMonitor;

                collection.CollectionChanged += Collection_CollectionChanged;
                Sync();
            }

            private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Sync();
            }

            private void Sync()
            {
                var removed = SynchronizeLists.Sync(monitoredItems, collection, (item, tuple) => tuple.Item2 == item, AddItem);
            }

            private Tuple<T, IDetectChanges> AddItem(T item)
            {
                IDetectChanges checkThis = selectWhatToMonitor(item);
                checkThis.DataChanged += parent.OnDataChanged;
                return Tuple.Create(item, checkThis);
            }

            public void Dispose()
            {
                if (null == monitoredItems) return; // already disposed

                collection.CollectionChanged -= Collection_CollectionChanged;

                foreach (var item in monitoredItems)
                {
                    item.Item2.DataChanged -= parent.OnDataChanged;
                }

                monitoredItems = null;
            }
        }
    }
}
