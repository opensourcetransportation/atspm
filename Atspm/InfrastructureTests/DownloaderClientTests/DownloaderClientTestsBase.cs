#region license
// Copyright 2026 Utah Departement of Transportation
// for InfrastructureTests - Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests/DownloaderClientTestsBase.cs
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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Exceptions;
using Utah.Udot.Atspm.Services;
using Xunit;
using Xunit.Abstractions;

namespace Utah.Udot.Atspm.InfrastructureTests.DownloaderClientTests
{
    public abstract class DownloaderClientTestsBase<T> : IDisposable where T : class
    {
        protected ITestOutputHelper Output;
        protected IDownloaderClient Sut;
        protected T Client;

        public DownloaderClientTestsBase(ITestOutputHelper output)
        {
            Output = output;
        }

        #region Connect

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncSucceeded()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            await Sut.ConnectAsync(connection, credentials);

            var condition = Sut.IsConnected;

            Assert.True(condition);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncNullConnection()
        {
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            var ex = await Record.ExceptionAsync(async () => await Sut.ConnectAsync(null, credentials));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientConnectionException>(ex);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncNullCredentials()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);

            var ex = await Record.ExceptionAsync(async () => await Sut.ConnectAsync(connection, null));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientConnectionException>(ex);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncControllerConnectionException()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.ConnectAsync(connection, credentials));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncOperationCanceledException()
        {
            var connection = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var credentials = new NetworkCredential("user", "password", "127.0.0.1");

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await Sut.ConnectAsync(connection, credentials, 2000, 2000, null, tokenSource.Token));
        }

        #endregion

        #region DeleteResource

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public abstract Task DeleteResourceAsyncSucceeded();

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public async virtual Task DeleteResourceAsyncNullResource()
        {
            var ex = await Record.ExceptionAsync(async () => await Sut.DeleteResourceAsync(null));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientDeleteResourceException>(ex);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public async virtual Task DeleteResourceAsyncInvalidResource()
        {
            var uri = new Uri("a/b/c", UriKind.Relative);

            var ex = await Record.ExceptionAsync(async () => await Sut.DeleteResourceAsync(uri));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientDeleteResourceException>(ex);
            Assert.IsType<UriFormatException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public async virtual Task DeleteResourceAsyncNotConnected()
        {
            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.DeleteResourceAsync(null));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public async virtual Task DeleteResourceAsyncControllerDeleteResourceException()
        {
            await Assert.ThrowsAsync<DownloaderClientDeleteResourceException>(async () => await Sut.DeleteResourceAsync(null));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DeleteResourceAsync))]
        public async virtual Task DeleteResourceAsyncOperationCanceledException()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await Sut.DeleteResourceAsync(null, tokenSource.Token));
        }

        #endregion

        #region Disconnect

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DisconnectAsync))]
        public async virtual Task DisconnectAsyncSucceeded()
        {
            await Sut.DisconnectAsync();

            var condition = Sut.IsConnected;

            Assert.False(condition);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DisconnectAsync))]
        public async virtual Task DisconnectAsyncNotConnected()
        {
            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.DisconnectAsync());
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DisconnectAsync))]
        public async virtual Task DisconnectAsyncControllerConnectionException()
        {
            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.DisconnectAsync());
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DisconnectAsync))]
        public async virtual Task DisconnectAsyncOperationCanceledException()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await Sut.DisconnectAsync(tokenSource.Token));
        }

        #endregion

        #region DownloadResource

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncSucceed()
        {
            var local = new UriBuilder()
            {
                Scheme = Uri.UriSchemeFile,
                Path = Path.GetTempFileName()
            }.Uri;

            var remote = new UriBuilder().Uri;

            var condition = await Sut.DownloadResourceAsync(local, remote);

            Assert.True(condition is FileInfo);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncNullLocal()
        {
            var remote = new UriBuilder().Uri;

            var ex = await Record.ExceptionAsync(async () => await Sut.DownloadResourceAsync(null, remote));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientDownloadResourceException>(ex);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncNullRemote()
        {
            var local = new UriBuilder()
            {
                Scheme = Uri.UriSchemeFile,
            }.Uri;

            var ex = await Record.ExceptionAsync(async () => await Sut.DownloadResourceAsync(local, null));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientDownloadResourceException>(ex);
            Assert.IsType<ArgumentNullException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncInvalidLocal()
        {
            var local = new UriBuilder().Uri;

            var remote = new UriBuilder().Uri;

            var ex = await Record.ExceptionAsync(async () => await Sut.DownloadResourceAsync(local, remote));

            Assert.NotNull(ex);
            Assert.IsType<DownloaderClientDownloadResourceException>(ex);
            Assert.IsType<UriFormatException>(ex.InnerException);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncNotConnected()
        {
            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.DownloadResourceAsync(null, null));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncDownloaderClientDownloadResourceException()
        {
            var local = new UriBuilder()
            {
                Scheme = Uri.UriSchemeFile,
            }.Uri;

            var remote = new UriBuilder().Uri;

            await Assert.ThrowsAsync<DownloaderClientDownloadResourceException>(async () => await Sut.DownloadResourceAsync(local, remote));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.DownloadResourceAsync))]
        public async virtual Task DownloadResourceAsyncOperationCanceledException()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await Sut.DownloadResourceAsync(null, null, tokenSource.Token));
        }

        #endregion

        #region ListResources

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ListResourcesAsync))]
        public async virtual Task ListResourcesAsyncSucceeded()
        {
            var expected = "/a/b/c";

            var result = await Sut.ListResourcesAsync(expected);

            foreach (var r in result)
            {
                Output.WriteLine($"result: {r}");
            }

            var actual = result.FirstOrDefault().PathAndQuery;

            Output.WriteLine($"expected: {expected}");
            Output.WriteLine($"actual: {actual}");

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ListResourcesAsync))]
        public async virtual Task ListResourcesAsyncNotConnected()
        {
            await Assert.ThrowsAsync<DownloaderClientConnectionException>(async () => await Sut.ListResourcesAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ListResourcesAsync))]
        public async virtual Task ListDirectoryAsyncControllerDownloadFileException()
        {
            await Assert.ThrowsAsync<DownloaderClientListResourcesException>(async () => await Sut.ListResourcesAsync(""));
        }

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ListResourcesAsync))]
        public async virtual Task ListResourcesAsyncDownloaderClientListResourcesException()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => await Sut.ListResourcesAsync("", tokenSource.Token));
        }

        #endregion

        [Fact]
        [Trait(nameof(IDownloaderClient), nameof(IDownloaderClient.ConnectAsync))]
        public async virtual Task ConnectAsyncConnectionProperties()
        {
            var ipAddress = "127.0.0.1";
            var port = Random.Shared.Next(1, 10);
            var userName = new Guid().ToString();
            var password = new Guid().ToString();
            var connectionTimeout = Random.Shared.Next(1000, 10000);
            var operationTImeout = Random.Shared.Next(1000, 10000);

            var connection = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var credentials = new NetworkCredential(userName, password, ipAddress);

            await Sut.ConnectAsync(connection, credentials, connectionTimeout, operationTImeout);

            Output.WriteLine($"VerifyIpAddress: {ipAddress}");
            Output.WriteLine($"VerifyPort: {port}");
            Output.WriteLine($"VerifyUserName: {userName}");
            Output.WriteLine($"VerifyPassword: {password}");
            Output.WriteLine($"VerifyConnectionTimeout: {connectionTimeout}");
            Output.WriteLine($"VerifyOperationTimeout: {operationTImeout}");

            Assert.True(VerifyIpAddress(Client, ipAddress));
            Assert.True(VerifyPort(Client, port));
            Assert.True(VerifyUserName(Client, userName));
            Assert.True(VerifyPassword(Client, password));
            Assert.True(VerifyConnectionTimeout(Client, connectionTimeout));
            Assert.True(VerifyOperationTimeout(Client, operationTImeout));
        }

        public abstract bool VerifyIpAddress(T client, string ipAddress);
        public abstract bool VerifyPort(T client, int port);
        public abstract bool VerifyUserName(T client, string userName);
        public abstract bool VerifyPassword(T client, string password);
        public abstract bool VerifyConnectionTimeout(T client, int connectionTimeout);
        public abstract bool VerifyOperationTimeout(T client, int operationTimeout);

        public virtual void Dispose()
        {
            if (Client is IDisposable d)
                d.Dispose();
        }
    }
}
