# DotNetMasterPassword
.NET Implementation of the Master Password Algorithm

This is an independent C# implementation of the Master Password Algorithm http://masterpasswordapp.com/.
Not very polished so far, but it works.

Project Overview (all .NET):
* MonoMasterPasswordLib - contains basic algorithm, see http://masterpasswordapp.com/algorithm.html for detailed description of the algorithm.
* ConsoleMasterPassword - command line client (.NET 4.5)
* WpfMasterPassword - simplistic WPF Windows App (.NET 4.5)
* MonoMacMasterPassword - simplistic Mac App built with Xamarin Studio and XCode (Mono 4.0.0)

Tools used:
* Windows: Visual Studio 2015
* Mac: Xamarin Studio 5.8.3, XCode 4.2 (intentionally outdated to still run on my OS X 10.7)

Some code was taken from other sources:
* https://github.com/ChrisMcKee/cryptsharp for the implementation of Pbkdf2 and some other crypto base stuff
* https://github.com/PrismLibrary/Prism for some WPF base classes DelegateCommand and BindableBase 

LICENSE<br>
MIT License<br>
http://opensource.org/licenses/MIT

AUTHOR<br>
Martin Schmidt mail@gomasch.de Rostock, Germany
