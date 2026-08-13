using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Mercury.Editor.Utils;

/// <summary>
/// A variant of the <see cref="ObservableCollection{T}"/> but that
/// sends notifications when any of its items is updated. 
/// </summary>
public sealed class ObservableCollectionEx<T> : ObservableCollection<T> 
    where T: INotifyPropertyChanged {
    
    public event PropertyChangedEventHandler? ItemPropertyChanged;
    
    public ObservableCollectionEx()
    {
        CollectionChanged += OnCollectionChangedInternal;
    }

    private void OnCollectionChangedInternal(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (T item in e.NewItems)
            {
                item?.PropertyChanged += Item_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (T item in e.OldItems)
            {
                item?.PropertyChanged -= Item_PropertyChanged;
            }
        }
    }
    
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(sender, e);
        // OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }

    protected override void ClearItems()
    {
        foreach (T item in this)
        {
            item?.PropertyChanged -= Item_PropertyChanged;
        }

        base.ClearItems();
    }
}