using System.Collections.ObjectModel;

namespace FolderCompareTool
{
    public class NonRepeatObservableCollection<T> : ObservableCollection<T>
    {
        public NonRepeatObservableCollection() : base() { }

        public NonRepeatObservableCollection(IEnumerable<T> collection) : base(collection) { }

        protected override void InsertItem(int index, T item)
        {
            if (Items.Contains(item)) return; // 如果元素已存在

            base.InsertItem(index, item); // 插入新元素
        }
    }
}
