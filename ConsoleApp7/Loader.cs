using InstagramApiSharp.API.Builder;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insta
{
    public class Loader
    {
        private static IInstaApi InstaApi;
        public static async Task<bool> MainAsync()
        {
            try
                
            {
                Console.WriteLine("Enter login, pass below");
                var userSession = new UserSessionData
                {
                    UserName = Console.ReadLine(),
                    Password = Console.ReadLine()            
            };

                var delay = RequestDelay.FromSeconds(1, 1);
                InstaApi = InstaApiBuilder.CreateBuilder()
                    .SetUser(userSession)
                    .UseLogger(new DebugLogger(LogLevel.All)) 
                    .SetRequestDelay(delay)
                    .Build();

                const string stateFile = "state.bin";
                try
                {
                    if (File.Exists(stateFile))
                    {
                        Console.WriteLine("Loading state from file");
                        using (var fs = File.OpenRead(stateFile))
                        {
                            InstaApi.LoadStateDataFromStream(fs);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (!InstaApi.IsUserAuthenticated)
                {
                    // login
                    Console.WriteLine($"Logging in as {userSession.UserName}");
                    delay.Disable();
                    var logInResult = await InstaApi.LoginAsync();
                    delay.Enable();
                    if (!logInResult.Succeeded)
                    {
                        Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                        return false;
                    }
                }
                var state = InstaApi.GetStateDataAsStream();
                using (var fileStream = File.Create(stateFile))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }


                Console.WriteLine("Press 6 to start");

                var samplesMap = new Dictionary<ConsoleKey, IDemoSample>
                {
                    //[ConsoleKey.D1] = new Basics(InstaApi),
                  //  [ConsoleKey.D2] = new UploadPhoto(InstaApi),
                  //  [ConsoleKey.D3] = new CommentMedia(InstaApi),
                  //  [ConsoleKey.D4] = new Stories(InstaApi),
                  //  [ConsoleKey.D5] = new SaveLoadState(InstaApi),
                    [ConsoleKey.D6] = new Messaging(InstaApi),
                  //  [ConsoleKey.D7] = new LocationSample(InstaApi),
                  //  [ConsoleKey.D8] = new CollectionSample(InstaApi),
                  //  [ConsoleKey.D9] = new UploadVideo(InstaApi)
                };
                var key = Console.ReadKey();
                Console.WriteLine(Environment.NewLine);
                if (samplesMap.ContainsKey(key.Key))
                    await samplesMap[key.Key].DoShow();
                Console.WriteLine("Done. Press esc key to exit...");

                key = Console.ReadKey();
                return key.Key == ConsoleKey.Escape;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // perform that if user needs to logged out
                // var logoutResult = Task.Run(() => _instaApi.LogoutAsync()).GetAwaiter().GetResult();
                // if (logoutResult.Succeeded) Console.WriteLine("Logout succeed");
            }
            return false;
        }
    }
}
