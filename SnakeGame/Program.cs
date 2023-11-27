using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    public static async Task Main(string[] args)
    {

    }
}

enum Direction
{
    Up,
    Down,
    Left,
    Right
}

interface IRenderable
{
    void Render();
}

readonly struct Position
{
    public Position(int top, int left)
    {
        Top = top;
        Left = left;
    }
    public int Top { get; }
    public int Left { get; }

    public Position RightBy(int n) => new Position(Top, Left + n);
    public Position DownBy(int n) => new Position(Top + n, Left);
}