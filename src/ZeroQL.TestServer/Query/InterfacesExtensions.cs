namespace ZeroQL.TestServer.Query;

public interface IFigure
{
    float Perimeter { get; }
}

public class Point : IFigure
{
    public float X { get; set; }
    public float Y { get; set; }

    public float Perimeter => 0;
}

public class Square : IFigure
{
    public Point TopLeft { get; set; }

    public Point BottomRight { get; set; }
    
    public float Perimeter => (TopLeft.Y - BottomRight.Y) * 2 + (BottomRight.Y - TopLeft.Y) * 2;
}

public class Circle : IFigure
{
    public Point Center { get; set; }

    public float Radius { get; set; }
    
    public float Perimeter => (float)Math.PI * 2 * Radius;

}

[ExtendObjectType(typeof(Query))]
public class InterfacesExtensions
{
    public IFigure[] GetFigures()
    {
        return GetCircles().Concat(
                GetSquares().OfType<IFigure>())
            .ToArray();
    }

    public Circle[] GetCircles()
    {
        return Enumerable
            .Range(0, 10)
            .Select(o => new Circle
            {
                Center = new Point { X = o, Y = o },
                Radius = o
            })
            .ToArray();
    }

    public Square[] GetSquares()
    {
        return Enumerable
            .Range(0, 10)
            .Select(o => new Square
            {
                TopLeft = new Point { X = o, Y = o },
                BottomRight = new Point { X = o + 1, Y = o + 1 },
            })
            .ToArray();
    }
}