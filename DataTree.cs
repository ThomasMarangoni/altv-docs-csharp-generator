namespace altv_docs_csharp_generator;

/*
public class Namespace
{
    public List<Class> Classes = new List<Class>();
    public string Name;
    public bool IsPinned;
}
*/

public class Class
{
    public List<Method> Methods = new List<Method>();
    public List<Property> Properties = new List<Property>();
    public List<Field> Fields = new List<Field>();
    public List<Enum> Enums = new List<Enum>();
    public string Name;
    public string Namespace;
    public string Parent;
    public string Child;
    public bool IsPinned;
}

public class Method
{
    public string Name;
    public List<Parameter> Parameters = new List<Parameter>();
    public string ReturnType;
    public bool IsStatic;
    public bool IsOverload;
}

public class Parameter
{
    public string Type;
    public string Name;
    public bool IsOptional;
    public bool IsOut;
}

public class Property
{
    public string Type;
    public string Name;
    public bool IsGetter;
    public bool IsSetter;
}

public class Field
{
    public string Type;
    public string Name;
    public bool IsStatic;
}

public class Enum
{
    public string Type;
    public string Name;
}