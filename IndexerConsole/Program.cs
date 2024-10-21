using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using SrtParser;

//var path = @"G:\Video\About Time (2013) [1080p]\About.Time.2013.1080p.BluRay.x264.YIFY.srt";
//var fstream = new FileStream(path, FileMode.Open);
//var subItems = SrtParser.SrtParser.ParseStream(fstream, System.Text.Encoding.UTF8);

var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    //.CertificateFingerprint("<FINGERPRINT>")
    .Authentication(new BasicAuthentication("elastic", "loobley@3129"));
//.DefaultIndex("customer");

var client = new ElasticsearchClient(settings);

//var x = await client.IndexManyAsync(subItems, "subtitles");

var x = await client.SearchAsync<SubtitleItem>(s => s
   .Index("subtitles")
   .Query(q => q
    .Term(t => t.Field(f => f.PlaintextLines).Value("بشین"))
    ));

await client.DeleteByQueryAsync<object>(q => q
          .Index("IndexName")
          .Query(rq => rq.MatchAll())
      );

client.DeleteByQuery<SubtitleItem>(del => del.Query(q => q.QueryString(qs => qs.Query("*")))

Console.ReadKey();