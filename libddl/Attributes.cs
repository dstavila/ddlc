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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        public int Steps = 1;

        public CommandAttribute(int steps = 1)
        {
            Steps = steps;
        }
    }
}