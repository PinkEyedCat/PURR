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

var state = "run";

//Salutations and question of what are the user Seaching for
Console.WriteLine("How do you do Fellow Degenerate?");
Console.WriteLine("What are you searching today? - Pools? Posts? we have it my friend. - Type below which one" + "\n");
var topic = Console.ReadLine();


switch (topic.ToLower())
{

    case "single_posts":

        //This checks for the Category(Posts/Pools) directory, if does not exists, creates and sets it.
        if (Directory.Exists($"{SessionOutput}/Single Posts"))
        {
            Directory.CreateDirectory($"{SessionOutput}/Single Posts");
            SessionOutput = $"{SessionOutput}/Single Posts";
        }
        else
        {
            SessionOutput = $"{SessionOutput}/Single Posts";
        }

        //This will take the User's topic and ask for which Tags and for How Many posts to search for
        Console.WriteLine($"\nSo... {topic} huh?");
        Console.WriteLine("What ya having? - Type what you search below UwU" + "\n");
        var ID = Int32.Parse(Console.ReadLine());

        //This will ask for a confirmation of the User's Option
        Console.WriteLine($"\nIDs to search = {ID}");
        Console.WriteLine("Confirm?(Yes/No)" + "\n");
        var confirmSingle = Console.ReadLine();

        if (confirmSingle.ToLower() == "yes")
        {

            if (ID != null)
            {

                var e621ClientSingle = new E621ClientBuilder()
                    .WithUserAgent("PURR - An E621 CLI DOWNLOADER (WIP)", "0.1.5b", "Pervertamura", "NONE")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();

                var post = await e621ClientSingle.GetPostAsync(ID);

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

                                var FileExtensionID = "." + post.File.FileExtension;
                                ImageFormat Format = null;
                                var default_path = $"{SessionOutput}/";
                                if (!Directory.Exists(default_path))
                                {
                                    Directory.CreateDirectory(default_path);
                                    default_path = $"{SessionOutput}/";
                                }
                                else
                                {
                                    default_path = $"{SessionOutput}/";
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
                        e621ClientSingle.Dispose();

                    }
                }

                //This part will define which action the system should take.
                Console.WriteLine("\nWhat you want to do now??\n");
                var following = Console.ReadLine().ToLower();

                switch (following)
                {
                    case "close":

                        try
                        {
                            // Create the file, or overwrite if the file exists.
                            using (FileStream fs = File.Create(LogPath))
                            {

                                byte[] info = new UTF8Encoding(true).GetBytes($"{DateTime.Now.ToString()} - File {ID} downloaded with no problem");
                                        // Add some information to the file.
                                fs.Write(info, 0, info.Length);
                            }
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                        Console.WriteLine("See you soon, restart the app if you want to search again");

                        break;
                    case "restart":
                        //Searches for the executable path and start it
                        System.Diagnostics.Process.Start(Environment.ProcessPath);

                        // Closes the current process
                        Environment.Exit(0);
                        break;

                }

            }
        else
        {
            Console.WriteLine("See you soon, restart the app if you want to search again");
            Environment.Exit(0);
        }

        break;

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
                    Console.WriteLine($"\nUnable to LOG-IN - System Returned: {success}" + "\n");
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


                //This part will define which action the system should take.
                Console.WriteLine("\nWhat you want to do now??\n");
                var following = Console.ReadLine().ToLower();

                switch (following)
                {
                    case "close":

                        var check_path = $"{SessionOutput}/{tags}";

                        if (Directory.GetFiles(check_path).Length < HowMany)
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
                        break;
                    case "restart":
                        //Searches for the executable path and start it
                        System.Diagnostics.Process.Start(Environment.ProcessPath);

                        // Closes the current process
                        Environment.Exit(0);
                        break;

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
        int page = 1;

        //This checks for the Category(Posts/Pools) directory, if does not exists, creates and sets it.
        if (Directory.Exists($"{SessionOutput}/Pools"))
        {
            Directory.CreateDirectory($"{SessionOutput}/Pools");
            SessionOutput = $"{SessionOutput}/Pools";
            Console.WriteLine($"Pools are being downloaded to: {SessionOutput}");
        }
        else
        {
            SessionOutput = $"{SessionOutput}/Pools";
            Console.WriteLine($"Pools are being downloaded to: {SessionOutput}");
        }

        //This create a Client... this time for pools.
        var e621ClientPools = new E621ClientBuilder()
                    .WithUserAgent("PURR - An E621 CLI DOWNLOADER (WIP)", "0.1.5b", "Pervertamura", "NONE")
                    .WithMaximumConnections(E621Constants.MaximumConnectionsLimit)
                    .WithRequestInterval(E621Constants.MinimumRequestInterval)
                    .Build();

        //This retrieves the first page of the Pools Section (by default "page" is 1)
        var pools = await e621ClientPools.GetPoolsAsync(page, limit: E621Constants.PoolsMaximumLimit);

        //This is a pretty neat loop, while the the player don't declare "end" for the loop state the system will run on that for ever
        //Maybe will try to implement this in the Posts sections.... seems more effective code-wise.
        while (state != "end")
        {
            if (state == "run")
            {
                Console.WriteLine("Retrieving Pools....\n\n");
                for (int i = 0; i < pools.Count; i++)
                {
                    var pool = pools.ElementAt(i);
                    Console.WriteLine(pool.Name + "\n");
                    state = "pause";
                }
            }
            else
            {
                Console.WriteLine("\n\nWhat now?");
                var action = Console.ReadLine().ToLower();

                switch (action)
                {
                    case "p.increase":
                        page++;
                        pools = await e621ClientPools.GetPoolsAsync(page, limit: E621Constants.PoolsMaximumLimit);
                        state = "run";
                        break;
                    case "retrieve":
                        var name = Console.ReadLine();
                        for (int i = 0; i < pools.Count; i++)
                        {
                            var pool = pools.ElementAt(i);
                            if (name == pool.Name)
                            {
                                Console.WriteLine($"Name: {pool.Name}");
                                Console.WriteLine($"ID: {pool.Id}");
                                Console.WriteLine($"Description: {pool.Description}");
                                Console.WriteLine($"Last Updated At: {pool.UpdatedAt}");
                            }
                        }
                        state = "pause";
                        break;
                    case "r.download":
                        var nametdl = Console.ReadLine();

                        for (int p = 0; p < pools.Count; p++)
                        {
                            var pool = pools.ElementAt(p);
                            if (nametdl == pool.Name)
                            {
                                var pgnmb = pool.PostIds.Count;
                                Console.WriteLine($"Pages to download: {pgnmb}");
                                for (int pg = 0; pg < pool.PostIds.Count; pg++)
                                {
                                    var post = await e621ClientPools.GetPostAsync(pool.PostIds.ElementAt(pg));

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

                                                var FileExtensionID = "." + post.File.FileExtension;
                                                ImageFormat Format = null;
                                                var default_path = $"{SessionOutput}/{pool.Name}/";
                                                if (!Directory.Exists(default_path))
                                                {
                                                    Directory.CreateDirectory(default_path);
                                                    default_path = $"{SessionOutput}/{pool.Name}/";
                                                }
                                                else
                                                {
                                                    default_path = $"{SessionOutput}/{pool.Name}/";
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
                                    }
                                }
                            }
                        }
                        
                        state = "pause";
                        break;
                    case "end":
                        Console.WriteLine("Bye Bye.");
                        state = "end";
                        break;
                }
            }
        }
        break;
}
