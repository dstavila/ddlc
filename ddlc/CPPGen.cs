using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;


namespace ddlc
{
    public static class CPPGen
    {
//        public static List<SelectDefinition> Selects = new List<SelectDefinition>();
//        public static List<BitfieldDefinition> Bitfields = new List<BitfieldDefinition>();
//        public static List<ClassDefinition> Classes = new List<ClassDefinition>();
//        public static List<NamespaceDefinition> Namespaces = new List<NamespaceDefinition>();
        public static string t1 = "    ";
        public static string t2 = "        ";
        public static string t3 = "            ";


        public static void Generate(
            string outDir,
            string sourceName,
            List<rNamespace> namespaces,
            List<rSelect> selects,
            List<rStruct> structs)
        {
            var sb = new StringBuilder();
            WriteHeader(sb, namespaces, selects, structs);

            Console.Error.WriteLine(sb.ToString());
            File.WriteAllText(Path.Combine(outDir, sourceName + ".h"), sb.ToString());
            sb = null;


            var sbs = new StringBuilder();
            WriteSourceFile(sbs, sourceName, namespaces, selects, structs);

            Console.Error.WriteLine(sbs.ToString());
            File.WriteAllText(Path.Combine(outDir, sourceName + ".cpp"), sbs.ToString());
            sbs = null;
        }

        private static void WriteHeader(StringBuilder sb,
            List<rNamespace> namespaces,
            List<rSelect> selects,
            List<rStruct> structs)

        {
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  vim: ft=cpp tw=80                                                           ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  DDL Generated code, do not modify directly.                                 ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("#pragma once");
            sb.AppendLine("#include \"Types.h\"");
            sb.AppendLine("#include <string>");
            sb.AppendLine("#include <vector>");
            sb.Append("\n\n");
            WriteNamespaces(sb, namespaces, selects, structs, true);
        }

        private static void WriteSourceFile(StringBuilder sb, string filename,
            List<rNamespace> namespaces,
            List<rSelect> selects,
            List<rStruct> structs)
        {
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  vim: ft=cpp tw=80                                                           ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  DDL Generated code, do not modify directly.                                 ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendFormat("#include \"{0}.h\"\n", filename);
            sb.AppendFormat("#include \"{0}\"\n", "json.hpp");
            sb.AppendFormat("#include <{0}>\n", "assert.h");
            sb.AppendFormat("using json = nlohmann::json;");
            sb.Append("\n\n");
            WriteNamespaces(sb, namespaces, selects, structs, false);
        }


        private static void WriteNamespaces(StringBuilder sb,
            List<rNamespace> Namespaces,
            List<rSelect> Selects,
            List<rStruct> Structs,
            bool header = true)
        {
            if (Namespaces.Count == 0)
                return;

            foreach (var n in Namespaces)
            {
                int nesting = 0;
                sb.AppendFormat("namespace {0}\n", namespace_name_to_c_friendly_name(n.Name, ref nesting));
                sb.AppendLine("{");
                if (header)
                {
                    foreach (var sn in n.Selects)
                    {
                        var s = find_select_by_name(sn, Selects);
                        if (s != null)
                        {
                            sb.AppendFormat(t1 + "enum class {0} : unsigned int\n", s.Name);
                            sb.AppendLine(t1 + "{");
                            foreach (var m in s.Items)
                            {
                                sb.Append(buildSelectField(t2, s.Name, m.Name, m.Value));
                            }

                            sb.AppendLine(t1 + "};");
                        }
                    }

                    foreach (var cn in n.Structs)
                    {
                        var c = find_struct_by_name(cn, Structs);
                        if (c != null)
                            WriteClass(t1, c, sb);
                    }
                }
                else
                {
                    foreach (var cn in n.Structs)
                    {
                        var c = find_struct_by_name(cn, Structs);
                        if (c != null)
                        {
                            CPPGenJsonSerialization.WriteStructJsonSerialization(t1, c, sb, Structs);
                            CPPGenJsonDeserialization.WriteStructJsonDeserialization(t1, c, sb, Structs);
                        }
                    }
                }

                for (int i = 0; i < nesting; ++i)
                    sb.Append("};");
                sb.AppendLine("");
            }
        }

