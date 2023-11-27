using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var tickRate = TimeSpan.FromMilliseconds(100);
        var snakeGame = new SnakeGame();

        using (var cts = new CancellationTokenSource())
        {
            async Task MonitorKeyPresses()
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        snakeGame.OnKeyPress(key);
                    }

                    await Task.Delay(10);
                }
            }

            var monitorKeyPresses = MonitorKeyPresses();

            do
            {
                snakeGame.OnGameTick();
                snakeGame.Render();
                await Task.Delay(tickRate);
            } while (!snakeGame.GameOver);

            // Allow time for the user to weep before the application exits.
            for (var i = 0; i < 3; i++)
            {
                Console.Clear();
                await Task.Delay(500);
                snakeGame.Render();
                await Task.Delay(500);
            }

            cts.Cancel();
            await monitorKeyPresses;
        }
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

class Apple : IRenderable
{
    public Apple(Position position)
    {
        Position = position;
    }

    public Position Position { get; }

    public void Render()
    {
        Console.SetCursorPosition(Position.Left, Position.Top);
        Console.Write("A");
    }
}


class Snake : IRenderable
{
    private List<Position> _body;
    private int _growthSpurtsRemaining;

    public Snake(Position spawnLocation, int initialSize = 1)
    {
        _body = new List<Position> { spawnLocation };
        _growthSpurtsRemaining = Math.Max(0, initialSize - 1);
        Dead = false;
    }

    public bool Dead { get; set; }
    public Position Head => _body.First();
    public IEnumerable<Position> Body => _body.Skip(1);

    public void Move(Direction direction)
    {
        if (Dead) throw new InvalidOperationException();

        Position newHead;

        switch (direction)
        {
            case Direction.Up:
                newHead = Head.DownBy(-1);
                break;

            case Direction.Left:
                newHead = Head.RightBy(-1);
                break;

            case Direction.Down:
                newHead = Head.DownBy(1);
                break;

            case Direction.Right:
                newHead = Head.RightBy(1);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_body.Contains(newHead) || !PositionIsValid(newHead))
        {
            Dead = true;
            return;
        }

        _body.Insert(0, newHead);

        if (_growthSpurtsRemaining > 0)
        {
            _growthSpurtsRemaining--;
        }
        else
        {
            _body.RemoveAt(_body.Count - 1);
        }
    }

    public void Grow()
    {
        if (Dead) throw new InvalidOperationException();

        _growthSpurtsRemaining++;
    }

    public void Render()
    {
        Console.SetCursorPosition(Head.Left, Head.Top);
        Console.Write("X");

        foreach (var position in Body)
        {
            Console.SetCursorPosition(position.Left, position.Top);
            Console.Write("■");
        }
    }

    private static bool PositionIsValid(Position position) =>
        position.Top >= 0 && position.Left >= 0;
}
