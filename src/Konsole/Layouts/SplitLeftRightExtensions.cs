﻿using System;
using static Konsole.BorderCollapse;

namespace Konsole
{
    public static class SplitLeftRightExtensions
    {
        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, null, null, LineThickNess.Single, border, c.ForegroundColor, c.BackgroundColor);
        }

        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, ConsoleColor foreground, ConsoleColor background, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, null, null, LineThickNess.Single, border, foreground, background);
        }

        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, string leftTitle, string rightTitle, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, leftTitle, rightTitle, LineThickNess.Single, border, c.ForegroundColor, c.BackgroundColor);
        }

        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, string leftTitle, string rightTitle, ConsoleColor foreground, ConsoleColor background, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, leftTitle, rightTitle, LineThickNess.Single, border, foreground, background);
        }

        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, string leftTitle, string rightTitle, LineThickNess thickness, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, leftTitle, rightTitle, thickness, border, c.ForegroundColor, c.BackgroundColor);
        }

        public static(IConsole left, IConsole right) SplitLeftRight(this Window c, string leftTitle, string rightTitle, LineThickNess thickness, ConsoleColor foreground, ConsoleColor background, BorderCollapse border = Collapse)
        {
            return _SplitLeftRight(c, leftTitle, rightTitle, thickness, border, foreground, background);
        }

        internal static (IConsole left, IConsole right) _SplitLeftRight(IConsole c, string leftTitle, string rightTitle, LineThickNess thickness, BorderCollapse border, ConsoleColor foreground, ConsoleColor background)
        {
            if (border == None)
            {
                var left = LayoutExtensions._LeftRight(c, leftTitle, false, false, thickness, foreground);
                var right = LayoutExtensions._LeftRight(c, rightTitle, true, false, thickness, foreground);
                return (left, right);
            }
            if (border == Separate)
            {
                var left = LayoutExtensions._LeftRight(c, leftTitle, false, true, thickness, foreground);
                var right = LayoutExtensions._LeftRight(c, rightTitle, true, true, thickness, foreground);
                return (left, right);
            }

            lock (Window._staticLocker)
            {
                int h = c.WindowHeight;
                int w = c.WindowWidth - 3;
                int leftWidth = w / 2;
                int rightWidth = (w - leftWidth);

                c.DoCommand(c, () =>
                {
                    //todo need unit test for merging two boxes :D for now, lets print them twice so we get true overlap to start with
                    new Draw(c)
                    .Box(0, 0, leftWidth + 1, h - 1, leftTitle, thickness);
                    new Draw(c)
                    .Box(leftWidth + 1, 0,  rightWidth + leftWidth + 2, h - 1, rightTitle, thickness);
                    // print the corners
                    c.PrintAt(leftWidth + 1, 0, '┬');
                    c.PrintAt(leftWidth + 1, h - 1, '┴');
                });

                var leftWin = Window._CreateFloatingWindow(1, 1, leftWidth, h - 2, foreground, background, true, c, null);
                var rightWin = Window._CreateFloatingWindow(leftWidth + 2, 1, rightWidth, h - 2, foreground, background, true, c, null);
                return (leftWin, rightWin);
            }
        }
    }
}
