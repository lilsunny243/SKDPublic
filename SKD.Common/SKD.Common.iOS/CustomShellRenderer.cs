using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace SKD.Common.iOS
{
    public class CustomShellRenderer : ShellRenderer
    {
        protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
        {
            return base.CreateShellSectionRenderer(shellSection);
        }

        protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
        {
            return new CustomShellTabBarAppearanceTracker();
        }

        //protected override IShellPageRendererTracker CreatePageRendererTracker()
        //{
        //    return new CustomShellPageRendererTracker();
        //}
    }

    //public class CustomShellPageRendererTracker : ShellPageRendererTracker
    //{
    //    public CustomShellPageRendererTracker(IShellContext context) : base(context) { }

    //    protected override void UpdateTitleView()
    //    {
    //        base.UpdateTitleView();
    //        this.ViewController.NavigationController.NavigationBar.
    //    }
    //}

    public class CustomShellSectionRenderer : ShellSectionRenderer
    {

        public CustomShellSectionRenderer(IShellContext context) : base(context) { }

        protected override IShellSectionRootRenderer CreateShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
        {
            return new CustomShellSectionRootRenderer(shellSection, shellContext);
        }
    }

    public class CustomShellSectionRootRenderer : ShellSectionRootRenderer
    {
        public CustomShellSectionRootRenderer(ShellSection section, IShellContext context) : base(section, context) { }

        protected override IShellSectionRootHeader CreateShellSectionRootHeader(IShellContext shellContext)
        {
            return new CustomShellSectionRootHeader(shellContext);
        }
    }

    public class CustomShellSectionRootHeader : ShellSectionRootHeader
    {
        public CustomShellSectionRootHeader(IShellContext context) : base(context) { }

        //private readonly Dictionary<NSIndexPath, nfloat> cellWidths = new Dictionary<NSIndexPath, nfloat>();

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var cell = base.GetCell(collectionView, indexPath) as ShellSectionHeaderCell;
            //cell.Label.Font = UIFont.FromName("LemonMilk-Regular", 12);
            //if (cellWidths.ContainsKey(indexPath))
            //    cellWidths[indexPath] = cell.Frame.Width;
            //else
            //    cellWidths.Add(indexPath, cell.Frame.Width);

            //var width = collectionView.Frame.Width;
            //var layout = CollectionView.CollectionViewLayout as UICollectionViewFlowLayout;
            //var totalCellWidth = (nfloat)cellWidths.Sum(x => x.Value);
            //collectionView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            //var xPadding = (width - totalCellWidth) / 2;

            //layout.SectionInset = new UIEdgeInsets(0, xPadding, 0, xPadding);

            return cell;
        }
    }

    public class CustomShellTabBarAppearanceTracker : ShellTabBarAppearanceTracker
    {
        
        public override void SetAppearance(UITabBarController controller, ShellAppearance appearance)
        {
            base.SetAppearance(controller, appearance);
            UITabBar tabBar = controller.TabBar;
            tabBar.Translucent |= tabBar.Items is null;
            if (!(tabBar.Items is null))
            {
                foreach (UITabBarItem item in tabBar.Items)
                {
                    var normalAttr = item.GetTitleTextAttributes(UIControlState.Normal);
                    var selectedAttr = item.GetTitleTextAttributes(UIControlState.Selected);
                    normalAttr.Font = selectedAttr.Font = UIFont.FromName("LemonMilk-Regular", 10);
                    item.SetTitleTextAttributes(normalAttr, UIControlState.Normal);
                    item.SetTitleTextAttributes(selectedAttr, UIControlState.Selected);
                }
            }
        }
    }
}