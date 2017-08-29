/**
 * TODO Add hex support.
 */

using System.Collections;
using System.Collections.Generic;

namespace Spewnity
{
    public class Map<T>
    {
        // Traversal order determines the order by which the map spots are enumerated.
        // If order doesn't matter, any will do. A traditional traversal is XY (0x0),
        // which loops over the X axis first, then nested loops over the Y axis.
        // Add YFirst to swap the looping order, and XReverse and/or YReverse to
        // iterate over either axis backwards.
        public static int YFirst = 0x1;
        public static int XReverse = 0x2;
        public static int YReverse = 0x4;

        private List<Point> orthogonalNeighborOffsets;
        private List<Point> neighborOffsets;

        public T[, ] contents;
        public readonly int width;
        public readonly int height;

        public Map(int width, int height, T initialValue = default(T))
        {
            contents = new T[width, height];
            this.width = width;
            this.height = height;

            // If not using default value, set all map positions to the supplied initial value
            if (!EqualityComparer<T>.Default.Equals(initialValue, default(T))) SetAll(initialValue);
        }

        public T Get(int x, int y)
        {
            return contents[x, y];
        }

        public Map<T> Set(int x, int y, T value)
        {
            contents[x, y] = value;
            return this;
        }

        public T this [int x, int y]
        {
            get { return contents[x, y]; }
            set { contents[x, y] = value; }
        }

        public T this [Point point]
        {
            get { return contents[point.x, point.y]; }
            set { contents[point.x, point.y] = value; }
        }

        public bool IsInBounds(int x, int y)
        {
            return (x >= 0 && x < width && y >= 0 && y < height);
        }

        public bool IsInBounds(Point p)
        {
            return IsInBounds(p.x, p.y);
        }

        public bool IsEdge(int x, int y)
        {
            return (x == 0 || x == (width - 1) || y == 0 || y == (height - 1));
        }

        public bool IsEdge(Point p)
        {
            return IsEdge(p.x, p.y);
        }

        /**
         * Returns true if both points are legal, and adjacent to each other.
         * If orthogonal is false, adjacency includes diagonals.
         */
        public bool AreAdjacent(Point p1, Point p2, bool orthogonally = true)
        {
            if (!IsInBounds(p1)) return false;
            if (!IsInBounds(p2)) return false;
            int ox = (p1.x - p2.x).Abs();
            int oy = (p1.y - p2.y).Abs();

            if (ox + oy == 1) return true;

            if (!orthogonally) return ox == 1 && oy == 1;

            return false;
        }

        public List<Point> GetNeighbors(Point p, bool orthogonally = true)
        {
            List<Point> result = new List<Point>();
            List<Point> offsets = (orthogonally ? orthogonalNeighborOffsets : neighborOffsets);

            // Save neighbor offsets for future use
            if (offsets == null)
            {
                if (orthogonally) offsets = orthogonalNeighborOffsets = new List<Point>()
                {
                new Point(0, -1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, 1),
                };
                else offsets = neighborOffsets = new List<Point>()
                {
                    new Point(-1, -1),
                    new Point(0, -1),
                    new Point(1, -1),
                    new Point(-1, 0),
                    new Point(1, 0),
                    new Point(-1, 1),
                    new Point(0, 1),
                    new Point(1, 1)
                };
            }

            // Check all neighbors to make sure they're in bounds
            foreach (Point off in offsets)
            {
                Point check = p;
                check.Add(off);
                if (IsInBounds(check)) result.Add(check);
            }

            return result;
        }

        public Map<T> Copy(int fromX, int fromY, int toX, int toY)
        {
            return Set(toX, toY, Get(fromX, fromY));
        }

        public Map<T> Copy(Point from, Point to)
        {
            return Copy(from.x, from.y, to.x, to.y);
        }

        public Map<T> SetAll(T value)
        {
            return ForEach((x, y) => contents[x, y] = value);
        }

        // Clears each position of the map to the default value (NOT the initial value, use SetAll for all)
        public Map<T> Clear()
        {
            return SetAll(default(T));
        }

        override public string ToString()
        {
            return this.ToString(", ", "\n", "null");
        }

        public string ToString(string cellDelim, string lineDelim, string nullToString)
        {
            string result = "";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    T item = this [x, y];
                    result += (default(T) == null && item as object == null ? nullToString : item.ToString());
                    if (x < (width - 1)) result += cellDelim;
                }
                result += lineDelim;
            }
            return result;
        }

        /// <summary>
        /// Returns all points of the map that match the supplied predicate.
        /// </summary>
        /// <param name="predicate">A function that receives a point of the map and returns true if it is a match</param>
        /// <returns>A list of all matching points</returns>
        public List<Point> FindAll(System.Predicate<Point> predicate)
        {
            List<Point> list = new List<Point>();
            this.ForEach((Point p) =>
            {
                if (predicate(p))
                    list.Add(p);
            });
            return list;
        }

        /// <summary>
        /// Returns the first point found that matches the predicate supplied.
        /// </summary>
        /// <param name="predicate">A function that receives a point of the map and returns true if it is a match</param>
        /// <returns>The first point matching, or Point(-1,-1) if not found</returns>
        public Point Find(System.Predicate<Point> predicate)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Point pt = new Point(x, y);
                    if (predicate(pt))
                        return pt;
                }
            return new Point(-1, -1);
        }

        // Iterates over each item in the map using the traversal order specified
        public Map<T> ForEach(System.Action<T> action, int traversalOrder = 0x0)
        {
            ForEach((x, y) => action.Invoke(Get(x, y)), traversalOrder);
            return this;
        }

        public Map<T> ForEach(System.Action<Point> action, int traversalOrder = 0x0)
        {
            ForEach((x, y) => action.Invoke(new Point(x, y)), traversalOrder);
            return this;
        }

        // Iterates over each position in the map using the traversal order specified
        // Traversal.YFirst | Traversal.XReverse is the same as looping over Y
        // in ascending order, then nested looping over X in descending order.
        public Map<T> ForEach(System.Action<int, int> action, int traversalOrder = 0x0)
        {
            // Cache flag checks
            bool revX = (traversalOrder & XReverse) > 0;
            bool revY = (traversalOrder & YReverse) > 0;
            bool yFirst = (traversalOrder & YFirst) > 0;

            // Determine loop details based on flags
            LoopDetails loopX = new LoopDetails(revX ? width - 1 : 0, revX ? -1 : width, revX ? -1 : 1);
            LoopDetails loopY = new LoopDetails(revY ? height - 1 : 0, revY ? -1 : height, revY ? -1 : 1);

            // Do the loop as YX
            if (yFirst)
            {
                for (int y = loopY.start; y != loopY.stop; y += loopY.inc)
                    for (int x = loopX.start; x != loopX.stop; x += loopX.inc) action.Invoke(x, y);
            }

            // As XY
            else
            {
                for (int x = loopX.start; x != loopX.stop; x += loopX.inc)
                    for (int y = loopY.start; y != loopY.stop; y += loopY.inc) action.Invoke(x, y);
            }

            return this;
        }

        private struct LoopDetails
        {
            public int start;
            public int stop;
            public int inc;

            public LoopDetails(int start, int stop, int inc)
            {
                this.start = start;
                this.stop = stop;
                this.inc = inc;
            }
        }
    }
}