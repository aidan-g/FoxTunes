using System;

namespace FoxTunes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class PlatformDependencyAttribute : Attribute
    {
        public PlatformDependencyAttribute()
        {

        }

        public int Major { get; set; }

        public int Minor { get; set; }
    }
}
