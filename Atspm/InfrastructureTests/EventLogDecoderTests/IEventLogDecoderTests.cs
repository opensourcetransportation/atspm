#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.EventLogDecoderTests/IEventLogDecoderTests.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders;
using Utah.Udot.NetStandardToolkit.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.EventLogDecoderTests
{
    public class EventLogDecoderTestData<T> where T : EventLogModelBase
    {

        byte[] EventLogs { get; set; }
        bool IsCompressed { get; set; }
        List<T> Events { get; set; }
    }

    public readonly struct AtspmFileSignatures
    {
        public static FileSignature ASC3 => new FileSignature(null, 0, ".dat", "Data file of Indianna Events", false);
        public static FileSignature ASC3Compressed => new FileSignature(null, 0, ".datZ", "Data file of Indianna Events", true);
    }

    public class IEventLogDecoderTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        //private IEventLogDecoder _decoder;
        //private ILogger _nullLogger;
        //private IOptions<SignalControllerDecoderConfiguration> _nullOptions;

        public IEventLogDecoderTests(ITestOutputHelper output)
        {
            _output = output;
            //_nullLogger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<StubLocationControllerDecoder>();
            //_nullOptions = Options.Create(new SignalControllerDecoderConfiguration() { EarliestAcceptableDate = new DateTime() });
            //_decoder = new StubLocationControllerDecoder((ILogger<StubLocationControllerDecoder>)_nullLogger, _nullOptions);

            //_output.WriteLine($"Created IEventLogDecoder Instance: {_decoder.GetHashCode()}");
        }

        #region IEventLogDecoder

        //#region IEventLogDecoder.IsCompressed




        [Fact]
        public void DecodesMaxTimeEventsWithLocationAndTimestamp()
        {
            const string xml = """
                <EventResponses><EventResponse>
                  <Event ID="1" TimeStamp="2024-06-23T22:47:07" EventTypeID="81" Parameter="41" />
                  <Event ID="2" TimeStamp="2024-06-23T22:47:12" EventTypeID="82" Parameter="47" />
                </EventResponse></EventResponses>
                """;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var device = new Device { Location = new Location { LocationIdentifier = "test-location" } };

            var events = new MaxtimeToIndianaDecoder().Decode(device, stream).ToList();

            Assert.Equal(2, events.Count);
            Assert.All(events, e => Assert.Equal("test-location", e.LocationIdentifier));
            Assert.Contains(events, e => e.EventCode == 81 && e.EventParam == 41 && e.Timestamp == new DateTime(2024, 6, 23, 22, 47, 7));
            Assert.Contains(events, e => e.EventCode == 82 && e.EventParam == 47 && e.Timestamp == new DateTime(2024, 6, 23, 22, 47, 12));

            //var file = new FileInfo(Path.Combine(TestDataPath, "638548149067839806.xml"));
            //var device = new Device()
            //{
            //    Location = new Location() { LocationIdentifier = "1234" }
            //};

            //var s = file.ToMemoryStream();

            //IEventLogDecoder sut = new MaxTimeEventLogDecoder();

            //var iscompress = sut.IsCompressed(s);

            //var result = sut.Decode(device, s);

        }






        //[Fact]
        //public void IEventLogDecoderIsCompressed()
        //{
        //    var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDecoderConfiguration>>();
        //    var file = new FileInfo(Path.Combine(TestDataPath, "4895_ECON_10.210.8.179_2024_02_21_1115.dat"));
        //    var data = file.ToMemoryStream();

        //    IEventLogDecoder<IndianaEvent> sut = new ASCEventLogDecoder(new NullLogger<ASCEventLogDecoder>(), mockConfig);

        //    var condition = sut.IsCompressed(data);

        //    Assert.False(condition);
        //}

        //#endregion

        //#region IEventLogDecoder.IsEncoded

        //[Fact]
        //public void IEventLogDecoderIsEncoded()
        //{
        //    var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDecoderConfiguration>>();
        //    var file = new FileInfo(Path.Combine(TestDataPath, "4895_ECON_10.210.8.179_2024_02_21_1115.dat"));
        //    var data = file.ToMemoryStream();

        //    IEventLogDecoder<IndianaEvent> sut = new ASCEventLogDecoder(new NullLogger<ASCEventLogDecoder>(), mockConfig);

        //    var condition = sut.IsEncoded(data);

        //    Assert.True(condition);
        //}

        //#endregion

        //#region IEventLogDecoder.Decompress

        //[Fact]
        //public void IEventLogDecoderDecompress()
        //{
        //    var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDecoderConfiguration>>();
        //    var file = new FileInfo(Path.Combine(TestDataPath, "1210_ECON_10.204.7.239_2021_08_09_1841.datZ"));
        //    var data = file.ToMemoryStream();

        //    IEventLogDecoder<IndianaEvent> sut = new ASCEventLogDecoder(new NullLogger<ASCEventLogDecoder>(), mockConfig);

        //    var d = sut.Decompress(data);

        //    var condition = d.IsCompressed();

        //    Assert.False(condition);
        //}

        //[Theory]
        //[EncodedControllerTestFiles]
        //public void Decompress(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    if (isCompressed)
        //    {
        //        var decompressed =_decoder.Decompress(fileInfo.ToMemoryStream());

        //        var condition = decompressed.IsCompressed();
        //        _output.WriteLine($"Condition: {condition}");

        //        Assert.False(condition);
        //    }
        //    else
        //    {
        //        Assert.False(isCompressed);
        //    }
        //}

        //#endregion

        //#region IEventLogDecoder.Decode

        //[Fact]
        //public void IEventLogDecoderDecodePass()
        //{
        //    var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDecoderConfiguration>>();
        //    var file = new FileInfo(Path.Combine(TestDataPath, "4895_ECON_10.210.8.179_2024_02_21_1115.dat"));
        //    var data = file.ToMemoryStream();
        //    var device = new Device()
        //    {
        //        Location = new Location()
        //        {
        //            LocationIdentifier = "1001"
        //        }
        //    };

        //    IEventLogDecoder<IndianaEvent> sut = new ASCEventLogDecoder(new NullLogger<ASCEventLogDecoder>(), mockConfig);

        //    var stuff = sut.Decode(device, data);


        //}

        //[Theory]
        //[EncodedControllerTestFiles]
        //public async void DecodeAsync(FileInfo fileInfo, bool isCompressed, bool isEncoded)
        //{
        //    if (_decoder.CanExecute(fileInfo))
        //    {
        //        var logs = await _decoder.DecodeAsync("0", fileInfo.ToMemoryStream());

        //        var condition = logs.Count > 0;

        //        Assert.True(condition);
        //    }
        //    else
        //    {
        //        Assert.False(true);
        //    }
        //}

        //[Fact]
        //public async Task DecodeNotValidFromNullData()
        //{
        //    var memoryStream = new MemoryStream();

        //    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _decoder.DecodeAsync(string.Empty, memoryStream));
        //}

        //[Fact]
        //public async Task DecodeNotValidFromInvalidData()
        //{
        //    var memoryStream = new MemoryStream();

        //    await Assert.ThrowsAnyAsync<InvalidDataException>(() => _decoder.DecodeAsync("0", memoryStream));
        //}

        //[Fact]
        //public async Task DecodeNotValidFromBadData()
        //{
        //    //byte[] badData = Encoding.UTF8.GetBytes(string.Join(",", Enumerable.Range(1, 100).Select(i => i.ToString())));

        //    //var memoryStream = new MemoryStream(badData);

        //    var test = await _decoder.DecodeAsync("0", new MemoryStream());

        //    _output.WriteLine($"test: {test.Count}");

        //    //await Assert.ThrowsAnyAsync<InvalidDataException>(() => _decoder.DecodeAsync("0", memoryStream));
        //    Assert.True(false);
        //}

        //[Fact]
        //public async Task DecodeWithProgress()
        //{
        //    //FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath, "1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

        //    int actual = 0;
        //    var progress = new Progress<int>(i => actual = i);

        //    var collection = await _decoder.DecodeAsync("0", new MemoryStream(), progress);
        //    var expected = collection.Count;

        //    Assert.Equal(expected, actual);
        //}

        //[Fact]
        //public async Task DecodeWithTokenCancelled()
        //{
        //    CancellationTokenSource cts = new CancellationTokenSource();
        //    cts.Cancel();

        //    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _decoder.DecodeAsync("0", new MemoryStream(), null, cts.Token));
        //}

        //#endregion

        #region ILocationControllerDecoder.ExecuteAsync





        //[Fact]
        //public void IEventLogDecoderCanExecutePass()
        //{
        //    var mockConfig = Mock.Of<IOptionsSnapshot<SignalControllerDecoderConfiguration>>();
        //    var file = new FileInfo(Path.Combine(TestDataPath, "4895_ECON_10.210.8.179_2024_02_21_1115.dat"));
        //    //var data = file.ToMemoryStream();

        //    var device = new Device()
        //    {
        //        DeviceConfiguration = new DeviceConfiguration()
        //        {
        //            Decoders = typeof(IndianaEvent)
        //        }
        //    };

        //    IEventLogDecoder<IndianaEvent> sut = new ASCEventLogDecoder(new NullLogger<ASCEventLogDecoder>(), mockConfig);

        //    var condition = sut.CanExecute(Tuple.Create(device, file));

        //    Assert.True(condition);
        //}








        //[Fact]
        //public async Task ExecuteAsyncWithNullFileInfoParameter()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>()).Object;

        //    await Assert.ThrowsAnyAsync<ArgumentNullException>(() => sut.ExecuteAsync(null, null));
        //}

        //[Fact]
        //public async Task ExecuteAsyncWithInvalidFileInfoParameter()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>()).Object;

        //    await Assert.ThrowsAnyAsync<FileNotFoundException>(() => sut.ExecuteAsync(new FileInfo("C:\\invalid.txt"), null));
        //}

        //[Fact]
        //public async Task ExecuteAsyncWithFailedCanExecute()
        //{
        //    var sut = new Mock<StubLocationControllerDecoder>(Mock.Of<ILogger<StubLocationControllerDecoder>>(), Mock.Of<IOptions<SignalControllerDecoderConfiguration>>());

        //    sut.Setup(m => m.CanExecute(It.IsAny<FileInfo>())).Returns(false);

        //    await Assert.ThrowsAnyAsync<ExecuteException>(() => sut.Object.ExecuteAsync(new FileInfo(Path.GetTempFileName()), null));
        //}

        #endregion

        /*

        

        #endregion

        #region IExecuteAsyncWithProgress

        #region IExecuteAsyncWithProgress.CanExecute

        

        [Fact]
        public void CanExecuteValidFromDat()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteValidFromDatz()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1210(datz)\\ECON_10.204.7.239_2021_08_09_1841.datZ"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.True(condition);
        }

        [Fact]
        public void CanExecuteNotValid()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1210(datz)\\ECON_10.204.12.179_2021_08_09_1831.bad"));

            var condition = _decoder.CanExecute(fileInfo);

            Assert.False(condition);
        }

        #endregion

        #region IExecuteAsyncWithProgress.ExecuteAsync

        [Theory]
        [EncodedControllerTestFiles]
        public void ExecuteAsyncValidFromDat(FileInfo fileInfo, string expected)
        {
            var collection = _decoder.ExecuteAsync(fileInfo).Result;
            var condition = collection?.Count > 0;

            Assert.True(condition);
            Assert.All(collection, l => Assert.Equal(expected, l.LocationId));
        }

        [Fact]
        public void ExecuteAsyncTokenCancelled()
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(TestDataPath,"1053(dat)\\ECON_10.204.12.167_2021_08_09_1831.dat"));

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            var expected = TaskStatus.Canceled;
            TaskStatus actual = _decoder.ExecuteAsync(fileInfo, cts.Token).Status;

            _output.WriteLine($"expected: {expected}");
            _output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [EncodedControllerTestFiles]
        public void ExecuteAsyncWithProgress(FileInfo fileInfo, string expected)
        {
            int actual = 0;
            var progress = new Progress<int>(i => actual = i);

            var collection = _decoder.ExecuteAsync(fileInfo, progress: progress).Result;
            var realExpected = collection.Count;

            Assert.Equal(realExpected, actual);
        }

        #endregion

        */

        #endregion

        public void Dispose()
        {
            //_output.WriteLine($"Disposing IEventLogDecoder Instance: {_decoder.GetHashCode()}");
        }
    }
}
