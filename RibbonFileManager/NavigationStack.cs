using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RibbonFileManager
{
    public sealed class RecentLocationsList<T> : ObservableCollection<T>
    {
        Int32 _index = -1;

        protected override void InsertItem(Int32 index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.InsertItem(index, item);
        }

        protected override void SetItem(Int32 index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.SetItem(index, item);
        }

        public void Navigate(T item)
        {
            _index++;
            if (Count == _index)
            {
                Add(item);
                return;
            }

            this[_index] = item;
        }

        public void Back()
        {
            _index--;
        }

        public void Forward()
        {
            _index++;
        }

        public T Current
        {
            get => this[_index];
            set => this[_index] = value;
        }
    }

    public sealed class NavigationStack<T> : ObservableCollection<T>
    {
        Int32 _index = 0;

        protected override void InsertItem(Int32 index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.InsertItem(index, item);
        }

        protected override void SetItem(Int32 index, T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            base.SetItem(index, item);
        }

        public T Back()
        {
            if (!CanGoBack)
            {
                throw new InvalidOperationException("There are no more elements to navigate to.");
            }

            _index--;
            return Current;

        }

        public T Forward()
        {
            if (!CanGoForward)
            {
                throw new InvalidOperationException("There are no more elements to navigate to.");
            }

            _index++;
            return Current;
        }

        public Boolean CanGoForward => _index + 1 <= Count - 1;

        public Boolean CanGoBack => _index > 0; //_index - 1 >= 0;

        public T Current
        {
            get => this[_index];
            set => this[_index] = value;
        }
    }
}
