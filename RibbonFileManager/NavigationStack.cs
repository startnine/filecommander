using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RibbonFileManager
{
    /*public sealed class RecentLocationsList<T> : ObservableCollection<T>
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

        public Int32 Index
        {
            get => _index;
        }

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

        public void ReplaceFuture(int index, T item)
        {
            Insert(index, item);
            for (int i = index; i < Count; i++)
                RemoveAt(index);
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
    }*/

    public class NavigationManager<T> : ObservableCollection<T>
    {
        public int CurrentIndex { get; private set; } = 0;

        void ReplaceFuture(int index, T item)
        {
            if (Count > 0)
            {
                for (int i = index; i < Count; i++)
                    RemoveAt(index);
            }

            Add(item); //Insert(index, item);
        }

        void ReplaceFuture(T item)
        {
            ReplaceFuture(CurrentIndex + 1, item);
        }

        public bool CanGoBack => CurrentIndex > 0;

        public bool GoBack()
        {
            if (CanGoBack)
            {
                CurrentIndex--;
                Navigated?.Invoke(this, new NavigationEventArgs(true));
                return true;
            }
            else
                return false;
        }

        public bool CanGoForward => CurrentIndex < (Count - 1);

        public bool GoFoward()
        {
            if (CanGoForward)
            {
                CurrentIndex++;
                InvokeNavigated();
                return true;
            }
            else
                return false;
        }

        public void MoveTo(T targetItem)
        {
            ReplaceFuture(targetItem);
            HistoryJumpTo(targetItem);
        }

        public bool HistoryJumpTo(int targetIndex)
        {
            if (IsIndexValid(targetIndex))
            {
                CurrentIndex = targetIndex;
                InvokeNavigated();
                return true;
            }
            else
                return false;
        }

        public bool HistoryJumpTo(T targetItem)
        {
            if (Contains(targetItem))
                return HistoryJumpTo(IndexOf(targetItem));
            else
                throw new Exception("Item \"" + targetItem.ToString() + "\" not present to jump to!");
        }

        public void InvokeNavigated()
        {
            Navigated?.Invoke(this, new NavigationEventArgs());
        }

        public bool IsIndexValid(int index)
        {
            return (index >= 0) && (index < Count);
        }

        public T Current// => this[CurrentIndex];
        {
            get
            {
                if (IsIndexValid(CurrentIndex))
                    return this[CurrentIndex];
                else
                    return default(T);
            }
        }

        public event EventHandler<NavigationEventArgs> Navigated;
    }

    public class NavigationEventArgs : EventArgs
    {
        public bool IsBackForwardNavigation { get; private set; } = false;

        public NavigationEventArgs() { }

        public NavigationEventArgs(bool isBackForwardNavigation)
        {
            IsBackForwardNavigation = isBackForwardNavigation;
        }
    }
}
