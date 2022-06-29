// See https://aka.ms/new-console-template for more information

using Noppes.E621;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using System.IO;
using ADDONS;

#nullable disable

//Function from StackOverflow(Finding words in a string) ended up using string.contains
static string getBetween(string strSource, string strStart, string strEnd)
{
    if (strSource.Contains(strStart) && strSource.Contains(strEnd))
    {
        int Start, End;
        Start = strSource.IndexOf(strStart, 0) + strStart.Length;
        End = strSource.IndexOf(strEnd, Start);
        return strSource.Substring(Start, End - Start);
    }

    return "";
}



#region Logging and Organization


    //This create a Sections Folder, a Log File and prepares the Session folder to save the Images
    var ProgramSrcPath = Directory.GetCurrentDirectory();
    var SessionDir = ProgramSrcPath + "/Sessions";
    var LogPath = $@"{SessionDir}/log.txt";
    var date = DateTime.Now;
    var SessionOutput = $"{SessionDir}/Session - {date.ToString("dd-MM-yyyy")}";

    #region Organization, File and Folder Creation and Exception Catcher

        Console.WriteLine("\n--------------------------------------------");
        Console.WriteLine("-------------------------------------------------- --    -\n");

        Console.WriteLine("     PURR - An E621 CLI DOWNLOADER (WIP)");
        Console.WriteLine("     VERSION - 0.1.5b");
        Console.WriteLine("     Author: Edgar Takamura");

        Console.WriteLine("\n-------------------------------------------------- --    -");
        Console.WriteLine("--------------------------------------------\n");

        //This creates a directory to store our sessions and therefore where the downloaded images will be downloaded
        if (!Directory.Exists(SessionDir))
        {
            Directory.CreateDirectory(SessionDir); //Creates the "Sessions" folder
            Directory.CreateDirectory(SessionOutput);//Inside of it creates the "Session - Date" folder to store the posts
            Console.WriteLine($"Sessions Directory - Session storage folder created at {SessionDir}");
            Console.WriteLine($"Current Session Directory - Section output folder created at {SessionOutput}");
        }
        else
        {
            Console.WriteLine("Sessions Directory - Directory Exists, recording session");

            if (!Directory.Exists(SessionOutput))
            {
                Directory.CreateDirectory(SessionOutput);
                Console.WriteLine($"Current Session Directory - Session output folder created at {SessionOutput}");
            }
            else 
            {
                Console.WriteLine($"Current Session Directory - Session output stored at {SessionOutput}");
            }

            //This will be used for creating a mini log, saying what we searched, it will be modified later at the end of the process
            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(LogPath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"{DateTime.Now.ToString()} - Awaiting input");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                // Open the stream and read it back.
                using (StreamReader sr = File.OpenText(LogPath))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        Console.WriteLine($"From Log File: {s}");
                    }
                }
            }
            //Catches any exception when creating a file (if any)
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        Console.WriteLine("\nImages are stored according Topic(Pools/Posts) and Researched Tags inside the Session Folder");
        Console.WriteLine("\n--------------------------------------------------");
        Console.WriteLine("--------------------------------------------------\n");

    #endregion

#endregion

//Little Comment

Console.WriteLine("How do you do Fellow Degenerate?");
Console.WriteLine("What are you searching today? - Pools? Posts? we have it my friend. - Type below which one" + "\n");
var topic = Console.ReadLine();


