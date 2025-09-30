# RueI
RueI is a hint framework, designed to be the definitive way to display multiple hints at once. you can get the latest release [here](https://github.com/Ruemena/RueI/releases/latest).

if you want to develop using RueI, make sure you install the [nuget package](https://www.nuget.org/packages/RueI)

### Installation
RueI is a LabAPI plugin, although it works with EXILED plugins, too. simply download the [latest release](https://github.com/Ruemena/RueI/releases/latest), then
put it in your plugin folder.

### Example
```cs
RueDisplay display = RueDisplay.Get(player);
Tag welcomeTag = new();
display.Show(welcomeTag, new BasicElement(800, "Welcome to the server!"));
display.Show(new BasicElement(300, "Don't forget to read the rules!"), 10f);
Timing.CallDelayed(5f, () =>
{
    display.Show(welcomeTag, new BasicElement(800, "New update: We added support for multiple hints at once!"), 10f);
});
```

### Features
- support for displaying multiple hints at once without them interfering with eachother
- allows using hint parameters 
- extensions for StringBuilder that makes adding tags easier 
- aspect ratio support
- extremely optimized and efficient

RueI is not a grid-based or line-based system, it calculates the offset necessary to put a hint at the same position no matter what
## Comments
if you've encountered any bugs please [make an issue](https://github.com/paislee/RueI/issues) (it helps me out a ton)
