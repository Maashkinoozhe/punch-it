using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PunchItClient
{
    public interface IInteractiveModeViewAdapter
    {
        void HandleUserAction(InteractiveUserAction action);
        List<FrameSegment> GetFrameSegments();
        bool IsDone();
    }

    public abstract class InteractiveModeViewAdapter : IInteractiveModeViewAdapter
    {
        public abstract void HandleUserAction(InteractiveUserAction action);
        public abstract List<FrameSegment> GetFrameSegments();
        public abstract bool IsDone();
    }

    public enum InteractiveUserAction
    {
        Up,
        Down,
        Left,
        Right,
        Quit,
        Enter,
        PageUp,
        PageDown
    }
}
