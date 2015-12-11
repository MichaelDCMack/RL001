namespace Code
{
    public struct Point
    {
        public int x, y;

        public Point(int px, int py)
        {
            x = px;
            y = py;
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }

        public override string ToString()
        {
            return "x = " + x + " y = " + y;
        }

        public float DistanceSquared(Point p)
        {
            return (x - p.x) * (x - p.x) + (y - p.y) * (y - p.y);
        }
    }
}