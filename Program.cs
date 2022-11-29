#define DEBUG

using System.Reflection;

namespace altv_docs_csharp_generator;

public static class Program
{
  private const string ServerPath = "docs/api/server";

  private static readonly Dictionary<string, string> SimpleTypeConversionDict = new Dictionary<string, string>()
  {
    /*
     * Definition of Numeric types
     */
    { "System.SByte", "sbyte" },
    { "System.SByte&", "sbyte&" },
    { "System.SByte[]", "sbyte[]" },
    { "System.SByte[]&", "sbyte[]&"},
    { "System.Byte", "byte" },
    { "System.Byte&", "byte&" },
    { "System.Byte[]", "byte[]" },
    { "System.Byte[]&", "byte[]&" },
    { "System.Int16", "short" },
    { "System.Int16&", "short&" },
    { "System.Int16[]", "short[]" },
    { "System.Int16[]&", "short[]&" },
    { "System.UInt16", "ushort" },
    { "System.UInt16&", "ushort&"},
    { "System.UInt16[]", "ushort[]" },
    { "System.UInt16[]&", "ushort[]&"},
    { "System.Int32", "int" },
    { "System.Int32&", "int&" },
    { "System.Int32[]", "int[]" },
    { "System.Int32[]&", "int[]&" },
    { "System.UInt32", "uint" },
    { "System.UInt32&", "uint&" },
    { "System.UInt32[]", "uint[]" },
    { "System.UInt32[]&", "uint[]&" },
    { "System.Int64", "long" },
    { "System.Int64&", "long&" },
    { "System.Int64[]", "long[]" },
    { "System.Int64[]&", "long[]&" },
    { "System.UInt64", "ulong" },
    { "System.UInt64&", "ulong&" },
    { "System.UInt64[]", "ulong[]" },
    { "System.UInt64[]&", "ulong[]&" },
    { "System.Single", "float"},
    { "System.Single&", "float&"},
    { "System.Single[]", "float[]"},
    { "System.Single[]&", "float[]&"},
    { "System.Double", "double"},
    { "System.Double&", "double&"},
    { "System.Double[]", "double[]"},
    { "System.Double[]&", "double[]&"},
    
    /*
     * Definitions of Vector types
     */
    { "System.Numerics.Vector2", "Vector2" },
    { "System.Numerics.Vector2[]", "Vector2[]" },
    { "System.Numerics.Vector3", "Vector3" },
    { "System.Numerics.Vector3[]", "Vector3[]" },
    { "System.Numerics.Vector4", "Vector4" },
    { "System.Numerics.Vector4[]", "Vector4[]" },
    
    /*
     * Definition of other types
     */
    { "System.Void", "void" },
    { "System.String", "string" },
    { "System.Boolean", "bool" },
    { "System.Boolean&", "bool&" },
    { "System.Boolean[]", "bool[]" },
    { "System.Boolean[]&", "bool[]&" },
    { "System.Object", "object"},
    { "System.Object&", "object&"},
    { "System.Object[]", "object[]"},
    { "System.Object[]&", "object[]&"},
  };

  private static string GetConvertedType(string? fullName, string? name)
  {
    if (fullName == null && name == null)
      return "";

    if (fullName == null)
      return name ?? "";

    return SimpleTypeConversionDict.ContainsKey(fullName) ? SimpleTypeConversionDict[fullName] : fullName;
  }

