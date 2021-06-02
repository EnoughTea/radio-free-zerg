using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NStack;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    /// <summary>
    ///     Implements an <see cref="T:Terminal.Gui.IListDataSource" /> that renders <see cref="RadioStation" />s
    ///     for <see cref="T:Terminal.Gui.ListView" />.
    /// </summary>
    /// <remarks>Implements support for rendering marked items.</remarks>
    public class RadioStationListSource : IListDataSource
    {
        private readonly UserState state;
        private BitArray marks = null!;
        private IList<RadioStation> stations = null!;

        public RadioStationListSource(IEnumerable<RadioStation> source, UserState userState) {
            state = userState;
            Init(source.ToList());
        }

        public IList<RadioStation> Stations {
            get => stations;
            set => Init(value);
        }

        /// <inheritdoc />
        public int Count => Stations.Count;

        /// <inheritdoc />
        public int Length { get; private set; }

        /// <inheritdoc />
        public void Render(ListView container,
                           ConsoleDriver driver,
                           bool marked,
                           int item,
                           int col,
                           int line,
                           int width,
                           int start = 0) {
            container.Move(col, line);
            var station = Stations[item];
            if (station != null) {
                string stationRepr = station.ToString();
                if (state.Favs.FavoriteStationsIds.Contains(station.Id)) {
                    stationRepr = $"* {stationRepr}";
                }
                
                RenderUstr(driver, stationRepr, col, line, width, start);
            } else {
                RenderUstr(driver, ustring.Make(""), col, line, width);
            }
        }

        /// <inheritdoc />
        public bool IsMarked(int item) => item >= 0 && item < Stations.Count && marks[item];

        /// <inheritdoc />
        public void SetMark(int item, bool value) {
            if (item < 0 || item >= Stations.Count) return;

            marks[item] = value;
        }

        /// <inheritdoc />
        public IList ToList() => (IList) Stations;

        private void Init(IList<RadioStation> source) {
            stations = source;
            marks = new BitArray(Stations.Count);
            Length = GetMaxLengthItem();
        }

        private int GetMaxLengthItem() =>
            Stations.Select(station => {
                        const int favoriteMarkLength = 2;
                        var stationLength = station.ToString().Length;
                        return state.Favs.FavoriteStationsIds.Contains(station.Id)
                            ? stationLength + favoriteMarkLength
                            : stationLength;
                    })
                    .Prepend(0)
                    .Max();

        private static void RenderUstr(
            ConsoleDriver driver,
            ustring ustr,
            int col,
            int line,
            int width,
            int start = 0) {
            var length = ustr.Length;
            var totalColumnWidth = 0;
            int size;
            for (var currentColumn = start; currentColumn < length; currentColumn += size) {
                uint rune;
                (rune, size) = Utf8.DecodeRune(ustr, currentColumn, currentColumn - length);
                var columnWidth = Rune.ColumnWidth(rune);
                if (totalColumnWidth + columnWidth <= width) {
                    driver.AddRune(rune);
                    totalColumnWidth += columnWidth;
                } else {
                    break;
                }
            }

            for (; totalColumnWidth < width; ++totalColumnWidth) {
                driver.AddRune(' ');
            }
        }
    }
}