//
// A 2D integer point for tile maps
// TODO: Generic parameters, Point<int> vs Point<float>
// TODO: Vector conversions, such as GetAngle(), GetMagnitude(), Normalize(), and Point.FromAngle(angle, magnitude)
//
namespace Spewnity
{
    [System.Serializable]
    public struct Point
    {
        public int x;
        public int y;

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        override public string ToString()
        {
            return "Point(" + this.x + "," + this.y + ")";
        }

        public void Add(Point otherPoint)
        {
            this.x += otherPoint.x;
            this.y += otherPoint.y;
        }

        public override bool Equals(System.Object obj)
        {
            return obj is Point && this == (Point) obj;
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public static bool operator == (Point left, Point right)
        {
            return (left.x == right.x && left.y == right.y);
        }

        public static bool operator != (Point x, Point y)
        {
            return !(x == y);
        }

        public static Point operator + (Point left, Point right)
        {
            return new Point(left.x + right.x, left.y + right.y);
        }

        public static Point operator - (Point left, Point right)
        {
            return new Point(left.x - right.x, left.y - right.y);
        }

        public static implicit operator UnityEngine.Vector2(Point p)
        {
            return new UnityEngine.Vector2(p.x, p.y);
        }

        public static implicit operator UnityEngine.Vector3(Point p)
        {
            return new UnityEngine.Vector3(p.x, p.y, 0);
        }

        public static Point zero = new Point(0, 0);
    }
}