  private static IEnumerable<Class> GenerateDataTree(Assembly assembly)
  {
    var dataTree = new List<Class>();
    
    foreach (var classType in assembly.GetTypes()
               .Where(x => x.IsPublic && x.IsClass && !x.IsSpecialName &&
                           x.Module.ToString().Contains("AltV.Net.")))
    {
      var classObj = new Class
      {
        Name = classType.Name,
        Namespace = classType.Namespace ?? ""
      };

#if DEBUG
      Console.WriteLine(classObj.Namespace + " " + classObj.Name);
#endif

      foreach (var methodInfo in classType.GetMethods().Where(x => x.IsPublic && !x.IsSpecialName))
      {
        var methodObj = new Method
        {
          Name = methodInfo.Name,
          IsStatic = methodInfo.IsStatic,
          IsOverload = classObj.Methods.Exists(x => x.Name == methodInfo.Name),
          ReturnType = GetConvertedType(methodInfo.ReturnType.FullName, methodInfo.ReturnType.Name)
        };

#if DEBUG
        Console.Write($"\t{(methodObj.IsStatic ? "static " : "")}{methodObj.ReturnType} {methodObj.Name}(");
#endif

        foreach (var parameterInfo in methodInfo.GetParameters())
        {
          var parameterObj = new Parameter
          {
            Name = parameterInfo.Name ?? "",
            Type = GetConvertedType(parameterInfo.ParameterType.FullName, parameterInfo.ParameterType.Name),
            IsOptional = parameterInfo.IsOptional,
            IsOut = parameterInfo.IsOut
          };

#if DEBUG
          Console.Write(
            $"{(parameterObj.IsOptional ? "optional " : "")}" +
            $"{(parameterObj.IsOut ? "out " : "")}{parameterObj.Type} " +
            $"{parameterObj.Name}{((parameterInfo.Position == methodInfo.GetParameters().Length - 1) ? "" : ", ")}"
            );
#endif
          methodObj.Parameters.Add(parameterObj);
        }

#if DEBUG
        Console.WriteLine(")");
#endif
        
        classObj.Methods.Add(methodObj);
      }

      foreach (var propertyInfo in classType.GetProperties().Where(x => !x.IsSpecialName))
      {
        var propertyObj = new Property
        {
          Name = propertyInfo.Name,
          Type = GetConvertedType(propertyInfo.PropertyType.FullName, propertyInfo.PropertyType.Name),
          IsGetter = propertyInfo.CanRead,
          IsSetter = propertyInfo.CanWrite
        };

#if DEBUG
        Console.WriteLine(
          $"\t{propertyObj.Type} {propertyObj.Name} {{{(propertyObj.IsGetter ? " get;" : "")} " +
          $"{(propertyObj.IsSetter ? "set; " : "")}}}"
          );
#endif

        classObj.Properties.Add(propertyObj);
      }

      foreach (var fieldInfo in classType.GetFields().Where(x => x.IsPublic && !x.IsSpecialName))
      {
        var fieldObj = new Field
        {
          Name = fieldInfo.Name,
          Type = GetConvertedType(fieldInfo.FieldType.FullName, fieldInfo.FieldType.Name),
          IsStatic = fieldInfo.IsStatic
        };

#if DEBUG
        Console.WriteLine($"\t{(fieldObj.IsStatic ? "static " : "")}{fieldObj.Type} {fieldObj.Name}");
#endif

        classObj.Fields.Add(fieldObj);
      }

      dataTree.Add(classObj);
    }

    return dataTree;
  }
  
 /*
  * TODO:
  * - Simple names for types
  * - Show Namespace and class at top
  * - "Pinning" Classes in docs
  * - Overloaded methods in one file
  * - Only show properties and method that aren't inherited and link inherited ones to correct page
  * - Show static for methods and properties
  * - Handle Enums
  * - Handle Class fields
  * - Namespace Tree as index page per namespace
  */

