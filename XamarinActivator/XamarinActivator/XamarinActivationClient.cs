using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using XamarinActivator.Models;

namespace XamarinActivator
{
    public class XamarinActivationClient
    {
        private readonly static bool IsWindows = Path.DirectorySeparatorChar == '\\';
        private const string TrialLevel = "Trial";

        private const string AuthUri = "https://auth.xamarin.com/api/v1/auth";
        private const string ActivateUri = "https://activation.xamarin.com/api/studio.ashx";
        private const string DeactivateUri = "https://activation.xamarin.com/api/deactivate.ashx";

        private const string MTouchToolPath = "/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/bin/mtouch";
        private const string MAndroidToolPath = "/Library/Frameworks/Xamarin.Android.framework/Versions/Current/bin/mandroid";
        private const string MMacToolPath = "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/bin/mmp";

        private const string DatafileArg = "--datafile";

        private readonly string userAgent;
        private readonly string apiKey;

        public XamarinActivationClient(string userAgent, string apiKey)
        {
            this.userAgent = userAgent;
            this.apiKey = apiKey;
        }

        public async Task RegisterProductsAsync(string email, string password, params XamarinProducts[] products)
        {
            var session = await LoginAsync(email, password);
            if (!session.Success)
            {
                throw new Exception("Unable to get Xamarin account: " + session.Error);
            }
            if (session.User == null || string.IsNullOrEmpty(session.Token))
            {
                throw new Exception("Xamarin account received does not contain user information or session token.");
            }

            foreach (var product in products)
            {
                var machineData = await GetMachineDataAsync(product);

                var result = await ActivateAsync(session.Token, session.User.Guid, product, machineData);
                if (result.Code != XamarinResponseCode.Success)
                {
                    throw new Exception(string.Format("Cannot activate Xamarin license. Code: {0}, Message: {1}, Details: {2}", result.Code, result.Message, result.MessageDetail));
                }

                await SaveLicenseAsync(result.Level, result.License, product);
            }

            await LogoutAsync(session.Token);
        }

        public async Task UnregisterProductsAsync(string email, string password, params XamarinProducts[] products)
        {
            var session = await LoginAsync(email, password);
            if (!session.Success)
            {
                throw new Exception("Unable to get Xamarin account: " + session.Error);
            }
            if (session.User == null || string.IsNullOrEmpty(session.Token))
            {
                throw new Exception("Xamarin account received does not contain user information or session token.");
            }

            foreach (var product in products)
            {
                var machineData = await GetMachineDataAsync(product);

                var result = await DeactivateAsync(session.Token, session.User.Guid, product, machineData);
                if (result.Code != XamarinResponseCode.Success)
                {
                    throw new Exception(string.Format("Cannot deactivate Xamarin license. Code: {0}, Message: {1}, Details: {2}", result.Code, result.Message, result.MessageDetail));
                }

                await DeleteLicenseAsync(result.Level, product);
            }

            await LogoutAsync(session.Token);
        }

        private HttpClient CreateHttpClient()
        {
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:", apiKey)));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            client.Timeout = TimeSpan.FromSeconds(30);

            return client;
        }

        private async Task<XamarinSession> LoginAsync(string email, string password)
        {
            var data = new Dictionary<string, string> {
                { "email", email },
                { "password", password },
            };

            using (var client = CreateHttpClient())
            using (var content = new FormUrlEncodedContent(data))
            using (var result = await client.PostAsync(AuthUri, content))
            {
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<XamarinSession>(json);
            }
        }

        private async Task<XamarinSession> LogoutAsync(string token)
        {
            var uri = string.Format("{0}/{1}", AuthUri, token);

            using (var client = CreateHttpClient())
            using (var result = await client.DeleteAsync(uri))
            {
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<XamarinSession>(json);
            }
        }

