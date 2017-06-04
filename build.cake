var target = Argument("target", "Default");

var FontAwesomeVersion = "v4.7.0";
var FontAwesomeStyleUrl = string.Format("https://raw.githubusercontent.com/FortAwesome/Font-Awesome/{0}/css/font-awesome.min.css", FontAwesomeVersion);
var FontAwesomeFontUrl = string.Format("https://raw.githubusercontent.com/FortAwesome/Font-Awesome/{0}/fonts/fontawesome-webfont.ttf", FontAwesomeVersion);

Task("Externals")
    .Does(() =>
{
    // download all the styles
    EnsureDirectoryExists("./externals/FontAwesome/");
    if (!FileExists("./externals/FontAwesome/font-awesome.min.css")) DownloadFile(FontAwesomeStyleUrl, "./externals/FontAwesome/font-awesome.min.css");
    if (!FileExists("./externals/FontAwesome/fontawesome-webfont.ttf")) DownloadFile(FontAwesomeFontUrl, "./externals/FontAwesome/fontawesome-webfont.ttf");
});

Task("Build")
    .IsDependentOn("Externals")
    .Does(() =>
{
    var GenerateIconifySource = new Action<FilePath, string>((stylesheet, type) => {
        var iconify = MakeAbsolute((FilePath)"./output/IconifyGenerator/iconify.exe");
        var iconifyResult = StartProcess(iconify, new ProcessSettings {
            Arguments = new ProcessArgumentBuilder()
                .AppendSwitchQuoted("-o", "=", stylesheet.GetDirectory().CombineWithFilePath(type + ".generated.cs").FullPath)
                .AppendSwitchQuoted("-n", "SkiaSharp.Extended.Iconify")
                .AppendSwitchQuoted("-t", type)
                .AppendQuoted(stylesheet.FullPath)
        });
        if (iconifyResult != 0) {
            throw new Exception("iconify.exe failed.");
        }
    });

    // first build the generator
    NuGetRestore("./source/IconifyGenerator.sln");
    DotNetBuild("./source/IconifyGenerator.sln", settings => settings.SetConfiguration("Release"));

    // copy generator to output
    EnsureDirectoryExists("./output/");
    CopyDirectory("./source/IconifyGenerator/bin/Release", "./output/IconifyGenerator/");

    // then, run the generator on the styles
    GenerateIconifySource("externals/FontAwesome/font-awesome.min.css", "FontAwesome");

    // now build the libraries
    NuGetRestore("./source/SkiaSharp.Extended.Iconify.sln");
    DotNetBuild("./source/SkiaSharp.Extended.Iconify.sln", settings => settings.SetConfiguration("Release"));

    // copy to output
    EnsureDirectoryExists("./output/");
    CopyFileToDirectory("./source/SkiaSharp.Extended.Iconify/bin/Release/SkiaSharp.Extended.Iconify.dll", "./output/");
    CopyFileToDirectory("./source/SkiaSharp.Extended.Iconify.FontAwesome/bin/Release/SkiaSharp.Extended.Iconify.FontAwesome.dll", "./output/");
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories ("./source/*/bin");
    CleanDirectories ("./source/*/obj");
    CleanDirectories ("./source/packages");

    CleanDirectories ("./samples/*/bin");
    CleanDirectories ("./samples/*/obj");
    CleanDirectories ("./samples/packages");
    CleanDirectories ("./samples/*/AppPackages");
    DeleteFiles ("./samples/*/project.lock.json");
    CleanDirectories ("./samples/*/*/bin");
    CleanDirectories ("./samples/*/*/obj");
    CleanDirectories ("./samples/*/packages");
    CleanDirectories ("./samples/*/*/AppPackages");
    DeleteFiles ("./samples/*/*/project.lock.json");

    if (DirectoryExists ("./externals"))
        DeleteDirectory ("./externals", true);

    if (DirectoryExists ("./output"))
        DeleteDirectory ("./output", true);
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