  public static int Main()
  {
    var assembly = Assembly.LoadFrom("data/server/AltV.Net.dll");
    Console.WriteLine("Loaded AltV.Net.dll");

    if (!Directory.Exists(ServerPath))
    {
      Directory.CreateDirectory(ServerPath);
    }
    else
    {
      Directory.Delete(ServerPath, true);
      Directory.CreateDirectory(ServerPath);
    }

    var dataTree = GenerateDataTree(assembly);


    Environment.Exit(0);
    
    // --------------- Refactor from here ---------------

    var indexText = "";
    foreach (var type in assembly.GetTypes()
               .Where(x => x.IsPublic && !x.IsSpecialName && x.Module.ToString().Contains("AltV.Net.")))
    {
      var namespaceString = type.Namespace?.Replace(".", "_");
      var dir = ServerPath + "/" + namespaceString + "/" + type.Name;
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      // Create index page with constructor information
      var classFileName = dir + "/index.md";
      var classFile = File.CreateText(classFileName);
      var classText = $@"---
title: {type.Name}
order: 0
---

# {{{{ $frontmatter.title }}}}

## Properties
";
      foreach (var property in type.GetProperties().Where(x =>
                 (x.GetMethod?.IsPublic == true || x.SetMethod?.IsPublic == true)
                 && !x.IsSpecialName))
      {
        classText += $"- [{property.Name}]({property.Name})\n";
      }

      classText += $@"

## Methods
";

      foreach (var method in type.GetMethods().Where(x => x.IsPublic && !x.IsSpecialName))
      {
        classText += $"- [{method.Name}]({method.Name})\n";
      }

      classFile.Write(classText);
      classFile.Flush();
      classFile.Close();

      var linkString = namespaceString + "/" + type.Name + "/index.html";
      indexText += $"- <a href=\"{linkString}\" target=\"_self\" rel=\"noreferrer\">{type.Namespace}.{type.Name}</a>\n";
      Console.WriteLine(classFileName);

      // create page for each property per class
      foreach (var property in type.GetProperties().Where(x =>
                 (x.GetMethod?.IsPublic == true || x.SetMethod?.IsPublic == true)
                 && !x.IsSpecialName))
      {
        var propertyString = property.PropertyType + " " + property.Name + " { ";
        if (property.GetMethod?.IsPublic == true)
          propertyString += "get; ";
        if (property.SetMethod?.IsPublic == true)
          propertyString += "set; ";

        propertyString += "}";

        var fileName = dir + "/" + property.Name + ".md";

        var file = File.CreateText(fileName);

        var text = $@"---
title: {property.Name}
order: 0
---

# {{{{ $frontmatter.title }}}}

<!--@include: ./{property.Name}_partial_header.md-->

## Function Definition

```c#
{propertyString}
```

## Documentation

<!--@include: ./{property.Name}_partial_footer.md-->
";

        file.Write(text);
        file.Flush();
        file.Close();
      }

      // create page for each method per class
      foreach (var method in type.GetMethods().Where(x => x.IsPublic && !x.IsSpecialName))
      {
        var paramString = "";
        var paramList = "";
        foreach (var param in method.GetParameters().OrderBy(x => x.Position))
        {
          paramString += param.ParameterType.FullName + " " + param.Name;

          // format optional parameter value
          if (param.IsOptional)
          {
            var value = param.DefaultValue;
            if (value == null)
              paramString += " = null";
            else
              paramString += " = " + (value.ToString()!.Length == 0 ? "\"\"" : value.ToString());
          }

          paramString += ", ";
          paramList += "* " + param.ParameterType.FullName + " " + param.Name + "\n";
        }

        // remove last , if the method has parameters
        if (method.GetParameters().Length > 0)
          paramString = paramString[..^2];

        var methodString = method.ReturnParameter + " " + type.Name + "." + method.Name + " (" + paramString + ");";

        var fileName = dir + "/" + method.Name + ".md";
        var file = File.CreateText(fileName);

        var text = $@"---
title: {method.Name}()
order: 0
---

# {{{{ $frontmatter.title }}}}

<!--@include: ./{method.Name}_partial_header.md-->

## Function Definition

```c#
{methodString}
```

### Arguments

{paramList}

### Returns

* {method.ReturnType.FullName}

## Documentation

<!--@include: ./{method.Name}_partial_footer.md-->
";
        file.Write(text);
        file.Flush();
        file.Close();
      }
    }


    var indexFilePath = ServerPath + "/index.md";
    var indexFile = File.CreateText(indexFilePath);
    indexFile.Write(indexText);
    indexFile.Flush();
    indexFile.Close();

    return 0;
  }
}