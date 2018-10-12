﻿using System.Collections.Generic;

namespace ddlc
{
    public enum EType
    {
        UNKNOWN = 0,
        UINT8,
        UINT16,
        UINT32,
        UINT64,
        INT8,
        INT16,
        INT32,
        INT64,
        FLOAT32,
        FLOAT64,
        VECTOR2,
        VECTOR3,
        VECTOR4,
        Quaternion,
        STRING,
        SELECT,
        BITFIELD,
        STRUCT,
        BOOLEAN,
        MAX,
    }

    public enum EArrayType
    {
        SCALAR = 0,
        FIXED,
        DYNAMIC,
        LIST,
        MAX,
    }


    public class rSelectItem
    {
        public string Name;
        public string Label;
        public string Description;
        public uint NameHash;
        public string Value;
    }

    public class rSelect
    {
        public string Name;
        public string Label;
        public string Description;
        public string Namespace;
        public uint NameHash;
        public int DefaultItem;
        public List<rSelectItem> Items = new List<rSelectItem>();
    }


    public class rStructField
    {
        public string Name;
        public string Label;
        public string Description;
        public string TypeName;

        public uint NameHash;
        public uint Count;
        public EType Type;
        public EArrayType ArrayType;

        public string Value;
    }

    public class rStruct
    {
        public uint NameHash;
        public string Name;
        public string Description;
        public List<string> Namespace;
        public List<rStructField> Fields = new List<rStructField>();
        public List<rStruct> Childs = new List<rStruct>();
        

        public string CSharpFullname
        {
            get { return build_fullpath("."); }
        }
        public string CPPFullname
        {
            get { return build_fullpath("::"); }
        }

        private string build_fullpath(string separator)
        {
            if (Namespace.Count == 0)
                return Name;

            string result = Namespace[0];
            for (int i = 1; i < Namespace.Count; ++i)
            {
                result = string.Format("{0}{2}{1}", result, Namespace[i], separator);
            }

            result = string.Format("{0}{2}{1}", result, Name, separator);
            return result;
        }
    }

    public class rNamespace
    {
        public string Name;
        public List<string> Selects = new List<string>();
        public List<string> Structs = new List<string>();
    }


    public static class Converter
    {
        public static EType StringToDDLType(string str,
            List<rSelect> selects,
            List<rStruct> structs)
        {
            if (str == "float" || str == "f32") return EType.FLOAT32;
            if (str == "double" || str == "f64") return EType.FLOAT64;
            if (str == "byte" || str == "i8") return EType.INT8;
            if (str == "sbyte" || str == "u8") return EType.UINT8;
            if (str == "short" || str == "i16") return EType.INT16;
            if (str == "ushort" || str == "u16") return EType.UINT16;
            if (str == "int" || str == "i32") return EType.INT32;
            if (str == "uint" || str == "u32") return EType.UINT32;
            if (str == "long" || str == "i64") return EType.INT64;
            if (str == "ulong" || str == "u64") return EType.UINT64;
            if (str == "float2" || str == "Vector2") return EType.VECTOR2;
            if (str == "float3" || str == "Vector3") return EType.VECTOR3;
            if (str == "float4" || str == "Vector4") return EType.VECTOR4;
            if (str == "Quaternion") return EType.Quaternion;
            if (str == "bool") return EType.BOOLEAN;
            if (str == "string") return EType.STRING;
            foreach (var s in selects)
            {
                if (s.Name == str)
                    return EType.SELECT;
            }

            foreach (var s in structs)
            {
                if (s.Name == str)
                    return EType.STRUCT;
            }

            return EType.UNKNOWN;
        }

        public static string DDLTypeToCSharpType(EType t, string typeName)
        {
            if (t == EType.UINT8)  return "ubyte";
            if (t == EType.UINT16) return "ushort";
            if (t == EType.UINT32) return "uint";
            if (t == EType.UINT64) return "ulong";
            if (t == EType.INT8)  return "byte";
            if (t == EType.INT16) return "short";
            if (t == EType.INT32) return "int";
            if (t == EType.INT64) return "long";
            if (t == EType.FLOAT32) return "float";
            if (t == EType.FLOAT64) return "double";
            if (t == EType.STRING) return "string";
            if (t == EType.BOOLEAN) return "bool";
            if (t == EType.VECTOR2) return "Vector2";
            if (t == EType.VECTOR3) return "Vector3";
            if (t == EType.VECTOR4) return "Vector4";
            if (t == EType.Quaternion) return "Quaternion";
            if (t == EType.SELECT) return typeName;
            if (t == EType.BITFIELD) return typeName;
            if (t == EType.STRUCT) return typeName;
            return "ERROR";
        }
        
        public static string DDLTypeToCPPType(EType t, string typeName)
        {
            if (t == EType.UINT8)  return "u8";
            if (t == EType.UINT16) return "u16";
            if (t == EType.UINT32) return "u32";
            if (t == EType.UINT64) return "u64";
            if (t == EType.INT8)  return "i8";
            if (t == EType.INT16) return "i16";
            if (t == EType.INT32) return "i32";
            if (t == EType.INT64) return "i64";
            if (t == EType.FLOAT32) return "f32";
            if (t == EType.FLOAT64) return "f64";
            if (t == EType.STRING) return "std::string";
            if (t == EType.BOOLEAN) return "bool";
            if (t == EType.VECTOR2) return "float2";
            if (t == EType.VECTOR3) return "float3";
            if (t == EType.VECTOR4) return "float4";
            if (t == EType.Quaternion) return "Quaternion";
            if (t == EType.SELECT) return typeName;
            if (t == EType.BITFIELD) return typeName;
            if (t == EType.STRUCT) return typeName;
            return "ERROR";
        }

        public static bool IsPOD(EType t)
        {
            if (t == EType.UINT8) return true;
            if (t == EType.UINT16) return true;
            if (t == EType.UINT32) return true;
            if (t == EType.UINT64) return true;
            if (t == EType.INT8) return true;
            if (t == EType.INT16) return true;
            if (t == EType.INT32) return true;
            if (t == EType.INT64) return true;
            if (t == EType.FLOAT32) return true;
            if (t == EType.FLOAT64) return true;
            if (t == EType.STRING)  return true;
            if (t == EType.BOOLEAN) return true;
            if (t == EType.SELECT)   return true;
            if (t == EType.BITFIELD) return true;
            return false;
        }
    }
}