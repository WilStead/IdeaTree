using System.Windows;
using System.Windows.Controls;

namespace IdeaTree2
{
    public class CollapsibleColumnDefinition : ColumnDefinition
    {
        public static DependencyProperty VisibleProperty;
        
        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        
        static CollapsibleColumnDefinition()
        {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(bool),
                typeof(CollapsibleColumnDefinition),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));

            WidthProperty.OverrideMetadata(typeof(CollapsibleColumnDefinition),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceWidth)));

            MinWidthProperty.OverrideMetadata(typeof(CollapsibleColumnDefinition),
                new FrameworkPropertyMetadata((double)0, null,
                    new CoerceValueCallback(CoerceMinWidth)));
        }
        
        public static void SetVisible(DependencyObject obj, bool nVisible) => obj.SetValue(VisibleProperty, nVisible);

        public static bool GetVisible(DependencyObject obj) => (bool)obj.GetValue(VisibleProperty);

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            obj.CoerceValue(WidthProperty);
            obj.CoerceValue(MinWidthProperty);
        }

        static object CoerceWidth(DependencyObject obj, object nValue) => (((CollapsibleColumnDefinition)obj).Visible) ? nValue : new GridLength(0);

        static object CoerceMinWidth(DependencyObject obj, object nValue) => (((CollapsibleColumnDefinition)obj).Visible) ? nValue : (double)0;
    }
}
