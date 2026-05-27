using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;

namespace TradingBrowser.Controls;

public sealed partial class TabItemPresenter : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(TabItemPresenter), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(TabItemPresenter), new PropertyMetadata(false, OnIsActiveChanged));

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }

    public event EventHandler<PointerRoutedEventArgs>? MiddleClicked;
    public event EventHandler<RoutedEventArgs>? CloseClicked;
    
    // ✅ FIX: ContextRequestedEventArgs forces the OS to yield the right-click to XAML
    public event EventHandler<ContextRequestedEventArgs>? TabContextRequested; 

    public TabItemPresenter() => this.InitializeComponent();

    // ✅ FIX: Bulletproof active state styling via direct property assignment
    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabItemPresenter p && p.IsLoaded)
        {
            p.ApplyActiveState();
        }
    }

    private void TabItemPresenter_Loaded(object sender, RoutedEventArgs e)
    {
        ApplyActiveState();
    }

    private void ApplyActiveState()
    {
        if (IsActive)
        {
            TabBackground.Fill = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"];
            TabBackground.Stroke = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
            BottomCover.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Visible;
        }
        else
        {
            TabBackground.Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            TabBackground.Stroke = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            BottomCover.Visibility = Visibility.Collapsed;
            CloseButton.Visibility = Visibility.Collapsed;
        }
    }

    // ✅ FIX: Explicit hover logic (VisualStateManager fails inside ListView templates)
    private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e) 
    { 
        CloseButton.Visibility = Visibility.Visible; 
    }

    private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e) 
    { 
        if (!IsActive) CloseButton.Visibility = Visibility.Collapsed; 
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => CloseClicked?.Invoke(this, e);

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            MiddleClicked?.Invoke(this, e);
            e.Handled = true;
        }
    }

    // ✅ FIX: ContextRequested blocks the default Windows "Move/Size" system menu
    private void RootGrid_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
    {
        TabContextRequested?.Invoke(this, args);
        args.Handled = true; 
    }
}
