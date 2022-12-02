/*
 * You can check more example of direct messaging in wiki pages:
 * https://github.com/ramtinak/InstagramApiSharp/wiki/Direct-messaging
 * 
 */

using System;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace Insta
{
    public static class ExtensionMethods
    {
        static public async Task<List<T>> ToListAsync<T>(this IEnumerable<Task<T>> This)
        {
            var tasks = This.ToList();     //Force LINQ to iterate and create all the tasks. Tasks always start when created.
            var results = new List<T>();   //Create a list to hold the results (not the tasks)
            foreach (var item in tasks)
            {
                results.Add(await item);   //Await the result for each task and add to results list
            }
            return results;
        }
    }

    internal class Messaging : IDemoSample
    {
        private readonly IInstaApi InstaApi;

        public Messaging(IInstaApi instaApi)
        {
            InstaApi = instaApi;
        }

        public async Task DoShow()
        {
            var recipientsResult = await InstaApi.MessagingProcessor.GetRankedRecipientsAsync();
            if (!recipientsResult.Succeeded)
            {
                Console.WriteLine("Unable to get ranked recipients");
                return;
            }
            Console.WriteLine("You can check more example of direct messaging in wiki pages:");
            Console.WriteLine("https://github.com/ramtinak/InstagramApiSharp/wiki/Direct-messaging");

            Console.WriteLine($"Got {recipientsResult.Value.Threads.Count} ranked threads");
            foreach (var thread in recipientsResult.Value.Threads)
                Console.WriteLine($"Threadname: {thread.ThreadTitle}, users: {thread.Users.Count}");

            var inboxThreads = await InstaApi.MessagingProcessor.GetDirectInboxAsync(InstagramApiSharp.PaginationParameters.MaxPagesToLoad(5));
            if (!inboxThreads.Succeeded)
            {
                Console.WriteLine("Unable to get inbox");
                return;
            }
            Console.WriteLine($"Got {inboxThreads.Value.Inbox.Threads.Count} inbox threads");
            foreach (var thread in inboxThreads.Value.Inbox.Threads)
                Console.WriteLine($"Threadname: {thread.Title}, users: {thread.Users.Count}");
            var all = inboxThreads.Value.Inbox.Threads
                .Where(t => t.LastActivity > DateTime.Parse("01.10.2022"))
                .Select(t => InstaApi.MessagingProcessor.GetDirectInboxThreadAsync(t.ThreadId, PaginationParameters.MaxPagesToLoad(1)).Result.Value)
                .ToList();
            using (FileStream fs = new FileStream(@"c:\\\\plot\\user.json", FileMode.OpenOrCreate))
            {
                await JsonSerializer.SerializeAsync<List<InstaDirectInboxThread>>(fs, all);
                Console.WriteLine("Data has been saved to file");
            }
            string json = JsonSerializer.Serialize(all);
            var firstThread = inboxThreads.Value.Inbox.Threads.FirstOrDefault(); 
            var dlg = await InstaApi.MessagingProcessor.GetDirectInboxThreadAsync(firstThread?.ThreadId, PaginationParameters.MaxPagesToLoad(1));

        }
    }
}
