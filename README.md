# XamlWatcher
XamlWatcher is a library that lets you edit the XAML of a WPF app, and have the running instance of the app update each time you save your XAML.

![](http://i.imgur.com/nYIxVpF.gif)

It is designed with MVVM apps in mind and will work with views and resource dictionaries. With a view the content of the view and the bindings will refresh. With resource dictionaries anything using a `DynamicResource` from the dictionary will update (of course if you are using `StaticResource` in a view you can edit that first to make it a `DynamicResource` then edit the resource dictionary.

# Usage
Install with nuget
```
>Install-Package XamlWatcher.WPF
```
Spin up a watcher and point it at the top level of the location of your XAML files. Keep a reference to the watcher around. You probably want to wrap this all in an `#if DEBUG`
```
_watcher = new Watcher(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName);
```
XamlWatcher is internally wrapped in a try/catch so we don't bring your app down if something goes wrong. But you might want to show a toast or something if that happens so the developer knows. In this example `Alert.OfException` is from the app we are in, it isn't part of XamlWatcher. Just hook into your own app's alert system.
```
_watcher.OnError = ex => Alert.OfException("XAML Watcher Error", "XAML Watcher update has failed", ex);
```
You can also hook into the new view after it has been refreshed. You need to do this if you are using Caliburn.Micro's convention based bindings:
```
_watcher.OnRefreshed = view =>
{
  view.SetValue(ViewModelBinder.ConventionsAppliedProperty, false);
  ViewModelBinder.Bind(view.DataContext, view, null);
};
```

#Limitations
XamlWatcher won't work *at all* on views where you have events hooked up in the XAML. 
```
<Button Click="Button_Clicked"/> <!-- XamlWatcher won't work at all if you do this -->
```
If you're doing cleanish MVVM this shouldn't be too much of a problem. 

![](https://ci.appveyor.com/api/projects/status/github/markryd/XamlWatcher?branch=master&svg=true)
