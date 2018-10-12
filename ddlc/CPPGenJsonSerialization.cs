﻿using System.Collections.Generic;
using System.Text;

namespace ddlc
{
    public static class CPPGenJsonSerialization
    {
        private static string t1 = "    ";
        private static string t2 = "        ";
        private static string t3 = "            ";
        
        
        public static void WriteStructJsonSerialization(string tab, rStruct str, StringBuilder sb, List<rStruct> Structs)
        {
            sb.AppendFormat(tab + "std::string {1}::to_json(const {0} * self)\n", str.Name, str.Name);
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab + t1 + "assert(self != nullptr);");
            sb.AppendLine(buildSubobjectSerialization(tab + t1, str, "root", "self->", null, Structs, 0, false));
            sb.AppendLine(tab + t1 + "return root.dump();");
            sb.AppendLine(tab + "}");
        }
        
        
        private static string buildSubobjectSerialization(string tab, rStruct str, string parentJson, string self,
            rStructField parent, List<rStruct> Structs, int nestLevel, bool nest = true,
            EArrayType array = EArrayType.SCALAR)
        {
            StringBuilder sb = new StringBuilder();
            if (!nest)
            {
                sb.AppendFormat(tab + "json {0};\n", parentJson);
                foreach (var f in str.Fields)
                    sb.Append(buildStructFieldJsonSerialization(tab, f, parentJson, self, Structs, nestLevel + 1));
            }
            else
            {
                if (array == EArrayType.SCALAR)
                {
                    string jsonName = string.Format("_{0}_{1}", parentJson, parent.Name);
                    string newSelf = string.Format("{0}{1}.", self, parent.Name);
                    sb.AppendLine(tab + "{");
                    sb.AppendFormat(tab + t1 + "json {0};\n", jsonName);
                    foreach (var f in str.Fields)
                        sb.Append(buildStructFieldJsonSerialization(tab + t1, f, jsonName, newSelf, Structs,
                            nestLevel + 1));
                    sb.AppendFormat(tab + t1 + "{0}[\"{1}\"] = {2};\n", parentJson, parent.Name, jsonName);
                    sb.AppendLine(tab + "}");
                }
                else if (array == EArrayType.DYNAMIC || array == EArrayType.FIXED || array == EArrayType.LIST)
                {
                    string jsonName = string.Format("_{0}_{1}", parentJson, parent.Name);
                    string itrName = string.Format("{0}_tmp", jsonName);
                    string itr = string.Format("i{0}", nestLevel);
                    string len = string.Format("len{0}", nestLevel);
                    string lenValue = array == EArrayType.FIXED
                        ? parent.Count.ToString()
                        : string.Format("{0}{1}.size()", self, parent.Name);
                    sb.AppendLine(tab + "{");
                    sb.AppendFormat(tab + t1 + "json {0};\n", jsonName);
                    sb.AppendFormat(tab + t1 + "for (size_t {0} = 0; {0} < {1}; ++{0})\n", itr, lenValue);
                    sb.AppendLine(tab + t1 + "{");
                    sb.AppendFormat(tab + t2 + "json {0};\n", itrName);

                    string itrSelf = string.Format("{0}{1}[{2}].", self, parent.Name, itr);
                    foreach (var f in str.Fields)
                        sb.Append(buildStructFieldJsonSerialization(tab + t2, f, itrName, itrSelf, Structs, nestLevel + 1));
                    
                    sb.AppendFormat(tab + t2 + "{0}.push_back({1});\n", jsonName, itrName);
                    sb.AppendLine(tab + t1 + "}");
                    sb.AppendFormat(tab + t1 + "{0}[\"{1}\"] = {2};\n", parentJson, parent.Name, jsonName);
                    sb.AppendLine(tab + "}");
                }
            }

            return sb.ToString();
        }
        
        
        private static string buildStructFieldJsonSerialization(string tab, rStructField m, string obj, string self,
            List<rStruct> Structs, int nestLevel)
        {
            if (Converter.IsPOD(m.Type))
            {
                return string.Format(tab + "{2}[\"{0}\"] = {1}{0};\n", m.Name, self, obj);
            }
            else
            {
                if (m.ArrayType == EArrayType.SCALAR)
                {
                    foreach (var st in Structs)
                    {
                        if (st.Name == m.TypeName)
                        {
                            var o1 = buildSubobjectSerialization(tab, st, obj, self, m, Structs, nestLevel + 1);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
                else if (m.ArrayType == EArrayType.DYNAMIC || m.ArrayType == EArrayType.FIXED || m.ArrayType == EArrayType.LIST)
                {
                    foreach (var st in Structs)
                    {
                        if (st.Name == m.TypeName)
                        {
                            var o1 = buildSubobjectSerialization(tab, st, obj, self, m, Structs, nestLevel + 1,
                                true,
                                m.ArrayType);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
            }
            return "ERROR";
        }
    }
}