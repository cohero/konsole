﻿using System;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;

namespace Konsole.Tests.WindowTests
{
    [UseReporter(typeof(DiffReporter))]
    public class WriteShould
    {
        [Test]
        public void when_write_ends_exactly_at_end_of_line_should_move_cursor_to_next_line()
        {
            //var con = new MockConsole(6, 3);
            //con.Write("123");
            //con.CursorLeft.Should().Be(3);
            //con.CursorTop.Should().Be(0);
            //con.Write("456");
            //con.CursorLeft.Should().Be(0);
            //con.CursorTop.Should().Be(1);
            //con.Write("ABCDE");
            //con.Write("1234");
            //var expected = new[]
            //{
            //    "123456",
            //    "ABCDE1",
            //    "234   "
            //};
            //con.Buffer.Should().BeEquivalentTo(expected);

            var con = new MockConsole(6, 3);
            con.Write("123456");
            con.Write("ABCDEF");
            con.Write("XY    ");
            var expected = new[]
            {
                "123456",
                "ABCDEF",
                "XY    "
            };
            con.Buffer.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void write_relative_to_the_window_being_printed_to_not_the_parent()
        {
            var c = new MockConsole(6, 4);
            c.WriteLine("------");
            c.WriteLine("------");
            c.WriteLine("------");
            c.Write("------");
            var w = new Window(1, 1, 4, 2, c, K.Transparent);
            w.Write("X");
            w.Write(" Y");
            var expected = new[]
            {
                "------",
                "-X Y--",
                "------",
                "------"
            };
            Console.WriteLine(c.BufferWrittenString);
            c.Buffer.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void write_to_end_of_line_and_WriteLine_should_write_to_current_line_and_move_cursor_to_beginning_of_next_line()
        {
            var console = new MockConsole(80, 20);
            console.WriteLine("line1");
            console.Write("This ");
            console.Write("is ");
            console.WriteLine("a test line.");
            console.WriteLine("line 3");

            var expected = new[]
            {
                "line1",
                "This is a test line.",
                "line 3"
            };
            System.Console.WriteLine(console.BufferWrittenString);
            Assert.That(console.BufferWrittenTrimmed, Is.EqualTo(expected));
        }


        [Test]
        public void print_to_the_parent_if_echo_set()
        {
            var console = new MockConsole(3, 3);
            console.ForegroundColor = ConsoleColor.Red;
            console.BackgroundColor = ConsoleColor.White;
            console.PrintAt(0, 0, "X");

            var w = new Window(console);
            w.Write("YY");

            var expectedAfter = new[]
            {
                "YY ",
                "   ",
                "   "
            };

            Assert.AreEqual(expectedAfter, console.Buffer);
        }


        [Test]
        public void not_increment_cursortop_or_left_of_parent_window()
        {
            var parent = new MockConsole(80, 20);
            var state = parent.State;

            var console = new Window(parent);
            state.Should().BeEquivalentTo(parent.State);

            console.Write("This is");
            state.Should().BeEquivalentTo(parent.State);

            console.Write(" a test line");
            state.Should().BeEquivalentTo(parent.State);
        }


    }
}