# ![duh](https://raw.githubusercontent.com/aornota/duh/master/src/ui/public/duh-24x24.png) | duh (ε)

Work-in-progress experiments with FSharp.Data.Adaptive &c. (based on [Peter Keše's example](https://github.com/pkese/Fable.React.Adaptive.Counter)).

[Visualization code](https://github.com/aornota/duh/blob/master/src/visualizer-console/visualizer.fs) (using Graphviz to produce dependency diagram) adapted from [Sergey Tihon's example](https://gist.github.com/sergey-tihon/46824acffb8c288fc5fe).

[Check it out!](https://aornota.github.io/duh/)

#### Development prerequisites

- [Microsoft .NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0): I'm currently using 6.0.304
- [Yarn](https://yarnpkg.com/lang/en/docs/install/): I'm currently using 1.22.19
- [Node.js (LTS)](https://nodejs.org/en/download/): I'm currently using 16.17.1

##### Also recommended

- [Microsoft Visual Studio Code](https://code.visualstudio.com/download/) with the following extensions:
    - [Microsoft C#](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
    - [Ionide-fsharp](https://marketplace.visualstudio.com/items?itemName=ionide.ionide-fsharp)
    - [Microsoft Debugger for Chrome](https://marketplace.visualstudio.com/items?itemName=msjsdiag.debugger-for-chrome)
    - [EditorConfig for VS Code](https://marketplace.visualstudio.com/items?itemName=editorconfig.editorconfig)
    - [Rainbow Brackets](https://marketplace.visualstudio.com/items?itemName=2gua.rainbow-brackets)
- [Google Chrome](https://www.google.com/chrome/) with the following extensions:
    - [React Developer Tools](https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi/)
    - [Redux DevTools](https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd/)
- ([Microsoft .NET Framework 4.7.2 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net472/): this appeared to resolve problems with Intellisense in
_[build.fsx](https://github.com/aornota/gibet/blob/master/build.fsx)_)

#### Running / building / publishing / deploying

- Before first running:
    - _dotnet tool restore_
    - _dotnet paket install_
- Build targets:
    - Run/watch for development (Debug): _dotnet fake build -t run_
    - Build for production (Release): _dotnet fake build -t build_
    - Publish to gh-pages (Release): _dotnet fake build -t publish-gh-pages_
    - Run the tests (Release): _dotnet fake build -t run-tests_
    - Run the visualizer console (Debug): _dotnet fake build -t run-visualizer-console_
    - Help (lists key targets): _dotnet fake build -t help_ (or just _dotnet fake build_)
