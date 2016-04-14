//
// A 2D integer point for tile maps
//
namespace Spewnity
{
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
	}
}