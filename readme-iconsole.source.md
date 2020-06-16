## IConsole

This is the main interface that all windows, and objects that wrap a window, or that wrap the `System.Console` writer. It implements the almost everything that `System.Console` does with some extra magic. 

`IConsole` is a well thought out .NET System.Console abstractions. Use to remove a direct dependancy on System.Console and replace with a dependancy on a well used and well known console interface, `IConsole`, to allow for building rich 'testable', high quality interactive console applications and utilities.

I have put  extra care into the design of the interfaces so that developers can choose the smallest interface that meets their needs in order to enable interoperability between open source console library and app developers. 

If you have any questions about how to use these abstractions (`interfaces`), please join the discussion on our gitter group at : https://gitter.im/goblinfactory-konsole/community or contact me directly and I will be glad to help.

## But I can write my own System.Console wrapper, it will be done in less than 40 lines? 

Yup, and many people do. [A quick search on Github returns more than 21K+ projects with their own form of IConsole or IConsoleWriter](https://github.com/search?l=C%23&q=%22%3A+IConsole%22&type=Code), so its very common (and easy) to do exactly that.

However, It's not about writing a wrapper, that is very easy. It's about setting a standard of interoperability between everyone that uses this as their interface. It's also about saving time. Since you would have started by writing your own interface, and then also writing something that implements that interface, why not save yourself the 2 or 3 hours you will be sidetracked doing that and dive right in to cleaning up your code.

You can use `IConsole` as simply as typing, `add package Goblinfactory.Konsole`. You can always come back later and remove it. 

## IConsole

This is the sum of all interfaces. It will require the most work to implement. Typically you often only need `IWrite` and-or  `IPrintAt` or `IPrintAtColor`. 

snippet: IConsole

<img src='docs/img/iconsole.png' align='center' />

## IWrite vs IConsole

If the app you are refactoring does not set the cursor position, and merely "writes" out via `System.Console` then use the `IWrite` interface as your dependancy. IWrite is good enough for 99% of `System.Console` refactorings, where you're essentially just logging stuff to the console. 

## How to use IConsole?

Pick the narrowest set of features that the class that you are refactoring depends on. 

Logging, printing only? `IWrite`, Needs to print in color? `IWriteColor`, Need to change the cursor position when printing? `IPrintAt`, eed to scroll portions of the screen? `IScrolling`, Need all of the above? `IConsole`.

Need only 2, e.g. printing only (no color) and printing at? Then use interface inheritance and implement just the bits you need. For example. 

```csharp

public interface IPrint : IWrite, IPrintAt { }

public class MyClass {
    public MyClass(IPrint print) { ...}
    ...
    _print.PrintAt(0, 60, $"Total {total}");
}
```

# Interfaces

## IWrite

Typically use for Logging and printing only. Nothing fancy, just writing something out the console or the build output.

snippet: IWrite

## ISetColors

Change the foreground and background color of what will get printed with the next Write, or WriteLine command.

snippet: ISetColors

## Colors (is a POCO class, and not an interface)

The eagle eyed amongst you will have spotted the single class file in this contract. Being able to specify the colors for something with a single assignment makes a lot of code easier to read. 

```csharp
myFoo.ForeGroundColor = System.ConsoleColor.Red;
myFoo.BackgroundColor = System.ConsoleColor.White;

vs

myFoo.Colors = MyStaticThemes.Default;
```
also, if you're writing Threadsafe code, then you'll be saving and restoring colors a lot.

```csharp
  lock(_staticLocker)
  {
    try
    {
      var currentColors = myFoo.Colors;
      myFoo.Colors = MyTheme.Highlighted;
      myFoo.WriteLine("I am highlighted item");
    }
    finally
    {
        myFoo.Colors = currentColors;
    }
  }
}
```
In fact, the pattern above is such a common pattern that the interface `IConsoleState` includes a dedicated method just for your implementation of that threadsafe pattern `void DoCommand(IConsole console, Action action);` the above code then becomes
```csharp
  myFoo.DoCommand(()=> {
    MyFoo.Colors = MyTheme.Highlited;
    myFoo.WriteLine("I am highlighted");
  );
```

#### Setting Colors

Set the foreground and background color in a single threadsafe way. i.e. locks using a static locker before setting the individual ForegroundColor and BackgroundColor properties. Colors is a syntactic shortcut for getting or setting both the Foreground and Background color in a single assignment. For example

calling 
```
console.Colors = new Colors(Red, White);
```
must be implemented such that it is the same as having called 
```
console.ForegroundColor = Red; 
console.BackgroundColor = White;
```

## IWriteColor 

If you need to print in color. 

snippet: IWriteColor

## IPrintAt

Interface for a class that needs to print at a specific location in a window. 

snippet: IPrintAt

## IPrintAtColor

snippet: IPrintAtColor

## IScrolling

Interface for a class that needs to be able to scroll portions of the screen. This will most likely cause your library to require platform specific implementations for scrolling.

snippet: IScrolling

## IWindowed

If you are writing a windowing library like `Konsole` then each window region needs to report back an AbsoluteX and AbsoluteY position so that printing can happen at the correct (relative) position on the real console.

snippet: IWindowed

## IConsoleState

Interface for all the console methods that are most at risk of causing corruptions in multithreaded programs. The way to protect against corruption is to manage locking and manually save and restore state.

snippet: IconsoleState

#### DoCommand

`void DoCommand(IConsole console, Action action)`

Runs an action that may or may not modify the console state that can cause corruptions when thread context swaps. Must lock on a static locker, do try catch, and ensure state is back to what it was before the command ran. If you're not writing a threadsafe control or threading is not an issue, then you can simply call `action()` in your implementation.

example implementation;

```csharp
  lock(_locker)
  {
    var state = console.State;
    try
    {
      action();
    }
    finally
    {
      console.State = state;</code>
    }
  }
```

# If you refactoring a project to remove depedencies on System.Console

## Getting Started

1. Find code that writes to the `System.Console` directly. Do a grep search for `Console.*` to get started.

```csharp
public class Accounts
{
  ... some code here

  // methods below writes to the console

  public void DoSomethingWithInvoice(Invoice inv) {
      ... some code
      Console.WriteLine($"Invoice #{inv.Number}, Date: {inv.Date} Amount:{inv.Amount}");
      ...
  }
}

```

2. Install `IConsole` interfaces

> install-package `Goblinfactory.Konsole`

3. Create your own live and  test stubb, Mock or fake that implements `IConsole`.
4. Refactor your code to use the  `IConsole` abstraction. `IConsole` uses `Konsole` as the parent namespace. 

#### example


```csharp

using Konsole; 

public class Accounts
{
  ... some code here
  private IConsole _console;
  public Accounts(IConsole console) {
      _console = console;
  }
  // change all writes to System.Console to use the injected IConsole
  
  public void DoSomethingWithInvoice(Invoice inv) {
      ... some code
      _console_.WriteLine($"Invoice #{inv.Number}, Date: {inv.Date} Amount:{inv.Amount}");
      ...
  }
}
```

5. now you can test your class


```csharp

using Konsole; 

    [Test]
    public void doing_something_to_invoices_must_print_the_invoice_details_to_the_console()
    {
        IConsole console = new MockConsole();
        var accounts = new Accounts(console);
        accounts.DoSomethingWithInvoice(TestData.MyTestInvoice1);

        // confirm the display is what you expected

        var expected = "Invoice #101, Date:21st February 2020, Amount:£ 1234.56";
        
        console.Buffer.Should().BeEquivalentTo(expected);
    }

```


## Pre-built battle hardened thread safe implementations for production and testing uses

Obviously because `IConsole` is distributed as part of the main `Goblinfactory.Konsole` library, you automatically have access to battle hardened threadsafe implementations of all of these interfaces, specifically;

- `ConcurrentWriter() : IConsole`
- `NullWriter() : IConsole` 
- `Window() : IConsole`
- `MockConsole() : IConsole` 

.. in addition to  a full suite of Console library utilities, Box, Forms, ProgressBar, Windows and more that are all `IConsole` compatible.

<hr/>

back to [table of contents](readme.md#contents)