        private static string buildSelectField(string tab, string select, string field, string value)
        {
            if (!string.IsNullOrEmpty(value))
                return string.Format(tab + "{0} = {1},\n", field, value);
            return string.Format(tab + "{0} = {1},\n", field, MurmurHash2.Hash(select + "." + field));
        }


        private static void WriteClass(string tab, rStruct str, StringBuilder sb)
        {
            sb.AppendFormat(tab + "struct {0}\n", str.Name);
            sb.AppendLine(tab + "{");

            foreach (var child in str.Childs)
            {
                if (child != null)
                    WriteClass(tab + t1, child, sb);
            }

            foreach (var field in str.Fields)
            {
                sb.Append(buildStructFieldHeader(tab + t1, field));
            }

            sb.AppendLine();
            sb.AppendFormat(tab + t1 + "static std::string to_json(const {0} * self);\n", str.Name);
            sb.AppendFormat(tab + t1 + "static bool from_json(const std::string & json, {0} * self);\n", str.Name);

            sb.AppendLine(tab + "};");
        }

        private static string buildStructFieldHeader(string tab, rStructField m)
        {
            if (m.ArrayType == EArrayType.SCALAR)
            {
                return string.Format(tab + "{0} {1};\n", Converter.DDLTypeToCPPType(m.Type, m.TypeName), m.Name);
            }

            if (m.ArrayType == EArrayType.DYNAMIC || m.ArrayType == EArrayType.LIST)
            {
                return string.Format(tab + "std::vector<{0}> {1};\n", Converter.DDLTypeToCPPType(m.Type, m.TypeName),
                    m.Name);
            }

            if (m.ArrayType == EArrayType.FIXED)
            {
                return string.Format(tab + "{0} {1}[2];\n", Converter.DDLTypeToCPPType(m.Type, m.TypeName), m.Name,
                    m.Count);
            }

            return "ERROR";
        }


//        private static void write_class_serialization(string tab, rStruct c, StringBuilder sb, string self,
//            List<rStruct> Structs)
//        {
//            foreach (var m in c.Fields)
//            {
//                var child = find_struct_by_name(m.TypeName, Structs);
//                if (Converter.IsPOD(m.Type) || child == null)
//                    sb.AppendFormat(tab + "j[\"{0}\"] = {1}{0};\n", m.Name, self);
////                    sb.AppendLine(tab + "j[\"" + m.Name + "\"] = " + self + m.Name + ",");
//                else
//                {
//                    sb.AppendLine(tab + "{");
//                    sb.AppendFormat(tab + t1 + "json _{0}_{1}",)
//                    sb.AppendLine(tab + "}");
//                    sb.AppendLine(tab + "{\"" + m.Name + "\", {");
//                    write_class_serialization(tab + t1, child, sb, self + m.Name + ".", Structs);
//                    sb.AppendLine(tab + "}},");
//                }
//
//////                sb.AppendFormat(tab + t2 + "{\"{0}\": self->{1} },\n", m.Name, m.Name);
//            }
//        }



//
//        private static void write_class_deserialization(string tab, ClassDefinition c, StringBuilder sb, string self,
//            string j, int depth)
//        {
//            foreach (var m in c.Members)
//            {
//                var child = find_class_by_name(m.Type);
//                if (child == null)
//                {
//                    sb.AppendFormat(tab + "it = {1}.find(\"{0}\");\n", m.Name, j);
//                    sb.AppendFormat(tab + "if (it != {1}.end() && it.value().{0}())\n",
//                        type_to_deserializer_type(m.Type, m.Array), j);
//                    sb.AppendFormat(tab + t1 + self + "{0} = {1}\n", m.Name, type_to_cpp_deserializer_type(m.Type, m.Array));
//                }
//                else
//                {
//                    sb.AppendFormat(tab + "it = {1}.find(\"{0}\");\n", m.Name, j);
//                    sb.AppendFormat(tab + "if (it != {0}.end() && it.value().is_object())\n", j);
//                    sb.AppendLine(tab + "{");
//                    sb.AppendFormat(tab + t1 + "auto {0} = it.value();\n", j + "" + depth);
//                    write_class_deserialization(tab + t1, child, sb, self + m.Name + ".", j + "" + depth, depth + 1);
//                    sb.AppendLine(tab + "}");
//                }
//            }
//        }
//
//
        private static string namespace_name_to_c_friendly_name(string name, ref int nesting)
        {
            var split = name.Split('.');
            nesting = split.Length;

            var result = split[0];
            for (int i = 1; i < split.Length; ++i)
            {
                result += " { " + string.Format("namespace {0}", split[i]);
            }

            return result;
        }


//
//        private static string type_to_cpp_deserializer_type(string type, bool isArray)
//        {
//            if (!isArray)
//            {
//                if (type == "bool")
//                    return "it.value().get<bool>();";
//                else if (type == "int")
//                    return "it.value().get<int>();";
//                else if (type == "float")
//                    return "it.value().get<float>();";
//                else if (type == "string")
//                    return "it.value().get<std::string>();";
//                else if (type_is_select(type))
//                    return string.Format("it.value().get<{0}>();", type);
//                else
//                    return "it.value();";
//            }
//            else
//            {
//                if (type == "bool")
//                    return "it.value().get<std::vector<bool>>();";
//                else if (type == "int")
//                    return "it.value().get<std::vector<int>>();";
//                else if (type == "float")
//                    return "it.value().get<std::vector<float>>();";
//                else if (type == "string")
//                    return "it.value().get<std::vector<std::string>>();";
//                else if (type_is_select(type))
//                    return string.Format("it.value().get<std::vector<{0}>>();", type);
//            }
//
//            return "null";
//        }
//
//        private static string type_to_deserializer_type(string name, bool isArray)
//        {
//            if (isArray)
//                return "is_array";
//
//            if (name == "bool")
//                return "is_boolean";
//            else if (name == "string")
//                return "is_string";
//            else
//                return "is_number";
////            else if (name == "int" || name == "uint")
////            Console.WriteLine("CAN'T HANDLE NAME: " + name);
////            return null;
//        }
//
//        private static string type_to_cpp_type(string name, bool isArray)
//        {
//            if (isArray)
//            {
//                if (name == "string")
//                    return "std::vector<std::string>";
//                if (name == "int")
//                    return "std::vector<int>";
//                if (name == "uint")
//                    return "std::vector<unsigned int>";
//                if (name == "bool")
//                    return "std::vector<bool>";
//                if (name == "float")
//                    return "std::vector<float>";
//                if (type_is_select(name))
//                    return string.Format("std::vector<{0}>", name);
//            }
//            else
//            {
//                if (name == "string")
//                    return "std::string";
//                if (name == "uint")
//                    return "unsigned int";
//                if (type_is_select(name))
//                    return name;
//            }
//
//
//            return name;
//        }
//
//        private static bool type_is_select(string name)
//        {
//            foreach (var s in Selects)
//            {
//                if (s.Name == name)
//                    return true;
//            }
//            return false;
//        }
//
        private static rSelect find_select_by_name(string name, List<rSelect> Selects)
        {
            foreach (var s in Selects)
            {
                if (s.Name == name)
                    return s;
            }

            return null;
        }

        private static rStruct find_struct_by_name(string name, List<rStruct> Structs)
        {
            foreach (var s in Structs)
            {
                if (s.Name == name)
                    return s;
            }

            return null;
        }

        private static rStruct find_struct_by_fullname(string name, List<rStruct> Structs)
        {
            foreach (var s in Structs)
            {
                if (s.CPPFullname == name)
                    return s;
            }

            return null;
        }

//
//        private static ClassDefinition find_class_by_name(string name)
//        {
//            foreach (var c in Classes)
//            {
//                if (c.Name == name)
//                    return c;
//            }
//
//            return null;
//        }
//
//        private static string Hash(string str)
//        {
//            return MurmurHash2.Hash(str).ToString();
//        }
    }
}