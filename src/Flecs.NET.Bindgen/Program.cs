using System.Runtime.CompilerServices;
using Bindgen.NET;

BindingOptions bindingOptions = new()
{
    Namespace = "Flecs.NET.Bindings",
    Class = "flecs",

    DllImportPath = "flecs",

    SuppressedWarnings = { "CS8981" },

    SystemIncludeDirectories = { Path.Combine(BuildConstants.ZigLibPath, "include") },
    IncludeDirectories = { GetFlecsIncludePath() },

    InputFile = GetFlecsHeaderPath(),
    OutputFile = GetBindingsOutputPath(),
    NativeOutputFile = GetBindingsHelperOutputPath(),

    GenerateExternVariables = true,
    GenerateDisableRuntimeMarshallingAttribute = true,

    Ignored = {"FLECS_IDEcsPipelineQueryID_"}
};

if (OperatingSystem.IsMacOS())
    bindingOptions.SystemIncludeDirectories.Add(Path.Combine(BuildConstants.ZigLibPath, "libc", "include", "any-macos-any"));

BindingGenerator.Generate(bindingOptions);
FixFunctionPointerInlineArrayName(bindingOptions.OutputFile);

return;

static void FixFunctionPointerInlineArrayName(string outputFile)
{
    string text = File.ReadAllText(outputFile);
    const string functionPointer = "delegate* unmanaged<ecs_function_ctx_t*, int, ecs_value_t*, ecs_value_t*, int, void>";
    string invalidTypeName = $"{functionPointer} _18";
    const string validTypeName = "ecs_vector_function_callback_t_18";

    text = text.Replace($"InlineArrays.\r\n\r\n{invalidTypeName} \r\n\r\nvector_callbacks", $"InlineArrays.{validTypeName} vector_callbacks");
    text = text.Replace($"InlineArrays.\n\n{invalidTypeName} \n\nvector_callbacks", $"InlineArrays.{validTypeName} vector_callbacks");
    text = text.Replace($"public partial struct \r\n\r\n{invalidTypeName} {{", $"public partial struct {validTypeName}\r\n        {{");
    text = text.Replace($"public partial struct \n\n{invalidTypeName} {{", $"public partial struct {validTypeName}\n        {{");
    text = text.Replace(invalidTypeName, validTypeName);

    File.WriteAllText(outputFile, text);
}

string GetFlecsIncludePath([CallerFilePath] string filePath = "")
{
    return Path.GetFullPath(Path.Combine(filePath, "..", "..", "..", "native", "flecs", "include"));
}

string GetFlecsHeaderPath([CallerFilePath] string filePath = "")
{
    return Path.GetFullPath(Path.Combine(filePath, "..", "..", "..", "native", "flecs", "include", "flecs.h"));
}

string GetBindingsOutputPath([CallerFilePath] string filePath = "")
{
    return Path.GetFullPath(Path.Combine(filePath, "..", "..", "Flecs.NET.Bindings", "Flecs.g.cs"));
}

string GetBindingsHelperOutputPath([CallerFilePath] string filePath = "")
{
    return Path.GetFullPath(Path.Combine(filePath, "..", "..", "..", "native", "flecs_helpers.c"));
}
