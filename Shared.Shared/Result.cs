using System.Collections.Generic;

namespace Shared.Shared
{
    public class Result<T>
    {
        public long    Total        { get; set; }
        public int     CurrentIndex { get; set; }
        public List<T> Data         { get; set; }
        
    }
}