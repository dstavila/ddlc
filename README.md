# ddlc
Project idea is very simple, I needed DDL language and I had almost no time to do it properly. So I'm using C# code parsing to process DDL files.

This project does couple things:
* [Select] on enums - generates hash value for field
* You can define classes - ddlc will generate C++ to_json and from_json functions for that class (for C# I'm not generating any parser stuff)

NOTE: currently I'm using nlohmann json library (https://github.com/nlohmann/json) for C++ to_json/from_json functions.


# NOTE
It's highly WIP, use at your own risk.


# Examples
### [Select]
```
/// DDL
[Select]
public enum ETesto
{
  kUnknown = 0, // If any value is assigned in select, I'm not doing anything interesting when generating Unity or C++ code
  kTesto, // In this case I'm using murmur hash of "ETesto.kTesto" as a value in output enum
}

// C#
public enum ETesto : uint
{
  kUnknown = 0,
  kTesto = 1223037801, // note, that's not actual value, just an example
}
```

### DDL -> C++ Json
```
/// DDL
public class Testo
{
  int Id;
  bool Flag;
}

/// DDL_Generated.h
struct Testo
{
  i32 Id;
  bool Flag;
  
   static std::string to_json(const Testo * self);
   static bool from_json(const std::string & json, Testo * self);
}

/// DDL_Generated.cpp
#include "json.hpp"
#include <assert.h>
using json = nlohmann::json;
std::string Testo::to_json(const Testo * self)
{
    assert(self != nullptr);
    json root;
    root["Id"] = self->Id;
    root["Flag"] = self->Flag;

    return root.dump();
}
bool Testo::from_json(const std::string & json, Testo * self)
{
    assert(self != nullptr);
    auto j = json::parse(json);
    if (j.empty())
        return false;
    auto it = j.end();
    it = j.find("Id");
    if (it != j.end() && it.value().is_number())
        self->Id = it.value().get<i32>();
    it = j.find("Flag");
    if (it != j.end() && it.value().is_boolean())
        self->Flag = it.value().get<bool>();

    return true;
}
```



Inspired by Mike Acton's DDLParser library (https://github.com/macton/DDLParser).
