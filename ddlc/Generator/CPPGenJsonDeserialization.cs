using System.Collections.Generic;
using System.Text;

namespace ddlc
{
    public static class CPPGenJsonDeserialization
    {
        private static string t1 = "    ";
        private static string t2 = "        ";


        public static void WriteStructJsonDeserialization(string tab, StructDecl aggregateDecl, List<StructDecl> Structs, StringBuilder sb)
        {
            var nspace = Utils.BuildNamespace(aggregateDecl);
            sb.AppendFormat(tab + "bool {0}::from_json(const std::string & json, {0} * self)\n", nspace);
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab + t1 + "assert(self != nullptr);");
            sb.AppendLine(tab + t1 + "auto j = json::parse(json);");
            sb.AppendLine(tab + t1 + "if (j.empty())");
            sb.AppendLine(tab + t2 + "return false;");
            sb.AppendLine(tab + t1 + "auto it = j.end();");
            sb.AppendLine(Subobject(tab + t1, aggregateDecl, "j", "self->", null, Structs, 0, false));
            sb.AppendLine(tab + t1 + "return true;");
            sb.AppendLine(tab + "}");
        }
        public static void WriteStructJsonDeserialization(string tab, ClassDecl decl, List<ClassDecl> Structs, StringBuilder sb)
        {
            var nspace = Utils.BuildNamespace(decl);
            sb.AppendFormat(tab + "bool {0}::from_json(const std::string & json, {0} * self)\n", nspace);
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab + t1 + "assert(self != nullptr);");
            sb.AppendLine(tab + t1 + "auto j = json::parse(json);");
            sb.AppendLine(tab + t1 + "if (j.empty())");
            sb.AppendLine(tab + t2 + "return false;");
            sb.AppendLine(tab + t1 + "auto it = j.end();");
            sb.AppendLine(Subobject(tab + t1, decl, "j", "self->", null, Structs, 0, false));
            sb.AppendLine(tab + t1 + "return true;");
            sb.AppendLine(tab + "}");
        }

        private static string Subobject(string tab, StructDecl aggregateDecl, string parentJson, string self,
            AggregateField parent, List<StructDecl> Structs, int nestLevel, bool nest = true,
            EArrayType array = EArrayType.SCALAR)
        {
            StringBuilder sb = new StringBuilder();
            if (!nest)
            {
                foreach (var f in aggregateDecl.Fields)
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
                    foreach (var f in aggregateDecl.Fields)
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
                    foreach (var f in aggregateDecl.Fields)
                        sb.Append(Field(tab + t2, f, jsval, itrSelf, Structs, nestLevel + 1));
                    
                    sb.AppendLine(tab + t1 + "}");
                    sb.AppendLine(tab + "}");
                }
            }

            return sb.ToString();
        }
        private static string Subobject(string tab, ClassDecl decl, string parentJson, string self,
            AggregateField parent, List<ClassDecl> Structs, int nestLevel, bool nest = true,
            EArrayType array = EArrayType.SCALAR)
        {
            StringBuilder sb = new StringBuilder();
            if (!nest)
            {
                foreach (var f in decl.Fields)
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
                    foreach (var f in decl.Fields)
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
                    foreach (var f in decl.Fields)
                        sb.Append(Field(tab + t2, f, jsval, itrSelf, Structs, nestLevel + 1));
                    
                    sb.AppendLine(tab + t1 + "}");
                    sb.AppendLine(tab + "}");
                }
            }

            return sb.ToString();
        }

        private static string Field(string tab, AggregateField m, string parentJson,
            string self,
            List<StructDecl> Structs, int nestLevel)
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
                        if (st.Name == m.sType)
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
                        if (st.Name == m.sType)
                        {
                            var o1 = Subobject(tab, st, parentJson, self, m, Structs, nestLevel + 1, true, m.ArrayType);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
            }
            return sb.ToString();
        }
        private static string Field(string tab, AggregateField m, string parentJson,
            string self,
            List<ClassDecl> Clases, int nestLevel)
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
                    foreach (var st in Clases)
                    {
                        if (st.Name == m.sType)
                        {
                            var o1 = Subobject(tab, st, parentJson, self, m, Clases, nestLevel + 1);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
                else if (m.ArrayType == EArrayType.DYNAMIC || m.ArrayType == EArrayType.FIXED || m.ArrayType == EArrayType.LIST)
                {
                    foreach (var st in Clases)
                    {
                        if (st.Name == m.sType)
                        {
                            var o1 = Subobject(tab, st, parentJson, self, m, Clases, nestLevel + 1, true, m.ArrayType);
                            return string.Format("{0}\n", o1);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private static string field_json_deserializer_type(AggregateField f)
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


        private static string field_to_cpp_deserializer_type(AggregateField f)
        {
            var t = f.Type;
            if (f.ArrayType == EArrayType.SCALAR)
            {
                switch (t)
                {
                    case EType.UINT8:
                        return "get<uint8_t>";
                    case EType.UINT16:
                        return "get<uint16_t>";
                    case EType.UINT32:
                        return "get<uint32_t>";
                    case EType.UINT64:
                        return "get<uint64_t>";
                    case EType.INT8:
                        return "get<int8_t>";
                    case EType.INT16:
                        return "get<int16_t>";
                    case EType.INT32:
                        return "get<int32_t>";
                    case EType.INT64:
                        return "get<int64_t>";
                    case EType.FLOAT32:
                        return "get<float>";
                    case EType.FLOAT64:
                        return "get<double>";
                    case EType.SELECT:
                        return string.Format("get<{0}>", Utils.TypeWithNamespace(f));
                    case EType.BITFIELD:
                        return string.Format("get<{0}>", Utils.TypeWithNamespace(f));
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
                        return "get<std::vector<uint8_t>>";
                    case EType.UINT16:
                        return "get<std::vector<uint16_t>>";
                    case EType.UINT32:
                        return "get<std::vector<uint32_t>>";
                    case EType.UINT64:
                        return "get<std::vector<uint64_t>>";
                    case EType.INT8:
                        return "get<std::vector<int8_t>>";
                    case EType.INT16:
                        return "get<std::vector<int16_t>>";
                    case EType.INT32:
                        return "get<std::vector<int32_t>>";
                    case EType.INT64:
                        return "get<std::vector<int64_t>>";
                    case EType.FLOAT32:
                        return "get<std::vector<float>>";
                    case EType.FLOAT64:
                        return "get<std::vector<double>>";
                    case EType.SELECT:
                        return string.Format("get<std::vector<{0}>>", f.sType);
                    case EType.BITFIELD:
                        return string.Format("get<std::vector<{0}>>", f.sType);
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