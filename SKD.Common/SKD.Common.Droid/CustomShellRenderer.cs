using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content.Resources;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Tabs;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace SKD.Common.Droid
{
    public class CustomShellRenderer : ShellRenderer
    {
        public static Typeface BottomTabTitleTypeface { get; private set; }

        public CustomShellRenderer(Context context) : base(context)
        {
            BottomTabTitleTypeface = ResourcesCompat.GetFont(context, Resource.Font.LemonMilkRegular);
        }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new CustomShellBottomNavViewAppearanceTracker(this, shellItem);
        }

        protected override IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection)
        {
            return new CustomShellTabLayoutAppearanceTracker(this);
        }

        protected override IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker()
        {
            return new CustomShellToolbarAppearanceTracker(this);
        }

    }
    public class CustomShellToolbarAppearanceTracker : ShellToolbarAppearanceTracker
    {
        public CustomShellToolbarAppearanceTracker(IShellContext context) : base(context)
        {
        }

        public override void SetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
        {
            base.SetAppearance(toolbar, toolbarTracker, appearance);
            toolbar.SetPadding(0, -16, 0, 0);
            //toolbar.LayoutParameters = new AppBarLayout.LayoutParams((toolbar.Parent as AppBarLayout)
            //    .LayoutParameters) { ScrollFlags = 0 };
        }

    }

    public class CustomShellTabLayoutAppearanceTracker : ShellTabLayoutAppearanceTracker
    {
        public CustomShellTabLayoutAppearanceTracker(IShellContext context) : base(context)
        {
        }

        public override void SetAppearance(TabLayout tabLayout, ShellAppearance appearance)
        {
            base.SetAppearance(tabLayout, appearance);
            //tabLayout.TabMode = 1;

            //if (Build.VERSION.SdkInt < BuildVersionCodes.P)
            //    return;

            //TypefaceSpan typefaceSpan = new TypefaceSpan(CustomShellRenderer.BottomTabTitleTypeface);
            //for (int i = 0; i < tabLayout.TabCount; i++)
            //{
            //    var menuItem = tabLayout.GetTabAt(i);
            //    if (menuItem.TextFormatted == null)
            //        return;
            //    SpannableStringBuilder spannableText = new SpannableStringBuilder(menuItem.TextFormatted);
            //    spannableText.SetSpan(typefaceSpan, 0, spannableText.Length(), 0);
            //    menuItem.SetText(spannableText);
            //}

        }

    }

    public class CustomShellBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        public CustomShellBottomNavViewAppearanceTracker(IShellContext context, ShellItem item) : base(context, item)
        {
        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);

            if (Build.VERSION.SdkInt < BuildVersionCodes.P)
                return;

            TypefaceSpan typefaceSpan = new TypefaceSpan(CustomShellRenderer.BottomTabTitleTypeface);
            for (int i = 0; i < bottomView.Menu.Size(); i++)
            {
                var menuItem = bottomView.Menu.GetItem(i);
                if (menuItem.TitleFormatted == null)
                    return;
                SpannableStringBuilder spannableTitle = new SpannableStringBuilder(menuItem.TitleFormatted);
                spannableTitle.SetSpan(typefaceSpan, 0, spannableTitle.Length(), 0);
                menuItem.SetTitle(spannableTitle);
            }
        }

    }

}