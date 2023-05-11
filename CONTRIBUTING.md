## Contributing to ArcCreate

This document covers guidelines for contributing code to ArcCreate.
For translation contributions, refer to [TRANSLATING.md](TRANSLATING.md)

### General guidelines

This section covers basic guidelines for writing code.

#### Use an IDE or a decent text editor

My text editor of choice is [VSCode](https://code.visualstudio.com/) because my potato PC can't handle anything better. But feel free to use [Visual Studio](https://visualstudio.microsoft.com) or [Rider](https://www.jetbrains.com/rider/) if you can afford to.

#### Use the correct Unity version

Do not upgrade or downgrade the project's version, and use the exact version of Unity as everyone else. If you have a good reason to upgrade then open an issue and everyone can discuss on it.

#### Follow code style guidelines

This project has already been configured to work with StyleCop linter, and will highlight any style guidelines violations within your code (at least, I've only tested this on VSCode, but it should work similarly with Visual Studio). There's also codefixes available (`Ctrl+.` on VSCode) which is very handy for quickly reformatting to the style guidelines.

Also, please avoid modifying .editorconfig to disable linting rules before consulting with me and other contributors first.

#### Write XML documentation for public members

Refer to [Microsoft's guide on XML documentation tags here](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags).

Public members (i.e methods and properties) of a class should be properly documented with XML tags (these information will even appear when you hover over them!). If you're on VSCode, the [C# XML Documentations Comments](https://marketplace.visualstudio.com/items?itemName=k--kato.docomment) extension is very handy for quickly writing these.

The reason I'm enforcing this is that I have massive skill issue in understanding other's code. Proper documentations makes it more clear what your code is actually doing and make it so that others have an easier time interfacing with your code.

Note that you can skip writing documentation for private members, and members that are very obvious what they are (for example, the `Timing` property of a note). Also if you anticipate that a class will not be used in too many places, not writing documentation for it is also acceptable.

#### Discuss first before making major changes

If you want to make any major changes to the application, such as addition of entirely new features, or major refactoring, make sure to discuss with me before doing anything. This helps both you and me from wasting each other times, and I can offer some advices if necessary. For small changes feel free to open a PR without discussing beforehands.

### About paid assets

This project makes no use of paid assets except for NativeAudio. Due to licensing the asset will not be included in the public git repository.

The codebase is setup so that NativeAudio will only be included when building for Android and iOS, and also if the [scripting define symbol](https://docs.unity3d.com/2019.4/Documentation/Manual/PlatformDependentCompilation.html) `USE_NATIVE_AUDIO` is defined. Therefore, if you don't have the asset yourself, everything should still function normally.

If you own NativeAudio and would like to use it in the editor to test things out locally, first import and move the plugin to `Assets/Plugins/Native Audio`, be careful not to overwrite any file (especially `E7.NativeAudio.asmdef.meta`). Then create the file `Assets/csc.rsp` and write `-define:USE_NATIVE_AUDIO` in the file, save it and then reimport any script in the Unity Editor.

### Optimization guidelines

#### Only optimize where necessary

In other words, try to optimize on code paths that are run every single frame. It's also recommended that you profile the game before trying optimize, since you might waste a bunch of time at best, or make the problem worse at worst. Unity's profiler is very capable and you should rely on it to know where to optimize.

#### Avoid allocating memory at runtime

Allocating memory will generate garbage, which needs to be cleaned up later. This is called garbage collection. This is one of the biggest cause of performance issue in C# programs.

Worse, a small amount of allocation each frame will still cause issue. As we don't know when the garbage collector wil be run, it might try to clean up a bunch of garbage at once, which might cause lag spikes. This is extremely detrimental to a rhythm game like ArcCreate. It also doesn't help that Unity's garbage collector is one of the least optimized out there. Therefore you should minimize memory allocation as much as possible.

Runtime allocation is acceptable if it's only contained to the Compose system (i.e. only happens on Desktop builds). You should still try to minimize allocation though.

You can read [the official guide by Unity](https://docs.unity3d.com/Manual/performance-garbage-collection-best-practices.html), or [this article by Sebastiano Mandalà](https://www.sebaslab.com/zero-allocation-code-in-unity/), which goes in depth about how you'd achieve this.

### Git workflow

(Inspired by [NodeJs's documentation](https://github.com/nodejs/node/blob/main/doc/contributing/pull-requests.md))

Working on a git repository of course requires some git knowledge. Some basic understanding of commits, branches, and remotes will go a long way when you contribute to an open source project.

Specific to ArcCreate however, to begin contributing, create a fork of this repository, and clone it. Your fork should be added as a remote named `origin`. To keep your fork updated, add this official repository as an upstream remote:

```
// Add original repo as upstream
git remote add upstream https://github.com/Arcthesia/ArcCreate.git

// Fetch changes from upstream
git fetch upstream
```

Create local branches from master and start coding changes.

For every commit, make sure to write descriptive commit names of the following format:

- Line 1: `[Module name]: [Verb] [Change]` (preferably less than 50 characters long)
- Line 2 should be kept blank 
- Line 3 onward: detailed description of your changes. Add links (such as issues URL, documentation) for references as necessary.

> For example:
> ```
> Gameplay: Fix arctap display with angleX
> 
> - Fixed issue with arctap shadow y position not being 0 within a timinggroup with angleX property.
> 
> Issue: (issue url here)
> ```

To submit your changes, create a pull request. Make sure to rebase your branch with upstream to synchronize your changes with the newest version from the main repository.
```
git fetch upstream HEAD
git rebase FETCH_HEAD
```
Fix any merge conflicts that might occur, and do thorough testing there before submitting a pull request.

Pull request should have descriptive names. Also attach images / videos as neccessary to help others understand your changes.

### Other notes

You're free to modify [/Assets/StreamingAssets/credit.txt](/Assets/StreamingAssets/credit.txt) if you want your name to be included in the credit as part of your pull request.
