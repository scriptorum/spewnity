using System.Collections;
using System.Collections.Generic;

namespace Spewnity
{
	public class Map<T>
	{
		public T[,] contents;	
		public readonly int width;
		public readonly int height;
		
		public Map(int width, int height, T initialValue = default(T))
		{
			contents = new T[width,height];
			this.width = width;
			this.height = height;

			// If not using default value, set all map positions to the supplied initial value
			if(!EqualityComparer<T>.Default.Equals (initialValue, default(T)))
				SetAll(initialValue);
		}
		
		public T Get(int x, int y)
		{
			return contents[x,y];
		}
		
		public Map<T> Set(int x, int y, T value)
		{
			contents[x,y] = value;
			return this;
		}
		
		public T this[int x, int y]
		{
			get { return contents[x, y]; }
			set { contents[x, y] = value; }
		}
		
		public T this[Point point]
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
			return IsInBounds (p.x, p.y);
		}	

		public Map<T> Copy(int fromX, int fromY, int toX, int toY)
		{
			return Set (toX, toY, Get (fromX, fromY));
		}
		
		public Map<T> Copy(Point from, Point to)
		{
			return Copy (from.x, from.y, to.x, to.y);
		}
		
		public Map<T> SetAll (T value)
		{
			return EachPosition((x,y) => contents[x,y] = value);
		}

		// Clears each position of the map to the default value (NOT the initial value, use SetAll for all)
		public Map<T> Clear()
		{
			return SetAll(default(T));
		}
		
		// Iterates over each item in the map using the traversal order specified
		public Map<T> EachItem(System.Action<T> action, int traversalOrder = 0x0)
		{
			EachPosition((x,y) => action.Invoke (Get(x,y)), traversalOrder);
			return this;
		}
		
		// Iterates over each position in the map using the traversal order specified
		// Traversal.YFirst | Traversal.XReverse is the same as looping over Y
		// in ascending order, then nested looping over X in descending order.
		public Map<T>  EachPosition(System.Action<int,int> action, int order = 0x0)
		{
			// Cache flag checks
			bool revX = (order & (int) Traversal.XReverse) > 0;
			bool revY = (order & (int) Traversal.YReverse) > 0;
			bool yFirst = (order & (int) Traversal.YFirst) > 0;
			
			// Determine loop details based on flags
			LoopDetails loopX = new LoopDetails(revX ? width - 1 : 0, revX ? -1 : width, revX ? -1 : 1);
			LoopDetails loopY = new LoopDetails(revY ? height - 1 : 0, revY ? -1 : height, revY ? -1 : 1);

			// Do the loop as YX
			if(yFirst)
			{
				for(int y = loopY.start; y != loopY.stop; y += loopY.inc)
					for(int x = loopX.start; x != loopX.stop; x += loopX.inc)
						action.Invoke(x, y);
			}

			// As XY
			else
			{
				for(int x = loopX.start; x != loopX.stop; x += loopX.inc)
					for(int y = loopY.start; y != loopY.stop; y += loopY.inc)
						action.Invoke(x, y);
			}
			
			return this;
		}
		
		// Traversal order determines the order by which the map spots are enumerated.
		// If order doesn't matter, any will do. A traditional traversal is XY (0x0), 
		// which loops over the X axis first, then nested loops over the Y axis.
		// Add YFirst to swap the looping order, and XReverse and/or YReverse to 
		// iterate over either axis backwards.
		public enum Traversal { YFirst = 0x1, XReverse = 0x2, YReverse = 0x4 };
		
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