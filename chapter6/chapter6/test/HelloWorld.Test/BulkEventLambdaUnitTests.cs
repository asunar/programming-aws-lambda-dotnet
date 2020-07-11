using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Lambda.S3Events;
using Amazon.S3.Model;
using Amazon.S3.Util;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace BulkEvents.Tests
{
    public class BulkEventLambdaUnitTests
    {
        [Fact]
        public void ShouldReadWeatherEvents()
        {
            var bulkEventsLambda = new BulkEventsLambda(null, null, "dummyTopic");
            var mockS3Response = new GetObjectResponse
            {
                ResponseStream = File.OpenRead("../../../../../sampledata.json")
            };

            var weatherEvents = bulkEventsLambda.ReadWeatherEvents(mockS3Response);

            weatherEvents[0].LocationName.Should().Be("New York, NY");
            weatherEvents[0].Temperature.Should().Be(91);
            weatherEvents[0].Timestamp.Should().Be(1564428897);
            weatherEvents[0].Latitude.Should().Be(40.70);
            weatherEvents[0].Longitude.Should().Be(-73.99);



        }

        [Fact]
        public void ShouldThrowWithBadData()
        {
            var bulkEventsLambda = new BulkEventsLambda(null, null, "dummyTopic");
            var mockS3Response = new GetObjectResponse
            {
                ResponseStream = File.OpenRead("../../../../../baddata.json")
            };

            var lambda = new BulkEventsLambda(null, null, "dummy");

           Action act = () => lambda.ReadWeatherEvents(mockS3Response);
           act.Should().Throw<System.Text.Json.JsonException>();
        }

    }
}