using Terminal.Gui;

namespace RadioFreeZerg.Windows
{
    public static class GuiHelper
    {
        public static void SetupScrollBars(ListView listView) {
            var stationsScrollBar = new ScrollBarView(listView, true);

            stationsScrollBar.ChangedPosition += () => {
                listView.TopItem = stationsScrollBar.Position;
                if (listView.TopItem != stationsScrollBar.Position)
                    stationsScrollBar.Position = listView.TopItem;
                listView.SetNeedsDisplay();
            };

            stationsScrollBar.OtherScrollBarView.ChangedPosition += () => {
                listView.LeftItem = stationsScrollBar.OtherScrollBarView.Position;
                if (listView.LeftItem != stationsScrollBar.OtherScrollBarView.Position)
                    stationsScrollBar.OtherScrollBarView.Position = listView.LeftItem;
                listView.SetNeedsDisplay();
            };

            listView.DrawContent += _ => {
                stationsScrollBar.Size = listView.Source.Count - 1;
                stationsScrollBar.Position = listView.TopItem;
                stationsScrollBar.OtherScrollBarView.Size = listView.Maxlength - 1;
                stationsScrollBar.OtherScrollBarView.Position = listView.LeftItem;
                stationsScrollBar.Refresh();
            };
        }
    }
}