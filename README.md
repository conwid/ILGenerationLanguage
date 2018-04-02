## What's this
This project is an ILSpy plugin to address the issue described here: https://github.com/icsharpcode/ILSpy/issues/1015
Currently it's in the very early stages of development, but it can decompile methods pretty well. Decompilation of other elements (types, fields, properties) are not supported yet. Also, method decompilation is definitely buggy (e.g. handling generic types is still far from perfect), but I think it's a good start.

![](https://dotnetfalconcontent.blob.core.windows.net/ilspy-plugin-to-decompile-an-assembly-with-reflectionemit/example.png)

## Contributing
Any help is appreciated. If you want to help out, you can do either of the two things:
* Just test stuff and if you find something that's not working or missing, create an issue. This would be a huge help because I'd know which features are useful for others and would help me focus.
* Implement something that's missing or fix something that's broken. If you want to do this, head over to the [ILSpy repository](https://github.com/icsharpcode/ILSpy), clone it, create a new folder inside the solution called ILGenerationLanguage.Plugin, clone the this repository into the folder, and you are good to go. I've set up the project so that if you build it in debug mode, it copies the output assembly to the output folder of the ILSpy project. Just rebuild, then start ILSpy in Debug mode.

## Download binaries
You can download the latest binaries from here: https://dotnetfalconcontent.blob.core.windows.net/ilspy-plugin-to-decompile-an-assembly-with-reflectionemit/ILGenerationLanguage.Plugin.dll
Just copy the downloaded assembly to the ILSpy folder and choose IL Generation from the language dropdown.

## Blog post
If you want to know more about my motivation, read my blog post on creating the plugin here:
www.dotnetfalcon.com/ilspy-plugin-to-decompile-an-assembly-with-reflectionemit-api-calls
