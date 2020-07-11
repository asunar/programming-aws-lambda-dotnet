using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.SimpleNotificationService;
using FakeItEasy;
using Xunit;

namespace BulkEvents.Tests
{
    public class BulkEventLambdaFunctionalTests
    {
        [Fact]
        public void ShouldHandleBulkWeatherEvents()
        {
            var fakeSns = A.Fake<AmazonSimpleNotificationServiceClient>();
            var fakeS3 = A.Fake<AmazonS3Client>();

            var s3Event = new S3Event()
            {
                Records = new List<S3EventNotification.S3EventNotificationRecord>
                {
                    new S3EventNotification.S3EventNotificationRecord
                    {
                        S3 = new S3EventNotification.S3Entity
                        {
                            Bucket = new S3EventNotification.S3BucketEntity
                            {
                                Name = "fakeBucket"
                            },
                            Object = new S3EventNotification.S3ObjectEntity
                            {
                                Key = "fakeKey"
                            }
                        }
                    }
                }
            };


            var s3GetObjectRequest = new GetObjectRequest()
            {
                BucketName = "fakeBucket",
                Key = "fakeKey"
            };

            var s3ObjectResponse = new GetObjectResponse
            {
                ResponseStream = File.OpenRead("../../../../../sampledata.json")
            };
            A.CallTo(() => 
                fakeS3.GetObjectAsync(A<GetObjectRequest>.That.Matches(x => x.BucketName == s3GetObjectRequest.BucketName && x.Key == s3GetObjectRequest.Key), A<CancellationToken>._))
                .Returns(Task.FromResult(s3ObjectResponse));

            var topic = "test-topic";

            var bulkEventsLambda = new BulkEventsLambda(fakeS3, fakeSns, topic);
            bulkEventsLambda.S3EventHandler(s3Event);

            A.CallTo(() =>
                    fakeSns.PublishAsync(topic, A<string>._, CancellationToken.None))
                .MustHaveHappened(3, Times.Exactly);

        }
    }
}