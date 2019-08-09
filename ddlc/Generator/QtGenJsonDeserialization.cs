using System.Collections.Generic;
using System.Text;

namespace ddlc
{
    public static class QtGenJsonDeserialization
    {
        public static void WriteStructJsonDeserialization(
            StructDecl aggregateDecl, 
            List<StructDecl> Structs, 
            TabbedStringBuilder sb)
        {
            var nspace = Utils.BuildNamespace(aggregateDecl);
            sb.WriteLine($"bool {nspace}::FromJson(const QByteArray& json)");
            sb.WriteLine("{");
            sb.PushTab();
            sb.WriteLine("if (json.size() == 0) return false;");
            sb.WriteLine("QJsonDocument doc = QJsonDocument::fromJson(json);");
            sb.WriteLine("if (!doc.isObject()) return false;");
            sb.WriteLine("QJsonObject object = doc.object();");
            sb.WriteLine("FromJsonObject(object);");
            sb.WriteLine("return true;");
            sb.PopTab();
            sb.WriteLine("}");
            sb.WriteLine("");
            
            
            sb.WriteLine($"void {nspace}::FromJsonObject(const QJsonObject& object)");
            sb.WriteLine("{");
            sb.PushTab();
            foreach (var f in aggregateDecl.Fields) {
                write_member(f, sb);
            }
            sb.PopTab();
            sb.WriteLine("}");
            sb.WriteLine("");
        }
        public static void WriteStructJsonDeserialization(ClassDecl decl, List<ClassDecl> Structs, TabbedStringBuilder sb)
        {
           var nspace = Utils.BuildNamespace(decl);
            sb.WriteLine($"bool {nspace}::FromJson(const QByteArray& json)");
            sb.WriteLine("{");
            sb.PushTab();
            sb.WriteLine("if (json.size() == 0) return false;");
            sb.WriteLine("QJsonDocument doc = QJsonDocument::fromJson(json);");
            sb.WriteLine("if (!doc.isObject()) return false;");
            sb.WriteLine("QJsonObject object = doc.object();");
            sb.WriteLine("FromJsonObject(object);");
            sb.WriteLine("return true;");
            sb.PopTab();
            sb.WriteLine("}");
            sb.WriteLine("");
            
            
            sb.WriteLine($"void {nspace}::FromJsonObject(const QJsonObject& object)");
            sb.WriteLine("{");
            sb.PushTab();
            foreach (var f in decl.Fields) {
                write_member(f, sb);
            }
            sb.PopTab();
            sb.WriteLine("}");
            sb.WriteLine("");
        }

        private static void write_member(AggregateField f, TabbedStringBuilder sb)
        {
            if (f.ArrayType == EArrayType.SCALAR)
            {
                if (Converter.IsPOD(f.Type))
                {
                    sb.WriteLine($"if (object.contains(\"{f.Name}\"))");
                    sb.WriteNestedLine($"{f.Name} = {assign_member(f, $"object[\"{f.Name}\"]")};");
                }
                else
                {
                    sb.WriteLine($"{f.Name}.FromJsonObject(object);");
                }
            }
            else
            {
                var arrayName = $"{f.Name}Array";
                sb.WriteLine($"QJsonArray {arrayName} = object[\"{f.Name}\"].toArray();");
                sb.WriteLine($"{f.Name}.reserve({arrayName}.size());");
                sb.WriteLine($"for(int i = 0; i < {arrayName}.size(); ++i)");
                sb.WriteLine("{");
                sb.PushTab();
                sb.WriteLine($"QJsonValue elem = {arrayName}.at(i);");
                if (Converter.IsPOD(f.Type))
                {
                    sb.WriteLine($"{f.Name}.append({assign_member(f, "elem")});");
                }
                else
                {
                    sb.WriteLine($"if (!elem.isObject()) continue;");
                    sb.WriteLine($"{Utils.TypeWithNamespace(f)} innerObject;");
                    sb.WriteLine($"innerObject.FromJsonObject(elem.toObject());");
                    sb.WriteLine($"{f.Name}.append(std::move(innerObject));");
                }
                sb.PopTab();
                sb.WriteLine("}");
            }
        }
        
        private static string assign_member(AggregateField f, string objectName)
        {
            if (f.Type == EType.UINT8)
                return $"static_cast<uint8_t>({objectName}.toInt())";
            if (f.Type == EType.UINT16)
                return $"static_cast<uint16_t>({objectName}toInt())";
            if (f.Type == EType.UINT32)
                return $"static_cast<uint32_t>({objectName}.toInt())";
            if (f.Type == EType.UINT64)
                return $"static_cast<uint64_t>({objectName}.toVariant().toLongLong())";
            if (f.Type == EType.INT8)
                return $"static_cast<int8_t>({objectName}.toInt())";
            if (f.Type == EType.INT16)
                return $"static_cast<int16_t>({objectName}.toInt())";
            if (f.Type == EType.INT32)
                return $"static_cast<int32_t>({objectName}.toInt())";
            if (f.Type == EType.INT64)
                return $"static_cast<int64_t>({objectName}.toVariant().toLongLong())";
            if (f.Type == EType.FLOAT32)
                return $"static_cast<float>({objectName}.toDouble())";
            if (f.Type == EType.FLOAT64)
                return $"static_cast<double>({objectName}.toDouble())";
            if (f.Type == EType.BOOLEAN)
                return $"{objectName}.toBool()";
            if (f.Type == EType.STRING)
                return $"{objectName}.toString()";
            if (f.Type == EType.SELECT)
                return $"static_cast<{Utils.TypeWithNamespace(f)}>({objectName}.toInt())";
            if (f.Type == EType.BITFIELD)
                return $"static_cast<{Utils.TypeWithNamespace(f)}>({objectName}.toInt())";
            return "ERROR";
        }
    }
}