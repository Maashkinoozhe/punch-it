using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIModel;

namespace PunchItClient
{
    class EditRecordsInteractiveMode : InteractiveModeViewAdapter
    {
        private GameMode _mode;
        private Record _model;
        private bool _done;
        //Show
        private int currentEntry;
        private int currentColumn;

        public EditRecordsInteractiveMode(Record model)
        {
            _model = model;
            _mode = GameMode.Show;
            _done = false;
            InitShow();
        }

        public override void HandleUserAction(InteractiveUserAction action)
        {
            if (_mode == GameMode.Show)
            {
                HandleAction_Show(action);
            }

            if (action == InteractiveUserAction.Quit) _done = true;

        }

        public override List<FrameSegment> GetFrameSegments()
        {
            List<FrameSegment> segments = null;
            if (_mode == GameMode.Show)
            {
                segments = GetSegmentsFor_Show();
            }

            return segments;
        }

        private void InitShow()
        {
            SetShow(_model.RecordEntries.Count - 1, 1);
        }

        private void SetShow(int entry, int column)
        {
            if (entry >= 0 && entry < _model.RecordEntries.Count)
            {
                currentEntry = entry;
            }

            if (column >= 0 && column <= 2)
            {
                currentColumn = column;
            }
        }

        private List<FrameSegment> GetSegmentsFor_Show()
        {
            List<FrameSegment> segments = new List<FrameSegment>();

            segments.AddRange(GetHeader_Show());

            segments.Add(new FrameSegment("\n", null, null));

            segments.AddRange(GetContentFor_Show());

            return segments;
        }

        private IEnumerable<FrameSegment> GetHeader_Show()
        {
            List<FrameSegment> segments = new List<FrameSegment>();
            segments.Add(new FrameSegment("Edit the RecordEntries.\n", null, null));

            segments.Add(new FrameSegment("Navigate with ", null, null));
            segments.Add(new FrameSegment(" " + $"{"\u2190"} {"\u2191"} {"\u2193"} {"\u2192"}" + " Arrows.", ConsoleColor.Green, null));

            segments.Add(new FrameSegment(" Change time with ", null, null));
            segments.Add(new FrameSegment(" PageUp, PageDown ", ConsoleColor.Green, null));

            segments.Add(new FrameSegment(" Quit with", null, null));
            segments.Add(new FrameSegment(" q, Q ", ConsoleColor.Green, null));

            segments.Add(new FrameSegment(" Quit and save with", null, null));
            segments.Add(new FrameSegment(" Enter", ConsoleColor.Green, null));

            segments.Add(new FrameSegment("\n", null, null));

            return segments;
        }

        private IEnumerable<FrameSegment> GetContentFor_Show()
        {
            int PackageNameWidth = 8;
            List<FrameSegment> segments = new List<FrameSegment>();

            foreach (var entry in _model.RecordEntries)
            {
                var isSelected = entry == _model.RecordEntries[currentEntry];
                var endIsEditable = entry.End.HasValue;
                var packageColor = isSelected && currentColumn == 0 ? ConsoleColor.Yellow : ConsoleColor.Cyan;
                var startColor = isSelected && currentColumn == 1 ? ConsoleColor.Yellow : ConsoleColor.Green;
                var endColor = isSelected && currentColumn == 2 ? ConsoleColor.Yellow : endIsEditable ? ConsoleColor.Green : ConsoleColor.Gray;

                segments.Add(new FrameSegment("\t" + $"{entry.PackageKey.PadRight(PackageNameWidth)}", packageColor, null));

                segments.Add(new FrameSegment("\t" + "Start: ", ConsoleColor.Gray, null));
                segments.Add(new FrameSegment($"{entry.Start.Value.Hour:00}:{entry.Start.Value.Minute:00}", startColor, null));

                segments.Add(new FrameSegment("\t" + "End: ", ConsoleColor.Gray, null));
                if (endIsEditable)
                {
                    segments.Add(new FrameSegment($"{entry.End.Value.Hour:00}:{entry.End.Value.Minute:00}", endColor, null));
                }
                else
                {
                    segments.Add(new FrameSegment($"--:--", endColor, null));
                }

                segments.Add(new FrameSegment("\n", null, null));
            }

            return segments;
        }

        private void HandleAction_Show(InteractiveUserAction action)
        {
            if (action == InteractiveUserAction.Up) SetShow(currentEntry - 1, currentColumn);
            else if (action == InteractiveUserAction.Down) SetShow(currentEntry + 1, currentColumn);
            else if (action == InteractiveUserAction.Left) SetShow(currentEntry, currentColumn - 1);
            else if (action == InteractiveUserAction.Right) SetShow(currentEntry, currentColumn + 1);

            else if (action == InteractiveUserAction.PageUp) ChangeModel_Show(1);
            else if (action == InteractiveUserAction.PageDown) ChangeModel_Show(-1);
        }

        private void ChangeModel_Show(int direction)
        {
            var entry = _model.RecordEntries[currentEntry];
            if (currentColumn == 0)
            {

            }
            else
            {
                var time = currentColumn == 1 ? entry.Start.Value : entry.End.Value;
                time += TimeSpan.FromMinutes(5 * direction);
                time = time - TimeSpan.FromMinutes(time.Minute) + TimeSpan.FromMinutes((time.Minute / 5) * 5);
                var x = currentColumn == 1 ? entry.Start = time : entry.End = time;
            }
        }


        public override bool IsDone()
        {
            return _done;
        }

        private enum GameMode
        {
            Show,
            SelectPackage,
            Finished
        }
    }
}
