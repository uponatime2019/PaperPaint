using System;
using PaperPaint.Helpers;
using PaperPaint.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace PaperPaint
{
    /// <summary>App shell: navigation view hosting Home, Coloring Pages, Artwork, Settings and the Paint Studio.</summary>
    public sealed partial class MainWindow : Window
    {
        private bool _syncingSelection;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Paper Paint";

            // Backdrop + icon are cosmetic: never let interop kill window creation.
            try
            {
                SystemBackdrop = new MicaBackdrop();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.MicaBackdrop");
            }

            try
            {
                WindowHelper.SetAppIcon(this);
                WindowHelper.Maximize(this);
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.SetAppIcon");
            }

            try
            {
                App.ContentFrame = ContentFrame;
                ContentFrame.Navigated += ContentFrame_Navigated;
                Nav.PageChanged += OnPageChanged;

                ContentFrame.Navigate(typeof(HomePage));
                // Selection is set here (not in XAML) so SelectionChanged never fires
                // while the frame fields are still connecting.
                NavView.SelectedItem = NavItemHome;
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.Initialize");
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                Nav.RaisePageChanged(e.SourcePageType);
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.Navigated");
            }
        }

        private void OnPageChanged(Type pageType)
        {
            _syncingSelection = true;
            try
            {
                if (pageType == typeof(StudioPage))
                {
                    // The studio keeps only a slim hamburger rail; other pages use the full pane.
                    NavView.SelectedItem = null;
                    NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                    NavView.IsPaneOpen = false;
                }
                else
                {
                    NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                    NavView.IsPaneOpen = true;
                    NavView.SelectedItem =
                        pageType == typeof(ColoringGalleryPage) ? NavItemColoring :
                        pageType == typeof(ArtworkPage) ? NavItemArtwork :
                        pageType == typeof(SettingsPage) ? NavItemSettings :
                        NavItemHome;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.OnPageChanged");
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (ContentFrame == null || _syncingSelection || args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
            {
                return;
            }

            try
            {
                switch (tag)
                {
                    case "home":
                        if (ContentFrame.CurrentSourcePageType != typeof(HomePage))
                        {
                            ContentFrame.Navigate(typeof(HomePage));
                        }

                        break;
                    case "coloring":
                        if (ContentFrame.CurrentSourcePageType != typeof(ColoringGalleryPage))
                        {
                            ContentFrame.Navigate(typeof(ColoringGalleryPage));
                        }

                        break;
                    case "artwork":
                        if (ContentFrame.CurrentSourcePageType != typeof(ArtworkPage))
                        {
                            ContentFrame.Navigate(typeof(ArtworkPage));
                        }

                        break;
                    case "settings":
                        if (ContentFrame.CurrentSourcePageType != typeof(SettingsPage))
                        {
                            ContentFrame.Navigate(typeof(SettingsPage));
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "MainWindow.SelectionChanged");
            }
        }
    }
}
