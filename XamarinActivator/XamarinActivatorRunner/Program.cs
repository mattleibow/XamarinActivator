using Mono.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using XamarinActivator;

namespace XamarinActivatorRunner
{
    class MainClass
    {
        public static int Main(string[] args)
        {
            var email = string.Empty;
            var password = string.Empty;
            var apiKey = string.Empty;
            var userAgent = string.Empty;
            var products = new List<XamarinProducts>();
            var showHelp = false;
            var action = Actions.Activate;

            var options = new OptionSet() {
                { "h|help",  "show this message and exit", h => showHelp = h != null },
                { "x|xamarin=", "the Xamarin platfom to activate/deactivate:\nios, andoid, mac", p => {
                    XamarinProducts prod;
                    if (Enum.TryParse(p, true, out prod))
                        products.Add (prod);
                    else
                        Console.WriteLine("Unable to parse Xamarin platform: '{0}'", p);
                } },
                { " License/User Credentials:" },
                { "e|email=", "the email address to use to log in.", (string e) => email = e },
                { "p|password=", "the password to use to log in.", (string p) => password= p },
                { " Xamarin Credentials:" },
                { "k|apikey=", "the Xamarin API key to use when communicating with the server.", (string k) => apiKey = k },
                { "u|useragent=", "the User-Agent to use when communicating with the server.", (string u) => userAgent = u },
            };

            List<string> extras;
            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException ex)
            {
                return ShowError(ex.Message);
            }

            if (showHelp)
            {
                return ShowHelp(options);
            }

            var act = extras.FirstOrDefault();
            if (!string.IsNullOrEmpty(act) && !Enum.TryParse(act, true, out action))
            {
                return ShowError("An invalid action was provided.");
            }
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return ShowError("The email address and/or was not provided.");
            }
            if (string.IsNullOrEmpty(apiKey))
            {
                return ShowError("The API key was not provided.");
            }
            if (string.IsNullOrEmpty(userAgent))
            {
                return ShowError("The User-Agent was not provided.");
            }
            if (products.Count == 0)
            {
                return ShowError("At least one Xamarin platform must be provided");
            }

            try
            {
                var client = new XamarinActivationClient(userAgent, apiKey);
                switch (action)
                {
                    case Actions.Activate:
                        client.RegisterProductsAsync(email, password, products.ToArray()).Wait();
                        break;
                    case Actions.Deactivate:
                        client.UnregisterProductsAsync(email, password, products.ToArray()).Wait();
                        break;
                    default:
                        throw new Exception("An invalid action was provided.");
                }

                Console.WriteLine("XamarinActivator: Success.");
            }
            catch (AggregateException ex)
            {
                return ShowError(ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                return ShowError(ex.Message);
            }

            return 0;
        }

        private static int ShowError(string errorMessage)
        {
            Console.Write("XamarinActivator: ");
            Console.WriteLine(errorMessage);
            Console.WriteLine("Try 'XamarinActivator --help' for more information.");

            return 1;
        }

        private static int ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: XamarinActivator [OPTIONS]+ action");
            Console.WriteLine("Activate or deactivate a Xamarin platform license on the current machine.");
            Console.WriteLine("If no action is specified, activate will be used.");
            Console.WriteLine();
            Console.WriteLine("Actions:");
            Console.WriteLine("  activate                   activate the platform license");
            Console.WriteLine("  deactivate                 deactivate the platform license");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            return 0;
        }
    }
    
    public enum Actions
    {
        Activate,
        Deactivate
    }
}
