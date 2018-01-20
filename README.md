# DotNetMasterPassword
.NET Implementation of the Master Password Algorithm

This is an independent implementation for Windows in C# of the Master Password Algorithm http://masterpasswordapp.com/.

The original master password apps and the concept is from Maarten Billemont, Lyndir. The repository here is just an independent implementation in .NET from a different author.

The basic idea is that you remember just one secret password that is never stored anywhere. You dynamically generate individual passwords for each site that you use.
The list of sites that you use can be saved or re-entered on different devices (e.g. your smartphones and pads) and machines (your PCs/Macs at home or at work).
Provided that the apps use the same algorithm and the same base data (your global name, your one master password, name of the site, counter and passwordtype for the site)
the passwords can be dynamically recreated everywhere. If a site is hacked then only that one specific password is compromised, not your other passwords. This 
rests on a specific cryptographic one-way-algorithm that makes it very hard to guess the master password from the site specific password. This is a brilliant concept IMO.

Project Overview:
* MasterPasswordLib - contains basic algorithm, see http://masterpasswordapp.com/algorithm.html for detailed description of the algorithm (.NET 4.5 / Mono 4.0.0).
* ConsoleMasterPassword - command line client (.NET 4.5)
* WpfMasterPassword - simple WPF Windows App (.NET 4.5)
* MonoMacMasterPassword - simplistic Mac App built with Xamarin Studio and XCode (.NET 4.5).
  Maarten Billemont also has a Mac app though at http://masterpasswordapp.com/, please consider that instead.

Tools used:
* Windows: Visual Studio 2015
* Mac: Xamarin Studio 6.3, XCode 7.3.1, Xamarin.Mac 3.2.0.175 (outdated to still run on my OS X 10.11, runs only in debug for some reason)

NOTE: I've reimplemented this as an Electron app at https://github.com/gomasch/ElectronMPW and switched to using that.

Screenshots

![Windows GUI Screenshot](WpfMasterPassword/Screenshot.png?raw=true "Windows GUI Screenshot")

Some code was taken from other sources:
* https://github.com/ChrisMcKee/cryptsharp for the implementation of Pbkdf2 and some other crypto base stuff
* https://github.com/PrismLibrary/Prism for some WPF base classes DelegateCommand and BindableBase 
* http://blogs.msdn.com/b/davidrickard/archive/2010/03/09/saving-window-size-and-location-in-wpf-and-winforms.aspx for window placement
* https://github.com/evanwon/WPFCustomMessageBox for custom message box

LICENSE<br>
MIT License<br>
http://opensource.org/licenses/MIT

AUTHOR<br>
Martin Schmidt mail@gomasch.de Rostock, Germany
