using System;

namespace PunchItClient
{
    public class FrameSegment
    {
        public FrameSegment(string text, ConsoleColor? colorF, ConsoleColor? colorB)
        {
            Text = text;
            ColorF = colorF;
            ColorB = colorB;
        }

        public ConsoleColor? ColorF { get; set; }
        public ConsoleColor? ColorB { get; set; }
        public string Text { get; set; }

    }
}