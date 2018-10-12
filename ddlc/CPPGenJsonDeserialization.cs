using System.Collections.Generic;
using System.Text;

namespace ddlc
{
    public static class CPPGenJsonDeserialization
    {
        private static string t1 = "    ";
        private static string t2 = "        ";
        private static string t3 = "            ";


        public static void WriteStructJsonDeserialization(string tab, rStruct str, StringBuilder sb,
            List<rStruct> Structs)
        {
            sb.AppendFormat(tab + "bool {0}::from_json(const std::string & json, {0} * self)\n", str.Name);
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab + t1 + "assert(self != nullptr);");
            sb.AppendLine(tab + t1 + "auto j = json::parse(json);");
            sb.AppendLine(tab + t1 + "if (j.empty())");
            sb.AppendLine(tab + t2 + "return false;");
            sb.AppendLine(tab + t1 + "auto it = j.end();");
            sb.AppendLine(Subobject(tab + t1, str, "j", "self->", null, Structs, 0, false));
            sb.AppendLine(tab + t1 + "return true;");
            sb.AppendLine(tab + "}");
        }

        private static string Subobject(string tab, rStruct str, string parentJson, string self,
            rStructField parent, List<rStruct> Structs, int nestLevel, bool nest = true,
            EArrayType array = EArrayType.SCALAR)
        {
            StringBuilder sb = new StringBuilder();
            if (!nest)
            {
                foreach (var f in str.Fields)
                    sb.Append(Field(tab, f, parentJson, self, Structs, nestLevel + 1));
            }
            else
            {
                if (array == EArrayType.SCALAR)
                {
                    string newSelf = string.Format("{0}{1}.", self, parent.Name);
                    string jsi = string.Format("j{0}_it", nestLevel);
                    string jsv = string.Format("j{0}_v", nestLevel);
                    sb.AppendLine(tab + "{");
                    sb.AppendFormat(tab + t1 + "auto {0} = {1}.find(\"{2}\");\n", jsi, parentJson, parent.Name);
                    sb.AppendFormat(tab + t1 + "if ({0} == {1}.end())\n", jsi, parentJson);
                    sb.AppendFormat(tab + t1 + t1 + "return false;\n");
                    sb.AppendFormat(tab + t1 + "auto {0} = {1}.value();\n", jsv, jsi);
                    sb.AppendLine(tab + t1 + "{");
                    foreach (var f in str.Fields)
                        sb.Append(Field(tab + t2, f, jsv, newSelf, Structs, nestLevel + 1));
                    sb.AppendLine(tab + t1 + "}");
                    sb.AppendLine(tab + "}");
                }
                else if (array == EArrayType.DYNAMIC || array == EArrayType.FIXED || array == EArrayType.LIST)
                {
                    string itr = string.Format("i{0}", nestLevel);
                    string len = string.Format("len{0}", nestLevel);
                    string jsi = string.Format("j{0}", nestLevel);
                    string jsv = string.Format("j{0}v", nestLevel);
                    string jsval = string.Format("j{0}_val", nestLevel);
                    sb.AppendLine(tab + "{");
                    sb.AppendFormat(tab + t1 + "auto {0} = {2}.find(\"{1}\");\n", jsi, parent.Name, parentJson);
                    sb.AppendFormat(tab + t1 + "if ({0} == {1}.end())\n", jsi, parentJson);
                    sb.AppendLine(tab + t1 + t1 + "return false;");
                    sb.AppendFormat(tab + t1 + "auto {0} = {1}.value();\n", jsv, jsi);
                    sb.AppendFormat(tab + t1 + "const size_t {0} = {1}.size();\n", len, jsv);
                    sb.AppendFormat(tab + t1 + "{0}{1}.resize({2});\n", self, parent.Name, len);
                    sb.AppendFormat(tab + t1 + "for (size_t {0} = 0; {0} < {1}; ++{0})\n", itr, len);
                    sb.AppendLine(tab + t1 + "{");
                    
                    sb.AppendFormat(tab + t2 + "auto {0} = {1}[{2}];\n", jsval, jsv, itr);
                    string itrSelf = string.Format("{0}{1}[{2}].", self, parent.Name, itr);
                    foreach (var f in str.Fields)
                        sb.Append(Field(tab + t2, f, jsval, itrSelf, Structs, nestLevel + 1));
                    
                    sb.AppendLine(tab + t1 + "}");
                    sb.AppendLine(tab + "}");
                }
            }

            return sb.ToString();
        }

