using Terminal.Gui;

namespace RadioFreeZerg.Windows
{
    public static class GuiHelper
    {
        public static void SetupScrollBars(ListView stationsListView) {
            var stationsScrollBar = new ScrollBarView(stationsListView, true);

            stationsScrollBar.ChangedPosition += () => {
                stationsListView.TopItem = stationsScrollBar.Position;
                if (stationsListView.TopItem != stationsScrollBar.Position)
                    stationsScrollBar.Position = stationsListView.TopItem;
                stationsListView.SetNeedsDisplay();
            };

            stationsScrollBar.OtherScrollBarView.ChangedPosition += () => {
                stationsListView.LeftItem = stationsScrollBar.OtherScrollBarView.Position;
                if (stationsListView.LeftItem != stationsScrollBar.OtherScrollBarView.Position)
                    stationsScrollBar.OtherScrollBarView.Position = stationsListView.LeftItem;
                stationsListView.SetNeedsDisplay();
            };

            stationsListView.DrawContent += _ => {
                stationsScrollBar.Size = stationsListView.Source.Count - 1;
                stationsScrollBar.Position = stationsListView.TopItem;
                stationsScrollBar.OtherScrollBarView.Size = stationsListView.Maxlength - 1;
                stationsScrollBar.OtherScrollBarView.Position = stationsListView.LeftItem;
                stationsScrollBar.Refresh();
            };
        }
    }
}