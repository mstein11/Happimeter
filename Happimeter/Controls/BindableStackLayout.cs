using System.Collections;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Happimeter.Controls
{
    /// <summary>
    /// https://smellyc0de.wordpress.com/2018/06/06/repeater-or-bindable-stacklayout/
    /// </summary>
    public class BindableStackLayout : StackLayout
    {
        public BindableStackLayout()
        {
        }

        public ICollection ItemsSource
        {
            get { return (ICollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource),
                                    typeof(ICollection),
                                    typeof(BindableStackLayout),
                                    null,
                                    BindingMode.OneWay,
                                    propertyChanged: (bindable, oldValue, newValue) => ((BindableStackLayout)bindable).PopulateItems());

        public DataTemplate ItemDataTemplate
        {
            get { return (DataTemplate)GetValue(ItemDataTemplateProperty); }
            set { SetValue(ItemDataTemplateProperty, value); }
        }
        public static readonly BindableProperty ItemDataTemplateProperty =
            BindableProperty.Create(nameof(ItemDataTemplate), typeof(DataTemplate), typeof(BindableStackLayout));

        void PopulateItems()
        {
            if (ItemsSource == null) return;
            if (ItemsSource is INotifyCollectionChanged)
            {
                (ItemsSource as INotifyCollectionChanged).CollectionChanged += Handle_CollectionChanged;
            }
            foreach (var item in ItemsSource)
            {
                var itemTemplate = ItemDataTemplate.CreateContent() as View;
                itemTemplate.BindingContext = item;
                Children.Add(itemTemplate);
            }
        }

        void Handle_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Children.Clear();
            }
            else
            {
                if (e.NewItems.Count > 0)
                {
                    var item = e.NewItems[0];
                    var itemTemplate = ItemDataTemplate.CreateContent() as View;
                    itemTemplate.BindingContext = item;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Children.Add(itemTemplate);
                    });

                }
            }
        }
    }
}