switch (topic.ToLower())
{

    case "posts":
            
            //This checks for the Category(Posts/Pools) directory, if does not exists, creates and sets it.
            if (Directory.Exists($"{SessionOutput}/Posts"))
            {
                Directory.CreateDirectory($"{SessionOutput}/Posts");
                SessionOutput = $"{SessionOutput}/Posts";
            }
            else
            {
                SessionOutput = $"{SessionOutput}/Posts";
            }

            //This will take the User's topic and ask for which Tags and for How Many posts to search for
            Console.WriteLine($"\nSo... {topic} huh?");
            Console.WriteLine("What ya having? - Type what you search below UwU" + "\n");
            var tags = Console.ReadLine();
            Console.WriteLine($"\nHmmmm.... {tags} isn't it? Daring today aren't we?");
            Console.WriteLine("Now... how many?");
            var HowMany = Int32.Parse(Console.ReadLine());

            //This will ask for a confirmation of the User's Option
            Console.WriteLine($"\nTags to search = {tags}");
            Console.WriteLine($"Posts to accuire = {HowMany}");
            Console.WriteLine("Confirm?(Yes/No)" + "\n");
            var confirm = Console.ReadLine();

        if (confirm.ToLower() == "yes")
        {

            if (tags != null && HowMany >= 1)
            {

                var e621Client = new E621ClientBuilder()
                    .WithUserAgent("PURR - An E621 CLI DOWNLOADER (WIP)", "0.1.5b", "Pervertamura", "NONE")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();

                bool success = await e621Client.LogInAsync("MyUsername", "255154");

                if (success)
                {
                    Console.WriteLine($"\nLogged with success - System returned: {success}" + "\n");    
                }
                else
                {
                    Console.WriteLine($"\nUnable to LOG-IN - System Returned:{success}" + "\n");
                }

                var posts = await e621Client.GetPostsAsync(tags, 1, HowMany);

                for (int i = 0; i < posts.Count; i++)
                {
                    var post = posts.ElementAt(i);

                    //This checks for a file in the posts... Why would not have one???
                    if (post.File != null && post.File.FileExtension != "webm")
                    {
                        #region This gather the Post images url and convert them to stream/bitmap.

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(post.File.Location);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        Stream receiveStream = response.GetResponseStream();

                        Bitmap mybitmap = new Bitmap(receiveStream);

                        #endregion

                        //If the stream has any content in it
                        if (mybitmap != null)
                        {

                            #region File Variables


                            //This will be revised
                            if (tags.Contains("score:>="))
                            {
                                Console.WriteLine("Special characters will be removed....");
                                var finaltag = tags.Replace("score:>=", "Score - BiggerThan ");
                                Console.WriteLine($"Result = {finaltag}" + "\n");
                                tags = finaltag;
                            }
                            if (tags.Contains("score:<="))
                            {
                                Console.WriteLine("Special characters will be removed....");
                                var finaltag = tags.Replace("score:<=", "Score - SmallerThan ");
                                Console.WriteLine($"Result = {finaltag}" + "\n");
                                tags = finaltag;
                            }

                            var FileExtensionID = "." + post.File.FileExtension;
                            ImageFormat Format = null;
                            var default_path = $"{SessionOutput}/{tags}";
                            if (!Directory.Exists(default_path))
                            {
                                Directory.CreateDirectory(default_path);
                                default_path = $"{SessionOutput}/{tags}/";
                            }
                            else
                            {
                                default_path = $"{SessionOutput}/{tags}/";
                            }

                            #endregion

                            //Switch for the Image Format in mybitmap.save();
                            switch ((post.File.FileExtension))
                            {

                                case "png":
                                    Format = ImageFormat.Png;
                                    break;
                                case "jpg":
                                    Format = ImageFormat.Jpeg;
                                    break;
                                case "gif":
                                    Format = ImageFormat.Gif;
                                    break;

                            }

                            Console.WriteLine($"Saving: {default_path}{post.Id}{FileExtensionID}");

                            //Saves the image
                            mybitmap.Save(default_path + post.Id + FileExtensionID, Format);

                        }

                        receiveStream.Close();
                        request.Abort();
                        response.Close();
                        e621Client.Dispose();

                    }
                }

                var check_path = $"{SessionOutput}/{tags}";

                if(Directory.GetFiles(check_path).Length < HowMany)
                {
                    Console.WriteLine("\nIt seems that not all images have been dowloaded Maybe one or more tags no declared in the research(General Post Tags)\nare in the Global Blacklist from E621, it is recommended to log-in to access said tags/content\n\nNote:This also maybe be a problem from our end since we don't support WEBM file format\nSo it maybe is not listing it for download(since we check for image-only files)\nMy Sincere Apologies - Edgar");
                }
                else
                {
                    Console.WriteLine("\nEverything downloaded, have a good... moment for yourself");
                    Console.WriteLine("See you soon, restart the app if you want to search again");
                }

                try
                {
                    // Create the file, or overwrite if the file exists.
                    using (FileStream fs = File.Create(LogPath))
                    {
                        if (Directory.GetFiles(check_path).Length < HowMany)
                        {
                            var InfoAmount = Directory.GetFiles(check_path).Length;
                            byte[] info = new UTF8Encoding(true).GetBytes($"{DateTime.Now.ToString()} - Searched for: {tags} downloaded {InfoAmount}(From the {HowMany} requested.)\nStatus = Not everything was downloaded.");
                            // Add some information to the file.
                            fs.Write(info, 0, info.Length);
                        }
                        else
                        {
                            var InfoAmount = Directory.GetFiles(check_path).Length;
                            byte[] info = new UTF8Encoding(true).GetBytes($"{DateTime.Now.ToString()} - Searched for: {tags} downloaded {InfoAmount}(From the {HowMany} requested.)\nStatus = Everything was downloaded.");
                            // Add some information to the file.
                            fs.Write(info, 0, info.Length);
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        else
        {
            Console.WriteLine("See you soon, restart the app if you want to search again");
            Environment.Exit(0);
        }

        break;

    case "pools":

        Console.WriteLine("Currently in Progress...");

        //This create a Client... this time for pools.
        var e621ClientPools = new E621ClientBuilder()
                    .WithUserAgent("PURR - An E621 CLI DOWNLOADER (WIP)", "0.1.5b", "Pervertamura", "NONE")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();
        var pools = await e621ClientPools.GetPoolsAsync(1, limit: E621Constants.PoolsMaximumLimit);

        for(int i = 0; i < pools.Count; i++)
        {
            var pool = pools.ElementAt(i);
            if (pool.Name == "Homeless Dog by Ponporio")
            {
                Console.WriteLine($"Pool Name: {pool.Name}");
                Console.WriteLine($"Pool ID: {pool.Id}");
                Console.WriteLine($"Pool Description: {pool.Description}");
                Console.WriteLine($"Last Updated: {pool.UpdatedAt}");
                Console.WriteLine($"Pages IDs: ");
                for (int p = 0; p < pool.PostIds.Count; p++)
                {
                    Console.WriteLine(pool.PostIds.ElementAt(p));
                }
            }

        }

        break;
}
