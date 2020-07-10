using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.SimpleNotificationService;
using DataPipeline.Common;

[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace BulkEvents
{
    
    public class BulkEventsLambda
    {
        private static readonly AmazonSimpleNotificationServiceClient sns =
            new AmazonSimpleNotificationServiceClient();

        private static readonly AmazonS3Client s3 = new AmazonS3Client();

        private static string SNS_TOPIC =
            Environment.GetEnvironmentVariable("FAN_OUT_TOPIC");

        public void S3EventHandler(S3Event s3Event)
        {
            s3Event.Records.ForEach(ProcessS3EventRecord);
        }

        private void ProcessS3EventRecord(S3EventNotification.S3EventNotificationRecord s3Record)
        {
            var weatherEvents =
                ReadObjectDataAsync(s3Record.S3.Bucket.Name, s3Record.S3.Object.Key);

            var topicArn = $"{SNS_TOPIC}";

            try
            {
                foreach (var weatherEvent in weatherEvents)
                {
                    var response = sns.PublishAsync(topicArn,JsonSerializer.Serialize(weatherEvent)).Result;
                    if(response.HttpStatusCode != HttpStatusCode.OK) Console.WriteLine($"Failed to publish weather event for {weatherEvent.LocationName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception publishing request");
                throw ex;
            }

            Console.WriteLine($"Published {weatherEvents.Count} weather events to SNS");
        }

        private static List<WeatherEvent> ReadObjectDataAsync(string bucketName, string keyName)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };
                using (GetObjectResponse response = s3.GetObjectAsync(request).Result)
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    return JsonSerializer.Deserialize<List<WeatherEvent>>(reader.ReadToEnd(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true});
                }
            }
            catch (AmazonS3Exception e)
            {
                // If bucket or object does not exist
                Console.WriteLine("Error encountered ***. Message:'{0}' when reading object", e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when reading object", e.Message);
                throw e;
            }
        }
    }
}
