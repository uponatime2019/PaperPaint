using System;
using Microsoft.UI.Xaml.Controls;

namespace PaperPaint.Helpers
{
    /// <summary>Tiny navigation bus: pages call Nav.Go&lt;T&gt;(payload) instead of holding Frame refs.</summary>
    public static class Nav
    {
        /// <summary>Raised after a navigation completes so the shell can sync selection.</summary>
        public static event Action<Type>? PageChanged;

        /// <summary>Payload consumed by the destination page in OnNavigatedTo.</summary>
        public static object? PendingArg { get; set; }

        public static bool Go<T>(object? arg = null) where T : Page
        {
            return Go(typeof(T), arg);
        }

        public static bool Go(Type pageType, object? arg = null)
        {
            var frame = App.ContentFrame;
            if (frame == null)
            {
                return false;
            }

            PendingArg = arg;
            return frame.Navigate(pageType);
        }

        public static void RaisePageChanged(Type pageType)
        {
            PageChanged?.Invoke(pageType);
        }
    }
}