        private static string Field(string tab, rStructField m, string parentJson,
            string self,
            List<rStruct> Structs, int nestLevel)
        {
            StringBuilder sb = new StringBuilder();
            if (Converter.IsPOD(m.Type))
            {
                sb.AppendFormat(tab + "it = {1}.find(\"{0}\");\n", m.Name, parentJson);
                sb.AppendFormat(tab + "if (it != {1}.end() && it.value().{0})\n", field_json_deserializer_type(m), parentJson);
                sb.AppendFormat(tab + t1 + "{0}{1} = it.value().{2}();\n", self, m.Name, field_to_cpp_deserializer_type(m));
            }
            else
            {
                if (m.Type == EType.VECTOR2)
                {
                }
                if (m.Type == EType.VECTOR3)
                {
                }
                if (m.Type == EType.VECTOR4)
                {
                }
                if (m.Type == EType.Quaternion)
                {
                }
                

                if (m.ArrayType == EArrayType.SCALAR)
                {
                    foreach (var st in Structs)
                    {
                        if (st.Name == m.TypeName)
                        {
                            var o1 = Subobject(tab, st, parentJson, self, m, Structs, nestLevel + 1);
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
                            var o1 = Subobject(tab, st, parentJson, self, m, Structs, nestLevel + 1, true, m.ArrayType);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private static string field_json_deserializer_type(rStructField f)
        {
            var t = f.Type;
            if (f.ArrayType == EArrayType.SCALAR)
            {
                switch (t)
                {
                    case EType.UINT8:
                    case EType.UINT16:
                    case EType.UINT32:
                    case EType.UINT64:
                    case EType.INT8:
                    case EType.INT16:
                    case EType.INT32:
                    case EType.INT64:
                    case EType.FLOAT32:
                    case EType.FLOAT64:
                    case EType.SELECT:
                    case EType.BITFIELD:
                        return "is_number()";
                    case EType.STRING:
                        return "is_string()";
                    case EType.BOOLEAN:
                        return "is_boolean()";
                    case EType.STRUCT:
                        return "is_object()";
                }
            }
            else if (f.ArrayType == EArrayType.DYNAMIC || f.ArrayType == EArrayType.FIXED || f.ArrayType == EArrayType.LIST)
                return "is_array()";
            return "ERROR";
        }

        private static string field_to_cpp_deserializer_type(rStructField f)
        {
            var t = f.Type;
            if (f.ArrayType == EArrayType.SCALAR)
            {
                switch (t)
                {
                    case EType.UINT8:
                        return "get<u8>";
                    case EType.UINT16:
                        return "get<u16>";
                    case EType.UINT32:
                        return "get<u32>";
                    case EType.UINT64:
                        return "get<u64>";
                    case EType.INT8:
                        return "get<i8>";
                    case EType.INT16:
                        return "get<i16>";
                    case EType.INT32:
                        return "get<i32>";
                    case EType.INT64:
                        return "get<i64>";
                    case EType.FLOAT32:
                        return "get<f32>";
                    case EType.FLOAT64:
                        return "get<f64>";
                    case EType.SELECT:
                        return string.Format("get<{0}>", f.TypeName);
                    case EType.BITFIELD:
                        return string.Format("get<{0}>", f.TypeName);
                    case EType.STRING:
                        return "get<std::string>";
                    case EType.BOOLEAN:
                        return "get<bool>";
                }
            }
            else if (f.ArrayType == EArrayType.DYNAMIC || f.ArrayType == EArrayType.FIXED || f.ArrayType == EArrayType.LIST)
            {
                switch (t)
                {
                    case EType.UINT8:
                        return "get<std::vector<u8>>";
                    case EType.UINT16:
                        return "get<std::vector<u16>>";
                    case EType.UINT32:
                        return "get<std::vector<u32>>";
                    case EType.UINT64:
                        return "get<std::vector<u64>>";
                    case EType.INT8:
                        return "get<std::vector<i8>>";
                    case EType.INT16:
                        return "get<std::vector<i16>>";
                    case EType.INT32:
                        return "get<std::vector<i32>>";
                    case EType.INT64:
                        return "get<std::vector<i64>>";
                    case EType.FLOAT32:
                        return "get<std::vector<f32>>";
                    case EType.FLOAT64:
                        return "get<std::vector<f64>>";
                    case EType.SELECT:
                        return string.Format("get<std::vector<{0}>>", f.TypeName);
                    case EType.BITFIELD:
                        return string.Format("get<std::vector<{0}>>", f.TypeName);
                    case EType.STRING:
                        return "get<std::vector<std::string>>";
                    case EType.BOOLEAN:
                        return "get<std::vector<bool>>";
                }
            }
            return "ERROR";
        }
    }
}