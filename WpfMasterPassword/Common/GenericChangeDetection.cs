using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfMasterPassword.Common
{
    /// <summary>
    /// this interface detects changes (simpler version of INotifyPropertyChanged)
    /// </summary>
    public interface IDetectChanges
    {
        event Action DataChanged;
    }

    /// <summary>
    /// Monitor multiple sources (INotifyPropertyChanged, IDetectChanges, ObservableCollection of ...)
    /// Dispose to disconnect from monitored stuff (unsubscribe events).
    /// </summary>
    public class GenericChangeDetection : IDetectChanges, IDisposable
    {
        private List<IDisposable> changeMonitors;

        public event Action DataChanged;

        /// <summary>
        /// Trigger DataChanged event
        /// </summary>
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
        /// monitor collection of INotifyPropertyChanged instances
        /// </summary>
        public void AddCollectionOfIDetectChanges<T>(ObservableCollection<T> collection, Func<T, IDetectChanges> selectWhatToMonitor) where T : class
        {
            Add(new MonitorCollectionOfIDetectChanges<T>(this, collection, selectWhatToMonitor));
        }

        /// <summary>
        /// monitor collection of INotifyPropertyChanged instances
        /// </summary>
        public void AddCollectionOfINotifyPropertyChanged<T>(ObservableCollection<T> collection, Func<T, INotifyPropertyChanged> selectWhatToMonitor) where T : class
        {
            Add(new MonitorCollectionOfINotifyPropertyChanged<T>(this, collection, selectWhatToMonitor));
        }

        #region Implementation
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
                Sync(); // hook into changes

                parent.OnDataChanged(); // list changed, tell him
            }

            private void Sync()
            {
                var removed = SynchronizeLists.Sync(monitoredItems, collection, (item, tuple) => tuple.Item1 == item, AddItem);
                foreach (var item in removed)
                {
                    item.Item2.DataChanged -= parent.OnDataChanged;
                }
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

        private class MonitorCollectionOfINotifyPropertyChanged<T> : IDisposable where T : class
        {
            private GenericChangeDetection parent;
            private ObservableCollection<T> collection;
            private Func<T, INotifyPropertyChanged> selectWhatToMonitor;

            private List<Tuple<T, INotifyPropertyChanged>> monitoredItems = new List<Tuple<T, INotifyPropertyChanged>>();

            public MonitorCollectionOfINotifyPropertyChanged(GenericChangeDetection parent, ObservableCollection<T> collection, Func<T, INotifyPropertyChanged> selectWhatToMonitor)
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
                var removed = SynchronizeLists.Sync(monitoredItems, collection, (item, tuple) => tuple.Item1 == item, AddItem);

                foreach (var item in removed)
                {
                    item.Item2.PropertyChanged -= CheckThis_PropertyChanged;
                }
            }

            private Tuple<T, INotifyPropertyChanged> AddItem(T item)
            {
                INotifyPropertyChanged checkThis = selectWhatToMonitor(item);

                checkThis.PropertyChanged += CheckThis_PropertyChanged;

                return Tuple.Create(item, checkThis);
            }

            private void CheckThis_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                parent.OnDataChanged();
            }

            public void Dispose()
            {
                if (null == monitoredItems) return; // already disposed

                collection.CollectionChanged -= Collection_CollectionChanged;

                foreach (var item in monitoredItems)
                {
                    item.Item2.PropertyChanged -= CheckThis_PropertyChanged;
                }

                monitoredItems = null;
            }
        }
        #endregion
    }
}
