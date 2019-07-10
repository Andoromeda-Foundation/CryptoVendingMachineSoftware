using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace VendingMachineKiosk.Controls
{
    public class ItemEnabledGridView : GridView
    {
        public string ItemEnabledPath { get; set; }

        public IValueConverter ItemEnabledConverter { get; set; }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is GridViewItem gridViewItem && !string.IsNullOrWhiteSpace(ItemEnabledPath))
            {
                var binding = new Binding
                {
                    Source = item,
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath(ItemEnabledPath),
                    Converter = ItemEnabledConverter
                };
                gridViewItem.SetBinding(IsEnabledProperty, binding);
            }
        }
    }
}
