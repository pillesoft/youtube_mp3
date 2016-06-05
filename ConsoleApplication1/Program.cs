using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeExtractor;
using System.IO;

namespace ConsoleApplication1
{
  class Program
  {
    static void Main(string[] args)
    {
      string inputfile = args[0];
      string links;
      using (TextReader tr = File.OpenText(inputfile))
      {
        links = tr.ReadToEnd();
      }

      string mp3path = app.Default.mp3path;
      string ffmpegexe = Path.Combine(app.Default.ffmpegpath, "ffmpeg.exe");
      Dictionary<char, char> charsmap = new Dictionary<char, char>();
      charsmap['Á'] = 'A';
      charsmap['É'] = 'E';
      charsmap['Ő'] = 'O';
      charsmap['Ú'] = 'U';
      charsmap['Ű'] = 'U';
      charsmap['Ó'] = 'O';
      charsmap['Ü'] = 'U';

      charsmap['á'] = 'a';
      charsmap['é'] = 'e';
      charsmap['ő'] = 'o';
      charsmap['ú'] = 'u';
      charsmap['ű'] = 'u';
      charsmap['ó'] = 'o';
      charsmap['ü'] = 'u';
      charsmap['í'] = 'i';
      charsmap['ö'] = 'o';
      
      charsmap[':'] = '_';
      charsmap['/'] = '_';
      charsmap['?'] = '_';

      foreach (string link in links.Split(new string[] {System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
      {
        Console.WriteLine(link);

        //string link = "http://www.youtube.com/watch?v=M2wEebUnUiA";
        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

        /*
        * We want the first extractable video with the highest audio quality.
        */
        VideoInfo video = videoInfos
            .Where(info => info.CanExtractAudio)
            .OrderByDescending(info => info.AudioBitrate)
            .First();

        string title = video.Title;
        foreach (var item in charsmap.Keys) {
          title = title.Replace(item, charsmap[item]);
        }
        string audiofilename = System.IO.Path.Combine(mp3path, title + video.AudioExtension);
        Console.WriteLine(audiofilename);

        DownloadAudio(video, audiofilename);
        //audiofilename = @"c:\Users\Ivan\Music\Cserháti&Charlie - Száguldás, Porsche....aac";

        // if the audio is aac we have to convert it to mp3
        if (video.AudioType == AudioType.Aac)
        {
          string mp3filename = System.IO.Path.Combine(mp3path, video.Title + ".mp3");
          string arguments = " -i \"" + audiofilename + "\" -ab 192k \"" + mp3filename + "\"";
          System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
          psi.FileName = ffmpegexe;
          psi.Arguments = arguments;
          psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
          System.Diagnostics.Process p = System.Diagnostics.Process.Start(psi);

        }

        string sa = "sdfg";
      }
    }

    private static void DownloadAudio(VideoInfo video, string filename)
    {


      /*
       * Create the audio downloader.
       * The first argument is the video where the audio should be extracted from.
       * The second argument is the path to save the audio file.
       */
      var audioDownloader = new AudioDownloader(video, filename);

      // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
      // because the download will take much longer than the audio extraction.
      audioDownloader.DownloadProgressChanged += (sender, args) => Console.Write('.');
      audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.Write('.');

      /*
       * Execute the audio downloader.
       * For GUI applications note, that this method runs synchronously.
       */
      audioDownloader.Execute();
    }

  }
}