        private async Task<XamarinActivationResult> ActivateAsync(string sessionToken, string userGuid, XamarinProducts product, string machineData)
        {
            var productCode = GetProductCode(product);
            var uri = string.Format("{0}?token={1}&guid={2}&product={3}", ActivateUri, Uri.EscapeUriString(sessionToken), Uri.EscapeUriString(userGuid), Uri.EscapeUriString(productCode));

            using (var client = CreateHttpClient())
            using (var content = new StringContent(machineData, Encoding.ASCII, "text/plain"))
            using (var result = await client.PostAsync(uri, content))
            {
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<XamarinActivationResult>(json);
            }
        }

        private async Task<XamarinActivationResult> DeactivateAsync(string sessionToken, string userGuid, XamarinProducts product, string machineData)
        {
            var productCode = GetProductCode(product);
            var uri = string.Format("{0}?token={1}&guid={2}&product={3}", DeactivateUri, Uri.EscapeUriString(sessionToken), Uri.EscapeUriString(userGuid), Uri.EscapeUriString(productCode));

            using (var client = CreateHttpClient())
            using (var content = new StringContent(machineData, Encoding.ASCII, "text/plain"))
            using (var result = await client.PostAsync(uri, content))
            {
                result.EnsureSuccessStatusCode();

                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<XamarinActivationResult>(json);
            }
        }

        private static Task<string> GetMachineDataAsync(XamarinProducts product)
        {
            switch (product)
            {
                case XamarinProducts.Android:
                    return RunTool(MAndroidToolPath, DatafileArg);
                case XamarinProducts.iOS:
                    return RunTool(MTouchToolPath, DatafileArg);
                case XamarinProducts.Mac:
                    return RunTool(MMacToolPath, DatafileArg);
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        private static async Task<string> RunTool(string tool, string args)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });
            var output = await process.StandardOutput.ReadToEndAsync();

            var tcs = new TaskCompletionSource<string>();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                tcs.SetException(new Exception(string.Format("{0} exited with {1} code.", tool, process.ExitCode)));
            }
            else {
                tcs.SetResult(output);
            }

            return await tcs.Task;
        }

        private static string GetProductCode(XamarinProducts product)
        {
            switch (product)
            {
                case XamarinProducts.Android:
                    return XamarinProductCodes.Android;
                case XamarinProducts.iOS:
                    return XamarinProductCodes.iOS;
                case XamarinProducts.Mac:
                    return XamarinProductCodes.Mac;
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        private static string GetLicensePath(string level, XamarinProducts product)
        {
            var isTrial = TrialLevel.Equals(level, StringComparison.OrdinalIgnoreCase);

            if (IsWindows)
            {
                var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                switch (product)
                {
                    case XamarinProducts.iOS:
                        return Path.Combine(commonAppData, "MonoTouch", "License", isTrial ? "monotouch.trial.licx" : "monotouch.licx");
                    case XamarinProducts.Android:
                        return Path.Combine(commonAppData, "Mono for Android", "License", isTrial ? "monoandroid.trial.licx" : "monoandroid.licx");
                }
            }
            else {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                switch (product)
                {
                    case XamarinProducts.iOS:
                        return Path.Combine(homeDir, "Library", "MonoTouch", isTrial ? "License.trial" : "License.v2");
                    case XamarinProducts.Android:
                        return Path.Combine(homeDir, "Library", "MonoAndroid", isTrial ? "License.trial" : "License");
                    case XamarinProducts.Mac:
                        return Path.Combine(homeDir, "Library", "Xamarin.Mac", isTrial ? "License.trial" : "License");
                }
            }
            throw new ArgumentOutOfRangeException("product");
        }

        private static async Task SaveLicenseAsync(string level, string license, XamarinProducts product)
        {
            var path = GetLicensePath(level, product);
            var licenseData = Convert.FromBase64String(license);

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(path, licenseData);
        }

        private static async Task DeleteLicenseAsync(string level, XamarinProducts product)
        {
            var path = GetLicensePath(level, product);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
