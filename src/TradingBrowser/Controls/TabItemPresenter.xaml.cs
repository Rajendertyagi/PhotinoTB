using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;

namespace TradingBrowser.Controls;

/// <summary>
/// Custom tab item control with trapezoidal shape, middle-click close, and visual state management.
/// </summary>
public sealed partial class TabItemPresenter : UserControl
{
    // Dependency properties for data binding
    public static readonly DependencyProperty TitleProperty = 
        DependencyProperty.Register("Title", typeof(string), typeof(TabItemPresenter), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty IsSelectedProperty = 
        DependencyProperty.Register("IsSelected", typeof(bool), typeof(TabItemPresenter), new PropertyMetadata(false, OnIsSelectedChanged));
    public static readonly DependencyProperty IsPinnedProperty = 
        DependencyProperty.Register("IsPinned", typeof(bool), typeof(TabItemPresenter), new PropertyMetadata(false));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsPinned
    {
        get => (bool)GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }

    // Internal brushes bound to XAML for state-driven theming
    private SolidColorBrush BackgroundBrush => IsSelected ? new SolidColorBrush(Microsoft.UI.Color.FromArgb(255, 32, 33, 36)) : new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    private SolidColorBrush ForegroundBrush => IsSelected ? new SolidColorBrush(Microsoft.UI.Colors.White) : new SolidColorBrush(Microsoft.UI.Color.FromArgb(255, 154, 160, 166));

    /// <summary>
    /// Event fired when the middle mouse button is pressed on this tab.
    /// </summary>
    public event Action? MiddleClicked;

    /// <summary>
    /// Event fired when the close button is clicked.
    /// </summary>
    public event Action? CloseClicked;

    public TabItemPresenter()
    {
        this.InitializeComponent();
        // Default close button visibility based on selection
        CloseButton.Visibility = IsSelected ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabItemPresenter control)
        {
            // Update visual state and close button visibility when selection changes
            VisualStateManager.GoToState(control, (bool)e.NewValue ? "Selected" : "Normal", true);
            control.CloseButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Detect middle mouse button click for quick tab closing
        var props = e.GetCurrentPoint(this).Properties;
        if (props.IsMiddleButtonPressed)
        {
            e.Handled = true;
            MiddleClicked?.Invoke();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        CloseClicked?.Invoke();
    }
}
