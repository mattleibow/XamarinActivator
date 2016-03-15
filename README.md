# Xamarin License Activator

[![Build status][1]][2]

## Usage

Using the activator is simple, and just requires a valid Xamarin account and
either Xamarin.iOS, Xamarin.Andoird or Xamarin.Mac to be installed on the 
current machine. 

### Activate

For example to activate Xamarin.iOS on the current machine:

```bash
XamarinActivator.exe activate -x ios -e email@example.com -p P@55w0rd
```

### Deactivate

And then to deactivate *all* the licenses on the current machine:

```bash
XamarinActivator.exe deactivate -x ios -e email@example.com -p P@55w0rd
```

Although all the licenses will be deactivated, you still need to specify what tools are 
installed on the machine. This is because the license is generated using the Xamarin tools.

### Advanced Usage

There are several other convinent ways to make use of the tool:

#### Multiple Activations

Multiple licenses can be activated at the same time, such as when building a 
Xamarin.Forms app.

```bash
XamarinActivator.exe activate -x ios -x android -x mac -e email@example.com -p P@55w0rd
```

#### Custom User-Agent

Instead of using the default User-Agent "XamarinActivator" you can specify 
a cusom one:

```bash
XamarinActivator.exe activate -x ios -e email@example.com -p P@55w0rd -u "Travis-CI"
```


## Help

```
Usage: XamarinActivator [OPTIONS]+ action
Activate or deactivate a Xamarin platform license on the current machine.
If no action is specified, activate will be used.

Actions:
  activate                   activate the platform license
  deactivate                 deactivate the platform license

Options:
  -h, -?, --help             show this message and exit

License/User Credentials:
  -x, --xamarin=VALUE        the Xamarin platfom to activate/deactivate:
                               ios, andoid, mac
  -e, --email=VALUE          the email address to use to log in.
  -p, --password=VALUE       the password to use to log in.

Xamarin Credentials: (optional)
  -k, --apikey=VALUE         the Xamarin API key to use when communicating with the server.
  -u, --useragent=VALUE      the User-Agent to use when communicating with the server.
```

[1]: https://ci.appveyor.com/api/projects/status/tiqess2rnyj516k4/branch/master?svg=true
[2]: https://ci.appveyor.com/project/mattleibow/xamarinactivator/branch/master
