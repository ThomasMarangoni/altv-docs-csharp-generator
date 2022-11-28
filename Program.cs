using System.Reflection;
using System.Security.Cryptography.X509Certificates;

const string serverPath = "docs/api/server";

var assembly = Assembly.LoadFrom("data/server/AltV.Net.dll");
Console.WriteLine("Loaded AltV.Net.dll");

if (!Directory.Exists(serverPath))
{
  Directory.CreateDirectory(serverPath);
}
else
{
  Directory.Delete(serverPath, true);
  Directory.CreateDirectory(serverPath);
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

var indexText = "";
foreach (var type in assembly.GetTypes().Where(x => x.IsPublic && !x.IsSpecialName && x.Module.ToString().Contains("AltV.Net.")))
{
  var namespaceString = type.Namespace?.Replace(".", "_");
  var dir = serverPath + "/" + namespaceString + "/" + type.Name;
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
  foreach (var property in type.GetProperties().Where(x => (x.GetMethod?.IsPublic == true || x.SetMethod?.IsPublic == true) 
                                                           &&!x.IsSpecialName))
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
  foreach (var property in type.GetProperties().Where(x => (x.GetMethod?.IsPublic == true || x.SetMethod?.IsPublic == true) 
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


var indexFilePath = serverPath + "/index.md";
var indexFile = File.CreateText(indexFilePath);
indexFile.Write(indexText);
indexFile.Flush();
indexFile.Close();
