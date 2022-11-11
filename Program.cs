using System.Reflection;

var assembly = Assembly.LoadFrom("data/AltV.Net.dll");
Console.WriteLine("Loaded AltV.Net.dll");

if (!Directory.Exists("docs/api"))
  Directory.CreateDirectory("docs/api");

foreach (var type in assembly.GetTypes().Where(x => x.IsPublic && x.Module.ToString().Contains("AltV.Net.")))
{
  var dir = "docs/api/" + type.Namespace + "/" + type.Name;
  if (!Directory.Exists(dir))
    Directory.CreateDirectory(dir);

  foreach (var property in type.GetProperties().Where(x => !x.IsSpecialName))
  {
    var propertyString = property.PropertyType + " " + property.Name + " { ";
    if (property.CanRead)
      propertyString += "get; ";
    if (property.CanWrite)
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

```cs
{propertyString}
```

## Documentation

<!--@include: ./{property.Name}_partial_footer.md-->
";
    
    file.Write(text);
    file.Flush();
    file.Close();
  }
  
  foreach(var method in type.GetMethods().Where(x => x.IsPublic && !x.IsSpecialName))
  {
    var paramString = "";
    var paramList = "";
    foreach(var param in method.GetParameters().OrderBy(x => x.Position))
    {
      paramString += param.ParameterType.FullName + " "  + param.Name;

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
    if(method.GetParameters().Length > 0)
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

```cs
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
