using System;
using System.Collections.Generic;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.SNSEvents;
using Amazon.S3.Util;
using SingleEvent;
using Xunit;

namespace BulkEvents.Tests
{
  public class FunctionTest
  {
        [Fact(Skip = "Useful for local development")]
        public void CanReadS3Object()
        {
            var sut = new BulkEventsLambda();

            var s3event = new S3Event
            {
                Records = new List<S3EventNotification.S3EventNotificationRecord>
                {
                    new S3EventNotification.S3EventNotificationRecord()
                    {
                        S3 = new S3EventNotification.S3Entity()
                        {
                            Bucket =
                                new S3EventNotification.S3BucketEntity()
                                {
                                    Name = "data-pipeline-alper"
                                },
                            Object =
                                new S3EventNotification.S3ObjectEntity()
                                {
                                    Key = "sampledata.json"
                                }
                        }
                    }
                }
            };




            Environment.SetEnvironmentVariable("AWS_PROFILE", "personal");
            Environment.SetEnvironmentVariable("FAN_OUT_TOPIC", $"arn:aws:sns:<REGION>:<ACCOUNT#>:FAN_OUT_TOPIC");
            sut.S3EventHandler(s3event);
        }

        //What exactly are we testing here?
        //[Fact]
        //public void CanReadSnsEvent()
        //{
        //    var snsEvent = new SNSEvent();
        //    snsEvent.Records = new List<SNSEvent.SNSRecord>()
        //    {
        //        new SNSEvent.SNSRecord()
        //        {
        //            Sns = new SNSEvent.SNSMessage() { Message = "	\t{\n\t\t\"locationName\":\"Brooklyn, NY\", \n\t\t\"temperature\":91, \n\t\t\"timestamp\":1564428897, \n\t\t\"latitude\": 40.70, \n\t\t\"longitude\": -73.99\n\t}"}
        //        }
        //    };

        //    Environment.SetEnvironmentVariable("AWS_PROFILE", "personal");

        //    var sut = new SingleEventLambda();
        //    sut.SnsEventHandler(snsEvent);

        //}
    }
}