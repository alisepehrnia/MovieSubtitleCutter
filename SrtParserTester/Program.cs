var path = @"G:\Video\About Time (2013) [1080p]\About.Time.2013.1080p.BluRay.x264.YIFY.srt";
var fstream = new FileStream(path, FileMode.Open);
var subItems = SrtParser.SrtParser.ParseStream(fstream, System.Text.Encoding.UTF8);

Console.ReadKey();