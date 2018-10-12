using System;

namespace ddl
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class SelectAttribute : Attribute
    {
        public string Description;
        public string Label;
    }

    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class BitfieldAttribute : Attribute
    {
        public string Description;
        public string Label;
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ListAttribute : Attribute
    {}
}