using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public static class GuiHelper
    {
        public static void SetupScrollBars(ListView listView) {
            var stationsScrollBar = new ScrollBarView(listView, true);

            stationsScrollBar.ChangedPosition += () => {
                if (listView.Visible) {
                    listView.TopItem = stationsScrollBar.Position;
                    if (listView.TopItem != stationsScrollBar.Position)
                        stationsScrollBar.Position = listView.TopItem;
                    listView.SetNeedsDisplay();
                }
            };

            stationsScrollBar.OtherScrollBarView.ChangedPosition += () => {
                if (listView.Visible) {
                    listView.LeftItem = stationsScrollBar.OtherScrollBarView.Position;
                    if (listView.LeftItem != stationsScrollBar.OtherScrollBarView.Position)
                        stationsScrollBar.OtherScrollBarView.Position = listView.LeftItem;
                    listView.SetNeedsDisplay();
                }
            };

            listView.DrawContent += _ => {
                if (listView.Visible) {
                    stationsScrollBar.Size = listView.Source?.Count - 1 ?? -1;
                    stationsScrollBar.Position = listView.TopItem;
                    stationsScrollBar.OtherScrollBarView.Size = listView.Maxlength - 1;
                    stationsScrollBar.OtherScrollBarView.Position = listView.LeftItem;
                    stationsScrollBar.Refresh();
                }
            };
        }
    }
}